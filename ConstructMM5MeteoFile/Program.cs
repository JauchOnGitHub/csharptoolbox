using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Text;
using Mohid;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Core;
using Mohid.Log;
using Mohid.Software;
using Mohid.HDF;
using Mohid.WebMail;


namespace Mohid
{
   class Program
   {      

      static void Main(string[] args)
      {
         OPTamegaScript script = new OPTamegaScript();
         CmdArgs cmdArgs = null;
         Exception e = null;
         bool sendMail = false;
         bool sendSMS = false;
         bool verbose = false;

         try
         {
            Setup.StandartSetup();
            cmdArgs = new CmdArgs(args);

            if (cmdArgs.HasParameter("mailcfg"))
               sendMail = true;

            if (cmdArgs.HasParameter("smscfg"))
               sendSMS = true;

            if (cmdArgs.HasOption("verbose"))
               verbose = true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Exception raised during initialization: {0}", ex.Message);
            return;
         }

         try
         {
            if (Run(cmdArgs, verbose))
               throw new Exception("Run failed.");
         }
         catch (Exception ex)
         {
            e = ex;
         }

         if (sendMail)
         {
            try
            {
               Config cfg = new Config(cmdArgs.Parameter("mailcfg"));
               if (!cfg.Load())
               {
                  Console.WriteLine("[{0}] Was not possible to load the mail configuration file '{1}'", DateTime.Now, cmdArgs.Parameter("mailcfg"));
               }
               else
               {
                  MailSender ms = new MailSender();

                  string sendTo,
                         header,
                         message;

                  if (e != null)
                  {
                     header = "[ERROR] " + cfg.Root["header", "MohidRun Report"].AsString();
                     sendTo = "sendto.onerror";
                     message = cfg.Root["message", "Mohid Run Report"].AsString() + Environment.NewLine;
                     message += "Exception raised: " + Environment.NewLine;
                     message += e.Message;
                  }
                  else
                  {
                     header = "[SUCCESS] " + cfg.Root["header", "MohidRun Report"].AsString();
                     sendTo = "sendto.onsuccess";
                     message = cfg.Root["message", "Mohid Run Report"].AsString();
                  }

                  ms.SetFrom(cfg.Root["from"].AsString(), cfg.Root["display", cfg.Root["from"].AsString()].AsString());
                  ms.User = cfg.Root["user", "mohid.operational@gmail.com"].AsString();
                  ms.Password = cfg.Root["pass", "MohidOperationalISTMARETEC2011"].AsString();
                  ms.SetMessage(message, header);
                  ms.Host = cfg.Root["host", "smtp.gmail.com"].AsString();
                  ms.Port = cfg.Root["port", 587].AsInt();
                  ms.EnableSSL = cfg.Root["enable.ssl", true].AsBool();

                  foreach (ConfigNode n in cfg.Root.ChildNodes.FindAll(delegate(ConfigNode node) { return (node.Name == sendTo || node.Name == "sendto"); }))
                  {
                     if (!(n["bcc", ""].AsString() == ""))
                        ms.AddBCC(n["bcc"].AsString(), n["display", n["bcc"].AsString()].AsString());
                     else if (!(n["cc", ""].AsString() == ""))
                        ms.AddCC(n["cc"].AsString(), n["display", n["cc"].AsString()].AsString());
                     else
                        ms.AddTo(n["to"].AsString(), n["display", n["to"].AsString()].AsString());
                  }


                  ms.SendMail();
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine("[{0}] Was not possible to send the mail. An EXCEPTION happened. The message returned was:", DateTime.Now);
               Console.WriteLine("{0}", ex.Message);
            }
         }

         if (sendSMS)
         {
         }
      }

      static bool Run(CmdArgs args, bool Verbose)
      {
         bool sim_run = true;
         MohidRunEngineData data = new MohidRunEngineData();
         data.args = args;
         OPTamegaScript op = new OPTamegaScript();

         try
         {
            if (Verbose) Console.Write("Loading Configuration...");
            LoadConfig(args, data);
            if (Verbose) Console.WriteLine("[OK]");

            if (Verbose) Console.Write("Running...");
            op.OnSimStart(data);
            if (Verbose) Console.WriteLine("[OK]");

         }
         catch (Exception ex)
         {
            if (Verbose)
            {
               Console.WriteLine("[FAILED]");
               Console.WriteLine("");
               Console.WriteLine("An EXCEPTION was raised. The message returned was:");
               Console.WriteLine(ex.Message);
               Console.WriteLine("");
            }
         }

         return sim_run;
      }

      static void LoadConfig(CmdArgs args, MohidRunEngineData data)
      {
         data.cfg = new Config();
         Config cfg = data.cfg;

         string configFile;
         if (args.HasParameter("simcfg"))
            configFile = args.Parameter("simcfg");
         else
            configFile = "sim.cfg";

         cfg.ConfigFile.FullPath = configFile;
         if (!cfg.Load())
            throw new Exception("Was not possible to load the configuration file '" + configFile + "'. " + cfg.ExceptionMessage);
      }

   }


