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

namespace Mohid
{
   public class MyWaterTamegaEVTPComparison : BCIMohidSim
   {
      protected FilePath boundaryconditions;
      protected FilePath mm5Path;

      public MyWaterTamegaEVTPComparison()
      {
         boundaryconditions = new FilePath(@"..\..\Run.4\general.data\boundary.conditions\");
         mm5Path = new FilePath();
      }

      public override bool OnSimStart(object data)
      {
         //string workingfolder = @"..\0.Data\Boundary.Conditions\";
         //string outputfolder = @"..\..\Run.4\general.data\boundary.conditions\";
	      MohidRunEngineData mre = (MohidRunEngineData)data;

         mm5Path.Path = @"..\..\Run.3\model.results\" + mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss");
         FileTools.CopyFile(mm5Path, boundaryconditions, "mm5.hdf5", CopyOptions.OVERWRIGHT);

         ////Extract date from original 2010 MM5 file
         //ExternalApp app = new ExternalApp();

         //Dictionary<String, String> replace_list = new Dictionary<string, string>();
         //replace_list.Add("<<start>>", mre.sim.Start.ToString("yyyy M d H m s"));
         //replace_list.Add("<<end>>", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
         //replace_list.Add("<<input>>", "MM5-D3-Portugal-2010.hdf5");
         //replace_list.Add("<<output>>", "mm5extracted.hdf5");

         //TextFile.Replace(workingfolder + "hdf5extractor.template", workingfolder + "hdf5extractor.dat", ref replace_list);

         //app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
         //app.Wait = true;
         //app.WorkingDirectory = workingfolder;
         //app.Executable = workingfolder + "HDF5Extractor.x64.omp.d.exe";

         //try
         //{
         //   if (!app.Run()) throw new Exception("Run Failed. Unknown error.");
         //}
         //catch (Exception ex)
         //{
         //   Console.WriteLine("");
         //   Console.WriteLine("[OnSimStart-Extract] Erro detectado. Mensagem de erro:");
         //   Console.WriteLine(ex.Message);
         //   Console.WriteLine("Start  : {0}", mre.sim.Start.ToString("yyyy M d H m s"));
         //   Console.WriteLine("End    : {0}", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
         //   Console.WriteLine("");
         //   return false;
         //}		  
		 
         ////Convert extracted HDF to project grid data
         
         //replace_list.Clear();
         //replace_list.Add("<<input>>", "mm5extracted.hdf5");
         //replace_list.Add("<<input.grid>>", "MM5-D3-Portugal.dat");
         ////replace_list.Add("<<output>>", outputfolder + "mm5.hdf5");
         //replace_list.Add("<<output>>", boundaryconditions.Path + "mm5.hdf5");
         //replace_list.Add("<<output.grid>>", "project-griddata.dat");

         //TextFile.Replace(workingfolder + "converttohdf5action.template", workingfolder + "converttohdf5action.dat", ref replace_list);

         //app.Executable = workingfolder + "Convert2HDF.x64.omp.d.exe";
         //try
         //{
         //   if (!app.Run()) throw new Exception("Run Failed. Unknown error.");
         //}
         //catch (Exception ex)
         //{
         //   Console.WriteLine("");
         //   Console.WriteLine("[OnSimStart-Convert] Erro detectado. Mensagem de erro:");
         //   Console.WriteLine(ex.Message);
         //   Console.WriteLine("Start  : {0}", mre.sim.Start.ToString("yyyy M d H m s"));
         //   Console.WriteLine("End    : {0}", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
         //   Console.WriteLine("");
         //   return false;
         //}		  

         return true;
      }

      public override bool OnSimEnd(object data)
      {
         //Copy result files
         //FilePath outputfolder = new FilePath(@"..\Run.4\general.data\boundary.conditions\");
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;

         //if (!FileTools.CopyFile(outputfolder, store, "*.hdf5", CopyOptions.OVERWRIGHT))
         if (!FileTools.CopyFile(boundaryconditions, store, "*.hdf5", CopyOptions.OVERWRIGHT))
            return false;

         return FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT);

         //Join Timeseries


         //Glue HDF's

      }
   }
}
