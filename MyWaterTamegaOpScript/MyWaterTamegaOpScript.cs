////DLLNAME: MohidToolboxCore
////DLLNAME: System

//using System;
//using System.Collections.Generic;
//using System.Text;
//using Mohid.Files;
//using Mohid.Configuration;
//using Mohid.Script;
//using Mohid.CommandArguments;
//using Mohid.Simulation;
//using Mohid.Core;
//using Mohid.Log;
//using Mohid.Software;
//using Mohid.HDF;

//namespace Mohid
//{
//   public class MyWaterTamegaOpScript : BCIMohidSim
//   {
//      private MohidRunEngineData mre;

//      #region METEO

//      private bool GenerateMeteoFilesToForecast()
//      {
//         DateTime simStartDate = mre.sim.Start;
//         ConfigNode root = mre.cfg.Root;
//         string toolsFolder = mre.sim.SimDirectory.Path + "..\\Tools\\";

         
//         System.Collections.Generic.Dictionary<string, string> toReplace = new Dictionary<string, string>();

//         toReplace["<<datetime>>"] = simStartDate.ToString("yyyyMMddHH");
//         TextFile.Replace(mre.sim.SimDirectory.Path + "Templates\\Tools\\ConvertToHDF5Action.template", toolsFolder + "ConvertToHDF\\ConvertToHDF5Action.dat", ref toReplace);

//         ExternalApp app = new ExternalApp();

//         app.Executable = toolsFolder + "ConvertToHDF\\convert.2.hdf5.interpolation.exe";
//         app.WorkingDirectory = toolsFolder + "ConvertToHDF\\";
//         app.CheckSuccessMethod = Mohid.Software.CheckSuccessMethod.DEFAULTOUTPUT;
//         app.TextToCheck = "successfully terminated";
//         app.SearchTextOrder = Mohid.Software.SearchTextOrder.FROMEND;
//         app.Wait = true;

//         return app.Run();
//      }

//      private bool GenerateAlternativeMeteoFilesToForecast()
//      {
//         DateTime simStartDate = mre.sim.Start;

//         DateTime startsim;
//         DateTime endsim;
//         string dateStr;
//         DateTime simStart = simStartDate.AddSeconds(1);
//         string toolsFolder = mre.sim.SimDirectory.Path + "..\\Tools\\";

//         //First is required the glue of two files
//         HDFGlue tool = new HDFGlue();
//         tool.AppName = "convert.2.hdf5.glue.exe";
//         tool.AppPath = toolsFolder + "ConvertToHDF\\";
//         tool.WorkingDirectory = toolsFolder + "ConvertToHDF\\";
//         //Console.WriteLine("AppPath: " + tool.AppPath);
//         tool.Output = "meteo_glued.hdf5";
//         tool.Is3DFile = false;

//         startsim = simStart.AddHours(-5);
//         endsim = startsim.AddHours(5);
//         dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
//         tool.FilesToGlue.Add("\\\\ftpserver\\ftp.mohid.com\\LocalUser\\meteoIST\\mm5_6h\\D3_" + dateStr + ".hdf5");
//         startsim = endsim.AddHours(1);
//         endsim = startsim.AddHours(5);
//         dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
//         tool.FilesToGlue.Add("\\\\ftpserver\\ftp.mohid.com\\LocalUser\\meteoIST\\mm5_6h\\D3_" + dateStr + ".hdf5");
//         tool.ThrowExceptionOnError = true;
//         if (tool.Glue() != 0) return false;

//         //Now, do the "extraction"
//         if (Mohid.DiskManager.Tools.CopyFile(simFolder + "Templates\\Tools\\ConvertToHDF5Action-2.template", toolsFolder + "ConvertToHDF\\ConvertToHDF5Action.dat", true) != 0) return false;

//         ExternalApp app = new ExternalApp();


//         app.Options.Path = toolsFolder + "ConvertToHDF\\" + ConvertToHDF5ForInterpolation_EXE;
//         app.Options.WorkingDirectory = toolsFolder + "ConvertToHDF\\";
//         app.Options.CheckSuccessMethod = CheckSuccessMethod.DefaultOutput;
//         app.Options.TextToCheck = "successfully terminated";
//         app.Options.TextOrder = TextOrder.FromEnd;
//         app.Options.Wait = true;