   public class MohidRunEngineData
   {
      public Config cfg;
      public IMohidSim userInterface;
      public FilePath resFolder;
      public FilePath storeFolder;
      public DateTime endOfSimulation;
      public bool useEndOfSimulation;
      public LogEngine log;
      public bool changeTemplates;
      public DateTime runStart;
      public DateTime runEnd;
      public bool RestartFailedRun;
      public int simID;
      public int maxIterations;
      public string dateFormat;
      public CultureInfo provider;
      public FilePath oldFolder;
      public bool LastOperationResult;
      public CmdArgs args;
      public FileName logFileName;

      public MohidRunEngineData()
      {
         log = new LogEngine();
         dateFormat = "yyyy/MM/dd HH:mm:ss";
         provider = CultureInfo.InvariantCulture;
         LastOperationResult = true;
      }

   }

   public class OPTamegaScript
   {
      protected ConfigNode root;
      protected MohidRunEngineData mre;
      protected string storeFolder, 
                       storeFolderOld,
                       outputPath, 
                       outputName, 
                       tempFolder,
                       pathToFilesToGlue;
      protected bool practicesFromHDF;
      protected FilePath generalBoundary;
      protected Dictionary<string, string> replace_list;
      protected ExternalApp tool;
      protected List<string> filesToGlue;

      public OPTamegaScript()
      {
         root = null;
         mre = null;         
         storeFolder = "";
         storeFolderOld = "";
         outputPath = "";
         outputName = "";
         tempFolder = "";
         pathToFilesToGlue = "";
         generalBoundary = new FilePath();
         replace_list = new Dictionary<string, string>();
         tool = new ExternalApp();
         filesToGlue = new List<string>();
      }

