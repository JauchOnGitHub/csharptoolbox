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
      public override bool OnSimStart(object data)
      {
         string workingfolder = @"..\0.Data\Boundary.Conditions\";
         string outputfolder = @"..\..\Run.1\general.data\boundary.conditions\";
         MohidRunEngineData mre = (MohidRunEngineData)data;

         //Extract date from original 2010 MM5 file
         ExternalApp app = new ExternalApp();

         Dictionary<String, String> replace_list = new Dictionary<string, string>();
         replace_list.Add("<<start>>", mre.sim.Start.ToString("yyyy M d H m s"));
         replace_list.Add("<<end>>", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
         replace_list.Add("<<input>>", @"U:\Aplica\MyWater\Tamega\MohidLand\EVTPComparison\0.Data\Boundary.Conditions\MM5-D3-Portugal-2010.hdf5");
         replace_list.Add("<<output>>", "mm5extracted.hdf5");

         TextFile.Replace(workingfolder + "hdf5extractor.template", workingfolder + "hdf5extractor.dat", ref replace_list);

         app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
         app.Wait = true;
         app.WorkingDirectory = workingfolder;
         app.Executable = workingfolder + "HDF5Extractor.x64.omp.d.exe";

         try
         {
            if (!app.Run()) throw new Exception("Run Failed. Unknown error.");
         }
         catch (Exception ex)
         {
            Console.WriteLine("");
            Console.WriteLine("[OnSimStart-Extract] Erro detectado. Mensagem de erro:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Start  : {0}", mre.sim.Start.ToString("yyyy M d H m s"));
            Console.WriteLine("End    : {0}", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
            Console.WriteLine("");
            return false;
         }

         //Convert extracted HDF to project grid data

         replace_list.Clear();
         replace_list.Add("<<input>>", "mm5extracted.hdf5");
         replace_list.Add("<<input.grid>>", "MM5-D3-Portugal.dat");
         replace_list.Add("<<output>>", outputfolder + "mm5.hdf5");
         replace_list.Add("<<output.grid>>", "dem.dat");

         TextFile.Replace(workingfolder + "converttohdf5action.template", workingfolder + "converttohdf5action.dat", ref replace_list);

         app.Executable = workingfolder + "Convert2HDF.x64.omp.d.exe";
         try
         {
            if (!app.Run()) throw new Exception("Run Failed. Unknown error.");
         }
         catch (Exception ex)
         {
            Console.WriteLine("");
            Console.WriteLine("[OnSimStart-Convert] Erro detectado. Mensagem de erro:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Start  : {0}", mre.sim.Start.ToString("yyyy M d H m s"));
            Console.WriteLine("End    : {0}", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
            Console.WriteLine("");
            return false;
         }

         return true;
      }

      public override bool OnSimEnd(object data)
      {
         //Copy result files
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath mm5 = new FilePath(mre.sim.SimDirectory.Path + @"general.data\boundary.conditions\");
         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);

         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;
         if (!FileTools.CopyFile(mre.resFolder, store, "basin.refevtp.hdf5", Files.CopyOptions.OVERWRIGHT)) return false;
         if (!FileTools.CopyFile(mm5, store, "mm5.hdf5", Files.CopyOptions.OVERWRIGHT)) return false;

         return true;
      }

      public override bool OnEnd(object data)
      {         
         HDFGlue tool = new HDFGlue();
         tool.AppName = "convert.2.hdf5.exe";
         tool.AppPath = @"..\Run.1\convert\";
         tool.WorkingDirectory = @"..\Run.1\convert\";

         bool res = true;
         MohidRunEngineData mre = (MohidRunEngineData)data;
         string cumulativeFolder = @"..\cumulative\";

         tool.Output = cumulativeFolder + "basin.refevtp.hdf5";
         tool.Is3DFile = false;
         tool.FilesToGlue.Clear();

         System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(mre.storeFolder.Path);
         foreach (System.IO.DirectoryInfo g in dir.GetDirectories())
         {
           
            if (System.IO.File.Exists(g.FullName + "\\basin.refevtp.hdf5"))
               tool.FilesToGlue.Add(g.FullName + "\\basin.refevtp.hdf5");
         }         
         if (tool.FilesToGlue.Count > 0)
            if (tool.Glue() != 0) res = false;
         return res;
      }
   }
}
