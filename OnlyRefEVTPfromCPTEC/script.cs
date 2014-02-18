//DLLNAME: MohidToolboxCore
//DLLNAME: System

using System;
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
   public class OnlyRefEVTPfromCPTEC : BCIMohidSim
   {
      public override bool OnRunFail(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;
         Console.WriteLine("RUN FAILED.");
         Console.WriteLine("{0} : {1}", mre.sim.Status.ToString(), mre.sim.StatusMessage);
         return false;
      }

      public override bool OnSimStart(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         if (!GenerateMeteoFiles(mre.sim.Start))
            return false;

         return true;
      }

      protected bool GenerateMeteoFiles(DateTime start)
      {
         string filename = "\\\\ftpserver\\FileRecipient\\ftp_SIGEL\\mm5_3d\\D3_" + start.ToString("yyyyMMddHH") + ".hdf5";
         bool firstOption = true;

         if (System.IO.File.Exists(filename))
         {
            System.IO.FileInfo objFileInfo = new System.IO.FileInfo(filename);
            if (objFileInfo.Length < 100000000)
               firstOption = false;
         }
         else
            firstOption = false;

         //Will force the second option for now
         firstOption = false;

         if (firstOption)
            return GenerateMeteoFilesToForecast(start);
         else
            return GenerateAlternativeMeteoFilesToForecast(start);
      }

      public bool GenerateMeteoFilesToForecast(DateTime simStartDate)
      {
         System.Collections.Generic.Dictionary<string, string> toReplace = new Dictionary<string, string>();

         toReplace["<<datetime>>"] = simStartDate.ToString("yyyyMMddHH");
         TextFile.Replace(@"..\templates\tools\convertToHDF5Action.template", @"..\tools\ConvertToHDF\ConvertToHDF5Action.dat", ref toReplace);

         ExternalApp app = new ExternalApp();

         app.Executable = @"..\tools\ConvertToHDF\interpolation.exe";
         app.WorkingDirectory = @"..\tools\ConvertToHDF\";
         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
         app.TextToCheck = "successfully terminated";
         app.SearchTextOrder = SearchTextOrder.FROMEND;
         app.Wait = true;

         return app.Run();
      }

      protected bool GenerateAlternativeMeteoFilesToForecast(DateTime simStartDate)
      {
         //bool result = true;
         DateTime startsim;
         DateTime endsim;
         string dateStr;
         DateTime simStart = simStartDate.AddSeconds(1);

         char pd = System.IO.Path.DirectorySeparatorChar;

         //First is required the glue of two files
         HDFGlue tool = new HDFGlue();
         tool.AppName = "glue.exe";
         tool.AppPath = System.IO.Path.Combine("..", "tools", "glue");
         tool.WorkingDirectory = tool.AppPath;
         tool.Output = "meteo.glued.hdf5";
         tool.Is3DFile = false;

         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);
         dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
         tool.FilesToGlue.Add("\\\\ftpserver\\ftp.mohid.com\\LocalUser\\meteoIST\\mm5_6h\\D3_" + dateStr + ".hdf5");
         startsim = endsim.AddHours(1);
         endsim = startsim.AddHours(5);
         dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
         tool.FilesToGlue.Add("\\\\ftpserver\\ftp.mohid.com\\LocalUser\\meteoIST\\mm5_6h\\D3_" + dateStr + ".hdf5");
         tool.ThrowExceptionOnError = true;
         if (tool.Glue() != 0) return false;

         

         //Now, do the "extraction"
         if (!FileTools.CopyFile(new FileName(@".."+ pd + "templates" + pd + "tools" + pd + "ConvertToHDF5Action-2.template"), new FileName(@".." + pd + "tools" + pd + "interpolation" + pd + "ConvertToHDF5Action.dat"), CopyOptions.OVERWRIGHT))
            return false;

         ExternalApp app = new ExternalApp();

         app.Executable = @".." + pd + "tools" + pd + "interpolation" + pd + "interpolation.exe";
         app.WorkingDirectory = @".." + pd + "tools" + pd + "interpolation" + pd;
         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
         app.TextToCheck = "successfully terminated";
         app.SearchTextOrder = SearchTextOrder.FROMEND;
         app.Wait = true;

         return app.Run();
      }

      public override bool OnSimEnd(object data)
      {
         //Copy result files
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);

         FilePath toFTP = FileTools.CreateFolder("to.ftp", store);

         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(mre.sim.DataDirectory, store, "*.*", CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(new FilePath(mre.sim.SimDirectory.Path + @"general data\boundary conditions\"), store, "*.*", CopyOptions.OVERWRIGHT))
            return false;

         if (!FileTools.CopyFile(store, toFTP, "*.hdf5", Files.CopyOptions.OVERWRIGHT))
            return false;

         // if (!SendToFTP(mre))
         // return false;

         return true;
         //return GenerateCumulative(mre);
      }

      protected bool GlueFiles(HDFGlue glue, bool is3D, FilePath workingFolder, FilePath destinationFolder, FileName toGlue, MohidRunEngineData mre)
      {
         glue.Output = @"..\..\cumulative\" + toGlue.FullName;
         glue.Is3DFile = false;
         glue.FilesToGlue.Clear();

         //Console.WriteLine("Checking if '{0}' exists", destinationFolder.Path + toGlue.FullName);
         if (!System.IO.File.Exists(destinationFolder.Path + toGlue.FullName))
         {
            //if (!FileTools.CopyFile(toGlue, new FileName(destinationFolder.Path + toGlue.FullName), CopyOptions.OVERWRIGHT))
            //   return false;

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(mre.storeFolder.Path);
            foreach (System.IO.DirectoryInfo g in dir.GetDirectories())
            {
               if (System.IO.File.Exists(g.FullName + System.IO.Path.DirectorySeparatorChar + toGlue.FullName))
               {
                  //Console.WriteLine("File '{0}' exists", g.FullName + "\\" + toGlue.FullName);
                  glue.FilesToGlue.Add(g.FullName + System.IO.Path.DirectorySeparatorChar + toGlue.FullName);
               }
               else
               {
                  //Console.WriteLine("File '{0}' DO NOT exists", g.FullName + "\\" + toGlue.FullName);
               }
            }
            //Console.WriteLine("Found {0} files to GLUE.", glue.FilesToGlue.Count.ToString());
            if (glue.FilesToGlue.Count > 0)
               if (glue.Glue() != 0)
                  return false;

            return true;
         }

         if (!FileTools.CopyFile(new FileName(destinationFolder.Path + toGlue.FullName), new FileName(workingFolder.Path + toGlue.FullName), CopyOptions.OVERWRIGHT))
            return false;

         glue.FilesToGlue.Add(toGlue.FullName);
         glue.FilesToGlue.Add(".." + System.IO.Path.DirectorySeparatorChar + toGlue.FullPath);

         if (glue.Glue() != 0)
            return false;

         return true;
      }

      public bool GenerateCumulative(MohidRunEngineData mre)
      {
         HDFGlue tool = new HDFGlue();
         tool.AppName = "glue.exe";
         tool.AppPath = @"..\tools\glue\";
         tool.WorkingDirectory = @"..\tools\glue\";

         bool res = true;

         FilePath pathToGlue = new FilePath(mre.storeFolder.Path + mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"));
         FileName toGlue = new FileName();
         toGlue.Path = mre.storeFolder.Path + mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss");
         FilePath destinationFolder = new FilePath(@"..\cumulative\");
         FilePath workingFolder = new FilePath(tool.WorkingDirectory);

         toGlue.FullName = "basin.hdf5";
         if (!GlueFiles(tool, false, workingFolder, destinationFolder, toGlue, mre))
            return false;

         toGlue.FullName = "basin.refevtp.hdf5";
         Console.WriteLine("{0}", toGlue.FullPath);
         if (!GlueFiles(tool, false, workingFolder, destinationFolder, toGlue, mre))
            return false;

         toGlue.FullName = "basin.evtp.hdf5";
         Console.WriteLine("{0}", toGlue.FullPath);
         if (!GlueFiles(tool, false, workingFolder, destinationFolder, toGlue, mre))
            return false;

         //toGlue.FullName = "porous.media.hdf5";
         //Console.WriteLine("{0}", toGlue.FullPath);
         //if (!GlueFiles(tool, true, workingFolder, destinationFolder, toGlue, mre))
         //   return false;

         toGlue.FullName = "drainage.network.hdf5";
         Console.WriteLine("{0}", toGlue.FullPath);
         if (!GlueFiles(tool, false, workingFolder, destinationFolder, toGlue, mre))
            return false;

         toGlue.FullName = "runoff.hdf5";
         Console.WriteLine("{0}", toGlue.FullPath);
         if (!GlueFiles(tool, false, workingFolder, destinationFolder, toGlue, mre))
            return false;

         toGlue.FullName = "atmosphere.hdf5";
         Console.WriteLine("{0}", toGlue.FullPath);
         if (!GlueFiles(tool, false, workingFolder, destinationFolder, toGlue, mre))
            return false;

         //tool.FilesToGlue.Clear();
         //tool.Output = cumulativeFolder + "atmosphere.hdf5";
         //foreach (System.IO.DirectoryInfo g in dir.GetDirectories())
         //{
         //   if (System.IO.File.Exists(g.FullName + "\\atmosphere.hdf5"))
         //      tool.FilesToGlue.Add(g.FullName + "\\atmosphere.hdf5");
         //}
         //if (tool.FilesToGlue.Count > 0)
         //   if (tool.Glue() != 0)
         //      res = false;

         if (!SendToFTPOnEnd(mre))
            res = false;

         return res;
      }

      //public override bool OnEnd(object data)
      //{
      //   MohidRunEngineData mre = (MohidRunEngineData)data;
      //   return SendToFTPOnEnd(mre);
      //}

      protected bool SendToFTP(MohidRunEngineData mre)
      {
         bool res = true;

         Dictionary<string, string> toReplace = new Dictionary<string, string>();

         string folderName = mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss");

         try
         {
            toReplace["<<folder>>"] = folderName;
            TextFile.Replace(@"..\templates\tools\ref-evtp.template", @"..\tools\ftp\ftp.dat", ref toReplace);

            ExternalApp app = new ExternalApp();

            app.Executable = "ftp";
            app.WorkingDirectory = @"..\store\" + folderName + @"\To.FTP";
            app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
            app.Wait = true;
            app.Arguments = "-n -s:" + @"..\..\..\tools\ftp\ftp.dat ftp.mywater-fp7.eu";

            res = app.Run();
            if (!res)
               return false;
         }
         catch
         {
            return false;
         }

         return res;
      }

      protected bool SendToFTPOnEnd(MohidRunEngineData mre)
      {
         bool res = true;

         Dictionary<string, string> toReplace = new Dictionary<string, string>();

         try
         {
            ExternalApp app = new ExternalApp();

            FileTools.CopyFile(new FileName(@"..\templates\Tools\ref-evtp-cumulative.template"), new FileName(@"..\tools\ftp\ftp.dat"), CopyOptions.OVERWRIGHT);

            app.Executable = "ftp";
            app.WorkingDirectory = @"..\Cumulative";
            app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
            app.Wait = true;
            app.Arguments = "-n -s:" + @"..\tools\ftp\ftp.dat ftp.mywater-fp7.eu";

            res = app.Run();

         }
         catch
         {
            return false;
         }


         return res;
      }
   }
}
