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
   public class MyWaterTamegaEVTPComparison : BCIMohidSim
   {
      protected FilePath boundaryconditions;
      protected FilePath mm5Path;

      public MyWaterTamegaEVTPComparison()
      {
         mm5Path = new FilePath();
      }

      public override bool OnSimStart(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         if (!GenerateMeteoFiles(mre.sim.Start))
            return false;

         return true;
      }

      protected bool GenerateMeteoFiles(DateTime simStartDate)
      {
         //bool result = true;
         DateTime startsim;
         DateTime endsim;
         string dateStr;
         DateTime simStart = simStartDate.AddSeconds(1);
         int counter;
		   string file;
         bool found_file;
         System.IO.FileInfo fi;

         //First is required the glue of two files
         HDFGlue tool = new HDFGlue();
         tool.AppName = "glue.exe";
         tool.AppPath = @"..\tools\glue\";
         tool.WorkingDirectory = @"..\tools\glue\";
         tool.Output = @"..\..\5\local.data\boundary.conditions\mm5.glued.hdf5";
         tool.Is3DFile = false;
	 
         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);
		 
		   Console.WriteLine("Sim start: {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
		   Console.WriteLine("Generating Meteo file from {0} to {1}", startsim.ToString("yyyy/MM/dd HH:mm:ss"), startsim.AddHours(11).ToString("yyyy/MM/dd HH:mm:ss"));
		   Console.WriteLine("Gluing files...");
         for (counter = 0; counter < 57; counter++)
         {
            found_file = false;

            dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
			   file = @"H:\mm5_6h\" + startsim.ToString("yyyy") + @"\D3_" + dateStr + ".hdf5";
            if (System.IO.File.Exists(file))
            {
               fi = new System.IO.FileInfo(file);
               if (fi.Length > 2000000)
                  found_file = true;
            }

            if (!found_file && startsim.Year == 2008)
            {
               file = @"H:\mm5_6h\2008_ANL\D3_" + dateStr + ".hdf5";
               if (System.IO.File.Exists(file))
               {
                  fi = new System.IO.FileInfo(file);
                  if (fi.Length > 2000000)
                     found_file = true;
               }
            }
            
            if (!found_file)
            {
               file = @"\\DATACENTER\alexandria3\modelos\Meteo_IST\mm5_6h\D3_" + dateStr + ".hdf5";
               if (System.IO.File.Exists(file))
               {
                  fi = new System.IO.FileInfo(file);
                  if (fi.Length > 2000000)
                     found_file = true;
               }
            }

            if (!found_file)
            {
               Console.WriteLine("The file 'D3_{0}.hdf5' was not found.", dateStr);
               return false;
            }
            else
            {
               tool.FilesToGlue.Add(file);
               //Console.WriteLine("Adding {0} to the list of files to be glued.", @"H:\mm5_6h\" + startsim.ToString("yyyy") + @"\D3_" + dateStr + ".hdf5");
               startsim = endsim.AddHours(1);
               endsim = startsim.AddHours(5);
            }
         }

         tool.ThrowExceptionOnError = true;
         if (tool.Glue() != 0) return false;

         //Now, do the "extraction"
         if (!FileTools.CopyFile(new FileName(@"..\5\templates\tools\convert.to.HDF5.action.template"), new FileName(@"..\tools\interpolation\converttoHDF5action.dat"), CopyOptions.OVERWRIGHT))
            return false;

         ExternalApp app = new ExternalApp();

         app.Executable = @"..\tools\interpolation\interpolation.exe";
         app.WorkingDirectory = @"..\tools\interpolation\";
         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
         app.TextToCheck = "successfully terminated";
         app.SearchTextOrder = SearchTextOrder.FROMEND;
         app.Wait = true;
		   Console.WriteLine("Interpolating meteo file to model grid...");
         return app.Run();
      }

      public override bool OnSimEnd(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(new FilePath(mre.sim.SimDirectory.Path + @"local.data\boundary.conditions"), store, "meteo.hdf5", Files.CopyOptions.OVERWRIGHT))
            return false;
         return FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT);
      }
   }
}
