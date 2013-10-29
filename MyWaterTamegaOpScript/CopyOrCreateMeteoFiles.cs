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
   public class MyWaterTamega : BCIMohidSim
   {
      protected FilePath boundaryconditions;
      protected FilePath mm5Path;
      protected FileName orig;

      public MyWaterTamega()
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
         string file, outputPath, outputName;
         bool found_file;
         System.IO.FileInfo fi;


         ConfigNode node = mre.cfg.Root.ChildNodes.Find(FindScriptBlocks);
         if (node == null)
         {
            Console.WriteLine("Node 'script.data' was not found in the config file.");
            return false;
         }

         //First is required the glue of the files
         HDFGlue tool = new HDFGlue();
         tool.AppName = node["glue.exe.name", "glue.exe"].AsString();
         tool.AppPath = node["glue.exe.path", @"..\tools\glue\"].AsString();
         tool.WorkingDirectory = node["glue.working.folder", @"..\tools\glue\"].AsString();
         outputPath = node["glue.output.path", @"..\" + mre.sim.SimDirectory.Path + @"local.data\boundary.conditions\"].AsString();
         outputName = node["glue.output.name", "mm5.glued.hdf5"].AsString();
         tool.Output = outputPath + outputName;
         tool.Is3DFile = false;

         //This will ensure that the first hour of simulation is included in the meteo file.
         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);
         int maxSkips = node["max.skips", 10].AsInt();
         int numberOfSkips = 0;

         Console.WriteLine("Sim start: {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Generating Meteo file from {0} to {1}", startsim.ToString("yyyy/MM/dd HH:mm:ss"), mre.sim.End.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim lenght: {0}", mre.sim.SimLenght);

         int numberOfFiles = (int)(mre.sim.SimLenght * 4 + 1.0);
         Console.WriteLine("Expected number of 6 hours files to look: {0}", numberOfFiles);
         Console.WriteLine("Folder where meteo files are: {0}", node["meteo.folder", @"H:\"].AsString());
         Console.WriteLine("");
         Console.WriteLine("Looking for files...");
         Console.WriteLine("");
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
                  tool.FilesToGlue.Add(file);
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

         Console.WriteLine("Gluing files...");

         tool.ThrowExceptionOnError = true;
         if (tool.Glue() != 0)
         {
            Console.WriteLine("Glue failed.");
            return false;
         }

         Dictionary<string, string> replace_list = new Dictionary<string, string>();
         replace_list["<<glued>>"] = outputPath;
         replace_list["<<father>>"] = node["father.grid", @"..\..\general.data\digital.terrain\"].AsString();
         replace_list["<<dest>>"] = node["hdf.output.path", @"..\..\general.data\boundary.conditions\"].AsString() + mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss") + @"\";
         replace_list["<<model>>"] = node["model.grid", @"..\..\general.data\digital.terrain\"].AsString();

         string template = node["template", mre.sim.SimDirectory.Path + @"templates\tools\convert.to.HDF5.action.template"].AsString();
         string action = node["action.file",  @"..\tools\interpolation\converttoHDF5action.dat"].AsString();

         TextFile.Replace(template, action, ref replace_list);

         ExternalApp app = new ExternalApp();

         app.WorkingDirectory = node["interpolation.working.folder", @"..\tools\interpolation\"].AsString();
         app.Executable = node["interpolation.exe", @"..\tools\interpolation\interpolation.exe"].AsString();         
         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
         app.TextToCheck = "successfully terminated";
         app.SearchTextOrder = SearchTextOrder.FROMEND;
         app.Wait = true;
         Console.WriteLine("Interpolating meteo file to model grid...");
         bool result = app.Run();
         if (!result)
         {
            Console.WriteLine("Interpolation failed.");
            return false;
         }

         //Saves the created meteo file in the folder
         if (!FileTools.CopyFile(new FilePath(replace_list["<<dest>>"]), new FilePath(orig.Path), "meteo.hdf5", Files.CopyOptions.OVERWRIGHT))
         {
            Console.WriteLine("Was not possible to copy the meteo file to the meteo storage folder.");
            return false;
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

