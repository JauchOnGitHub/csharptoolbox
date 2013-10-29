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

//for run 2

namespace Mohid
{
   public class JoseNuncioBarrosaMM5Exporter2 : BCIMohidSim
   {
      protected FilePath boundaryconditions;
      protected FilePath mm5Path;

      public JoseNuncioBarrosaMM5Exporter2()
      {
         mm5Path = new FilePath();
      }

      public override bool OnSimStart(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         //bool result = true;
         DateTime startsim;
         DateTime endsim;
         string dateStr;
         DateTime simStart = mre.sim.Start;
         int counter;

         //First is required the glue of two files
         HDFGlue tool = new HDFGlue();

         tool.AppName = "ConvertToHdf5_release_single.exe";
         tool.GlueInpuFile = "glue.2.dat";
         tool.Arguments = "-c glue.2.dat";
         tool.AppPath = @"..\tools\Convert 2 HDF\";
         tool.WorkingDirectory = @"..\tools\Convert 2 HDF\";
         tool.Output = @"..\" + mre.sim.SimDirectory.Path + @"local data\mm5.glued.hdf5";
         tool.Is3DFile = false;

         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);

         string file_to_glue, path;
         int tentatives;
         bool must_find_another;
         bool file_exist;

         for (counter = 0; counter < 5; counter++)
         {
            tentatives = 3;
            must_find_another = true;

            while (must_find_another)
            {
               dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
               file_to_glue = @"D3_" + dateStr + ".hdf5";
               path = @"Q:\";

               Console.WriteLine("Looking for file: {0}", file_to_glue);

               file_exist = System.IO.File.Exists(path + file_to_glue);
               if (file_exist)
               {
                  System.IO.FileInfo fi = new System.IO.FileInfo(path + file_to_glue);
                  if  (fi.Length < 2000000)
                     file_exist = false;
               }

               if (file_exist)
               {
                  Console.WriteLine("File exists");
                  must_find_another = false;
                  tool.FilesToGlue.Add(path + file_to_glue);

                  if (counter == 0)
                  {
                     if (tentatives < 3)
                     {
                        startsim = simStart.AddHours(1);
                        endsim = startsim.AddHours(5);
                     }
                     else
                     {
                        startsim = endsim.AddHours(1);
                        endsim = startsim.AddHours(5);
                     }
                  }
                  else
                  {
                     startsim = endsim.AddHours(1);
                     endsim = startsim.AddHours(5);
                  }
               }
               else
               {
                  tentatives--;

                  if (tentatives == 0)
                  {
                     Console.WriteLine("Reach max tentatives to find a valid MM5 file.");
                     return false;
                  }

                  if (counter == 0)
                  {
                     startsim = startsim.AddHours(-6);
                     endsim = startsim.AddHours(5);
                  }
                  else
                  {
                     startsim = endsim.AddHours(1);
                     endsim = startsim.AddHours(5);
                  }
               }
            }
         }

         tool.ThrowExceptionOnError = true;
         if (tool.Glue() != 0)
            return false;

         //Console.WriteLine (mre.sim.SimDirectory.Path + @"templates\tools\interpolate.template");

         FileName template = new FileName(mre.sim.SimDirectory.Path + @"templates\tools\interpolate.template");
         FileName griddata = new FileName(mre.sim.SimDirectory.Path + @"templates\tools\grid.data.dat");

         //Now, do the "extraction"
         if (!FileTools.CopyFile(template, new FileName(@"..\tools\convert 2 hdf\interpolate.dat"), CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(griddata, new FileName(@"..\tools\convert 2 hdf\grid.data.dat"), CopyOptions.OVERWRIGHT))
            return false;

         ExternalApp app = new ExternalApp();

         app.Executable = @"..\tools\convert 2 hdf\ConvertToHdf5_release_double.exe";
         app.Arguments = "-c interpolate.dat";
         app.WorkingDirectory = @"..\tools\convert 2 hdf\";
         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
         app.TextToCheck = "successfully terminated";
         app.SearchTextOrder = SearchTextOrder.FROMEND;
         app.Wait = true;

         if (!app.Run())
            return false;

         //if (!FileTools.CopyFile(new FileName(@"..\tools\convert 2 hdf\meteo.mm5.hdf5"), new FileName(mre.sim.SimDirectory.Path + @"local data\meteo.mm5.hdf5"), CopyOptions.OVERWRIGHT))
         //   return false;

         return true;
      }

      public override bool OnSimEnd(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(new FilePath(mre.sim.SimDirectory.Path + "local data"), store, "*.*", CopyOptions.OVERWRIGHT))
            return false;
         return FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT);
      }
   }
}

