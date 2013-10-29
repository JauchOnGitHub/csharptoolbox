//DLLNAME: MohidToolboxCore
//DLLNAME: System

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Simulation;
using Mohid.Core;
using Mohid.Log;
using Mohid.Software;
using Mohid.HDF;

namespace Mohid
{
   public class TamegaScript : BCIMohidSim
   {
      protected FilePath boundaryconditions;
      protected FilePath mm5Path;
      protected FileName orig;

      public TamegaScript()
      {
         orig = new FileName();
         mm5Path = new FilePath();
      }

      public override bool OnSimStart(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         if (!CopyMeteoFiles(mre))
         {
            Console.WriteLine("Was not possible to copy the meteo files.");
            return false;
         }

         return true;
      }

      protected bool CopyMeteoFiles(MohidRunEngineData mre)
      {

         FileName dest = new FileName();

         orig.FullPath = @"..\general.data\boundary.conditions\" + mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss") + @"\meteo.hdf5";
         dest.FullPath = mre.sim.SimDirectory.Path + @"local.data\boundary.conditions\meteo.hdf5";
         if (!Directory.Exists(orig.Path))
            Directory.CreateDirectory(orig.Path);

         if (File.Exists(orig.FullPath))
         {
            FileTools.CopyFile(orig, dest, CopyOptions.OVERWRIGHT);
         }
         else
         {
            if (!GenerateMeteoFiles(mre))
            {
               Console.WriteLine("Was not possible to create the meteo files.");
               return false;
            }
            FileTools.CopyFile(orig, dest, CopyOptions.OVERWRIGHT);
         }

         return true;
      }

      protected bool GenerateMeteoFiles(MohidRunEngineData mre)
      {
         DateTime simStartDate = mre.sim.Start;
         DateTime startsim;
         DateTime endsim;
         string dateStr;
         DateTime simStart = simStartDate.AddSeconds(1);
         int counter;
         string file, outputPath, outputName, tempFolder;
         bool found_file;
         System.IO.FileInfo fi;
         HDFGlue glueTool = new HDFGlue();
         ExternalApp interpolateTool = new ExternalApp();

         ConfigNode node = mre.cfg.Root.ChildNodes.Find(FindScriptBlocks);
         if (node == null)
         {
            Console.WriteLine("Node 'script.data' was not found in the config file.");
            return false;
         }

         //This will ensure that the first hour of simulation is included in the meteo file.
         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);