//         return app.Run();
//      }
//      private bool GenerateMeteoFiles()
//      {
//         string filename = "\\\\ftpserver\\FileRecipient\\ftp_SIGEL\\mm5_3d\\D3_" + mre.sim.Start.ToString("yyyyMMddHH") + ".hdf5";
//         bool firstOption = true;

//         if (System.IO.File.Exists(filename))
//         {
//            System.IO.FileInfo objFileInfo = new System.IO.FileInfo(filename);
//            if (objFileInfo.Length < 100000000)
//               firstOption = false;
//         }
//         else
//            firstOption = false;


//         //Will force the second option for now
//         firstOption = false;

//         if (firstOption)
//            return GenerateMeteoFilesToForecast();
//         else
//            return GenerateAlternativeMeteoFilesToForecast();
//      }

//      #endregion METEO

//      public override bool OnSimStart(object data)
//      {
//         string workingfolder = @"..\0.Data\Boundary.Conditions\";
//         string outputfolder = @"..\..\Run.1\general.data\boundary.conditions\";
//         mre = (MohidRunEngineData)data;            

         

//         //Extract date from original 2010 MM5 file
//         ExternalApp app = new ExternalApp();

//         Dictionary<String, String> replace_list = new Dictionary<string, string>();
//         replace_list.Add("<<start>>", mre.sim.Start.ToString("yyyy M d H m s"));
//         replace_list.Add("<<end>>", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
//         replace_list.Add("<<input>>", "MM5-D3-Portugal-2010.hdf5");
//         replace_list.Add("<<output>>", "mm5extracted.hdf5");

//         TextFile.Replace(workingfolder + "hdf5extractor.template", workingfolder + "hdf5extractor.dat", ref replace_list);

//         app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
//         app.Wait = true;
//         app.WorkingDirectory = workingfolder;
//         app.Executable = workingfolder + "HDF5Extractor.x64.omp.d.exe";

//         try
//         {
//            if (!app.Run()) throw new Exception("Run Failed. Unknown error.");
//         }
//         catch (Exception ex)
//         {
//            Console.WriteLine("");
//            Console.WriteLine("[OnSimStart-Extract] Erro detectado. Mensagem de erro:");
//            Console.WriteLine(ex.Message);
//            Console.WriteLine("Start  : {0}", mre.sim.Start.ToString("yyyy M d H m s"));
//            Console.WriteLine("End    : {0}", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
//            Console.WriteLine("");
//            return false;
//         }

//         //Convert extracted HDF to project grid data

//         replace_list.Clear();
//         replace_list.Add("<<input>>", "mm5extracted.hdf5");
//         replace_list.Add("<<input.grid>>", "MM5-D3-Portugal.dat");
//         replace_list.Add("<<output>>", outputfolder + "mm5.hdf5");
//         replace_list.Add("<<output.grid>>", "project-griddata.dat");

//         TextFile.Replace(workingfolder + "converttohdf5action.template", workingfolder + "converttohdf5action.dat", ref replace_list);

//         app.Executable = workingfolder + "Convert2HDF.x64.omp.d.exe";
//         try
//         {
//            if (!app.Run()) throw new Exception("Run Failed. Unknown error.");
//         }
//         catch (Exception ex)
//         {
//            Console.WriteLine("");
//            Console.WriteLine("[OnSimStart-Convert] Erro detectado. Mensagem de erro:");
//            Console.WriteLine(ex.Message);
//            Console.WriteLine("Start  : {0}", mre.sim.Start.ToString("yyyy M d H m s"));
//            Console.WriteLine("End    : {0}", mre.sim.End.AddHours(1).ToString("yyyy M d H m s"));
//            Console.WriteLine("");
//            return false;
//         }

//         return true;
//      }

//      public override bool OnSimEnd(object data)
//      {
//         //Copy result files

//         MohidRunEngineData mre = (MohidRunEngineData)data;
         
//         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
//         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
//            return false;
//         return FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT);

//         //Join Timeseries


//         //Glue HDF's

//      }
//   }
//}