      public bool OnSimStart(object data)
      {
         mre = (MohidRunEngineData)data;
         DateTime start, end;

         root = mre.cfg.Root.ChildNodes.Find(FindScriptBlocks);
         if (root == null)
         {
            Console.WriteLine("Block 'script.data' was not found in the config file.");
            return false;
         }

         start = root["start"].AsDateTime(mre.dateFormat);
         end = root["end"].AsDateTime(mre.dateFormat);

         //Get some important input from the script block in config file
         outputPath           = root["glue.output.path"].AsString();
         outputName           = root["glue.output.name", "meteo.hdf5"].AsString();
         tempFolder           = root["interpolation.folder", @".\temp\"].AsString();
         pathToFilesToGlue    = root["glue.input.folder", @"..\interpolation\temp\"].AsString();
         
         if (!GenerateMeteoFiles(start, end))
            return false;

         return true;
      }

      protected bool GenerateMeteoFiles(DateTime start, DateTime end)
      {
         DateTime simStartDate =start,
                  simEndDate = end,
                  simStart = simStartDate.AddSeconds(1),
                  startsim,
                  endsim;
         string dateStr;         
         int counter;
         string mm5NewFilesFolder;
         FileName file = new FileName();
         bool found_file = false;
         System.IO.FileInfo fi;        
         Dictionary<string, KeywordData> meteoFolderList;

         filesToGlue.Clear();


         if (root.NodeData.ContainsKey("new.mm5.folder")) mm5NewFilesFolder = root["new.mm5.folder"].AsFilePath().Path;
         else mm5NewFilesFolder = "";

         //--------------------------------------------------------------------------------------------------------------
         //1. Loads the list of folders to look for the meteo files.
         //--------------------------------------------------------------------------------------------------------------
         ConfigNode foldersListNode = root.ChildNodes.Find(FindFoldersListBlocks);
         if (foldersListNode == null)
         {
            //If the list of folders were not provided, looks for the meteo.folder keyword.
            //If the keyword is also missing, uses a default folder, that in this case will be 'H'
            meteoFolderList = new Dictionary<string, KeywordData>();
            meteoFolderList.Add("1", root["meteo.folder"]);
         }
         else
         {
            meteoFolderList = foldersListNode.NodeData;
         }
         //--------------------------------------------------------------------------------------------------------------

         Console.WriteLine("Sim start date   : {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim enddate      : {0}", simEndDate.ToString("yyyy/MM/dd HH:mm:ss"));

         //Console.WriteLine("Generating Meteo file from {0} to {1}", startsim.ToString("yyyy/MM/dd HH:mm:ss"), mre.sim.End.ToString("yyyy/MM/dd HH:mm:ss"));
         
         bool skipInterpolation = root["skip.interpolation", false].AsBool();

         //This will ensure that the first hour of simulation is included in the meteo file.
         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);

         //--------------------------------------------------------------------------------------------------------------
         //2. Find the required files. The meteo file must contain data for the entire simulation period
         //--------------------------------------------------------------------------------------------------------------
         bool isFirstInstant;

         Console.WriteLine("Folder where meteo files are: {0}", root["meteo.folder"].AsString());
         Console.WriteLine("");
         Console.WriteLine("Looking for files...");
         Console.WriteLine("");

         int maxSkips = root["max.skips", 10].AsInt();
         int numberOfSkips = 0;
         List<string> requiredFiles = new List<string>();

         Console.WriteLine("The folowing folders will be used on searching meteo files:");
         counter = 0;
         foreach (KeywordData folder in meteoFolderList.Values)
         {
            counter++;
            Console.WriteLine("  {0}. {1}", counter, folder.AsFilePath().Path);
         }
         Console.WriteLine("");

         isFirstInstant = true;
         file.FullPath = "";
         while (startsim < end || !found_file)
         {
            dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
            Console.WriteLine("Looking for {0}", @"D3_" + dateStr + ".hdf5");
            foreach (KeywordData folder in meteoFolderList.Values)
            {
               found_file = false;
               file.FullPath = folder.AsFilePath().Path + @"D3_" + dateStr + ".hdf5";
               
               if (System.IO.File.Exists(file.FullPath))
               {
                  fi = new System.IO.FileInfo(file.FullPath);
                  if (fi.Length > 2000000)
                  {
                     Console.WriteLine("  Found at {0}", folder.AsFilePath().Path);
                     if (!skipInterpolation)
                     {
                        try
                        {
                           Console.Write("  Interpolating file....");
                           if (!InterpolateMeteoFile(file))
                           {
                              Console.WriteLine("[failed]");
                              continue;
                           }
                           else
                              Console.WriteLine("[ok]");
                        }
                        catch (Exception ex)
                        {
                           Console.WriteLine("[exception]");
                           Console.WriteLine("  Message returned: {0}", ex.Message);
                           continue;
                        }
                     }
                     else
                     {
                        filesToGlue.Add(file.FullPath);
                     }

                     found_file = true;
                     break;
                  }
               }
            }

            if (!found_file)
            {
               Console.WriteLine("  Not found!", file);

               if ((++numberOfSkips) > maxSkips)
               {
                  Console.WriteLine("Max number of skips reached during meteo creation file.");
                  return false;
               }

               if (isFirstInstant) //first file in the list
               {
                  Console.WriteLine("Going backward...");
                  endsim = startsim.AddHours(-1);
                  startsim = startsim.AddHours(-6);

                  continue;
               }
               else
               {
                  Console.WriteLine("Skipping for the next file...");
                  startsim = endsim.AddHours(1);
                  endsim = startsim.AddHours(5);
               }
            }
            else if (isFirstInstant && numberOfSkips > 0)
            {
               numberOfSkips = 0;
               counter = 0;

               startsim = simStart.AddHours(1);
               endsim = startsim.AddHours(5);
            }
            else if (startsim > end)
            {
               numberOfSkips = 0;
               break;
            }
            else
            {
               numberOfSkips = 0;
               startsim = endsim.AddHours(1);
               endsim = startsim.AddHours(5);
            }

            if (isFirstInstant && found_file) 
               isFirstInstant = false;
         }
         //--------------------------------------------------------------------------------------------------------------

         //Now the interpolated files are glued
         HDFGlue glueTool = new HDFGlue(filesToGlue);         
         glueTool.AppName = root["glue.exe.name"].AsString();
         glueTool.AppPath = root["glue.exe.path"].AsString();
         glueTool.WorkingDirectory = root["glue.working.folder"].AsString();
         glueTool.Output = outputPath + outputName;
         glueTool.Is3DFile = false;
         glueTool.ThrowExceptionOnError = true;

         Console.WriteLine("Gluing files...");

         if (glueTool.Glue() != 0)
         {
            Console.WriteLine("Glue failed.");
            return false;
         }

         //Delete all the files in the temp directory
         try
         {
            System.IO.DirectoryInfo directory = new DirectoryInfo(tempFolder);
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

      protected bool InterpolateMeteoFile(FileName file)
      {
         try
         {
            replace_list.Clear();
            replace_list["<<input.hdf>>"] = file.FullPath;
            replace_list["<<father.grid>>"] = root["father.grid"].AsString();
            replace_list["<<output.hdf>>"] = tempFolder + file.FullName;
            replace_list["<<model.grid>>"] = root["model.grid"].AsString();

            string template = root["template"].AsString();
            string action = root["action.file"].AsString();

            TextFile.Replace(template, action, ref replace_list);

            tool.Arguments = "";
            tool.WorkingDirectory = root["interpolation.working.folder"].AsString();
            tool.Executable = root["interpolation.exe"].AsString();
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "successfully terminated";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            bool result = tool.Run();
            if (!result)
            {
               return false;
            }

            filesToGlue.Add(pathToFilesToGlue + file.FullName);
         }
         catch (Exception ex)
         {
            Console.WriteLine("An exception has happened when interpolating file '{0}'", file.FullName);
            Console.WriteLine("The exception was: {0}.", ex.Message);
            return false;
         }
         return true;
      }

      protected bool FindScriptBlocks(ConfigNode toMatch)
      {
         if (toMatch.Name == "script.data")
            return true;
         return false;
      }
      protected bool FindFoldersListBlocks(ConfigNode toMatch)
      {
         if (toMatch.Name == "meteo.folders.list")
            return true;
         return false;
      }
      protected bool FindHDFExtractorBlocks(ConfigNode toMatch)
      {
         if (toMatch.Name == "hdf.extractor")
            return true;
         return false;
      }
      protected bool FindPracticesBlock(ConfigNode toMatch)
      {
         if (toMatch.Name == "practices")
            return true;
         return false;
      }
   
   }
}