         Console.WriteLine("Sim start: {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Generating Meteo file from {0} to {1}", startsim.ToString("yyyy/MM/dd HH:mm:ss"), mre.sim.End.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim lenght: {0}", mre.sim.SimLenght);

         //Get some important input from the script block in config file
         outputPath = node["glue.output.path", @"..\" + mre.sim.SimDirectory.Path + @"local.data\boundary.conditions\"].AsString();
         outputName = node["glue.output.name", "meteo.hdf5"].AsString();
         tempFolder = node["interpolation.folder", @".\temp\"].AsString();

         //First, find the required files.
         int numberOfFiles = (int)(mre.sim.SimLenght * 4 + 1.0);
         Console.WriteLine("Expected number of 6 hours files to look: {0}", numberOfFiles);
         Console.WriteLine("Folder where meteo files are: {0}", node["meteo.folder", @"H:\"].AsString());
         Console.WriteLine("");
         Console.WriteLine("Looking for files...");
         Console.WriteLine("");

         int maxSkips = node["max.skips", 10].AsInt();
         int numberOfSkips = 0;
         List<string> requiredFiles = new List<string>();

         for (counter = 0; counter < numberOfFiles; counter++)
         {
            found_file = false;

            dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
            file = node["meteo.folder", @"H:\"].AsString() + @"D3_" + dateStr + ".hdf5";

            if (System.IO.File.Exists(file))
            {
               fi = new System.IO.FileInfo(file);
               if (fi.Length > 2000000)
               {
                  found_file = true;
                  requiredFiles.Add(file);
               }
            }

            if (!found_file)
            {
               Console.WriteLine("File '{0}' was not found.", file);

               if (counter + 1 == numberOfFiles)
               {
                  counter--;
                  numberOfSkips++;
                  if (numberOfSkips > maxSkips)
                  {
                     Console.WriteLine("Max number of skips reached during meteo creation file.");
                     return false;
                  }
               }
            }

            startsim = endsim.AddHours(1);
            endsim = startsim.AddHours(5);
         }

         //Now the files were found, they will be interpolated to the model's grid
         Console.WriteLine("Interpolating meteo file to model grid...");                  
         Dictionary<string, string> replace_list = new Dictionary<string, string>();
         FileName f_name = new FileName();
         string path = node["glue.input.folder", @"..\interpolation\temp\"].AsString();
         bool skipInterpolation = node["skip.interpolation", false].AsBool();

         foreach (string f in requiredFiles)
         {
            f_name.FullPath = f;
            glueTool.FilesToGlue.Add(path + f_name.FullName);

            if (!skipInterpolation)
            {

               replace_list["<<input.hdf>>"] = f;
               replace_list["<<father.grid>>"] = node["father.grid", @"..\..\general.data\digital.terrain\mm5.dat"].AsString();
               replace_list["<<output.hdf>>"] = tempFolder + f_name.FullName;
               replace_list["<<model.grid>>"] = node["model.grid", @"..\..\general.data\digital.terrain\grid.dat"].AsString();

               string template = node["template", mre.sim.SimDirectory.Path + @"templates\tools\convert.to.HDF5.action.template"].AsString();
               string action = node["action.file", @"..\tools\interpolation\converttoHDF5action.dat"].AsString();

               TextFile.Replace(template, action, ref replace_list);

               interpolateTool.WorkingDirectory = node["interpolation.working.folder", @"..\tools\interpolation\"].AsString();
               interpolateTool.Executable = node["interpolation.exe", @"..\tools\interpolation\interpolation.exe"].AsString();
               interpolateTool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
               interpolateTool.TextToCheck = "successfully terminated";
               interpolateTool.SearchTextOrder = SearchTextOrder.FROMEND;
               interpolateTool.Wait = true;

               bool result = interpolateTool.Run();
               if (!result)
               {
                  Console.WriteLine("Interpolation failed.");
                  return false;
               }
            }
         }

         //Now the interpolated files are glued        
         glueTool.AppName = node["glue.exe.name", "glue.exe"].AsString();
         glueTool.AppPath = node["glue.exe.path", @"..\tools\glue\"].AsString();
         glueTool.WorkingDirectory = node["glue.working.folder", @"..\tools\glue\"].AsString();         
         glueTool.Output = outputPath + outputName;
         glueTool.Is3DFile = false;
         glueTool.ThrowExceptionOnError = true;

         Console.WriteLine("Gluing files...");
         
         if (glueTool.Glue() != 0)
         {
            Console.WriteLine("Glue failed.");
            return false;
         }

         //Saves the created meteo file in the folder
         if (!FileTools.CopyFile(node["local.boundary.folder", @"..\actual\local.data\boundary.conditions\"].AsFilePath(), new FilePath(orig.Path), "meteo.hdf5", Files.CopyOptions.OVERWRIGHT))
         {
            Console.WriteLine("Was not possible to copy the meteo file to the meteo storage folder.");
            return false;
         }

         //Delete all the files in the temp directory
         try
         {
            System.IO.DirectoryInfo directory = new DirectoryInfo(@"..\tools\interpolation\temp\");
            foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
               fileToDelete.Delete();
         }
         catch (Exception ex)
         {
            Console.WriteLine("Was not possible to empty the temp directory due to an exception.");
            Console.WriteLine("Message returned was: {0}", ex.Message);
         }

         return true;
      }

      public override bool OnSimEnd(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
         {
            Console.WriteLine("Was not possible to copy the 'fin' files to the init folder.");
            return false;
         }

         return FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT);
      }

      protected bool FindScriptBlocks(ConfigNode toMatch)
      {
         if (toMatch.Name == "script.data")
            return true;
         return false;
      }
   }
}

