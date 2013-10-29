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
   public class OPTamegaScript : BCIMohidSim
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
      bool use_spin_up;

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

      public override bool OnStart(object data)
      {
         mre = (MohidRunEngineData)data;
         root = mre.cfg.Root.ChildNodes.Find(FindScriptBlocks);
         use_spin_up = root["use.spin.up.to.start", false].AsBool();
         return true;
      }

      public override bool OnSimStart(object data)
      {
         mre = (MohidRunEngineData)data;

         
         if (root == null)
         {
            Console.WriteLine("Block 'script.data' was not found in the config file.");
            return false;
         }

         storeFolder = mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss");

         if (mre.simID != 1)
         {
            storeFolderOld = mre.sim.Start.AddDays(-mre.log[mre.log.LastIndex].NodeData["sim.lenght"].AsDouble()).ToString("yyyyMMdd.HHmmss") +
                             "-" +
                             mre.sim.Start.ToString("yyyyMMdd.HHmmss");
         }

         //Get some important input from the script block in config file
         generalBoundary.Path = root["general.boundary.conditions"].AsFilePath().Path + storeFolder;
         outputPath           = root["glue.output.path", @"..\" + mre.sim.SimDirectory.Path + @"local.data\boundary.conditions\"].AsString();
         outputName           = root["glue.output.name", "meteo.hdf5"].AsString();
         tempFolder           = root["interpolation.folder", @".\temp\"].AsString();
         pathToFilesToGlue    = root["glue.input.folder", @"..\interpolation\temp\"].AsString();
         
         if (!SetupTemplates())
            return false;

         if (!CopyIniFiles())
            return false;

         if (!CopyMeteoFiles())
            return false;

         if (!CreatePracticesIDHDFFile())
            return false;

         return true;
      }

      protected bool CreatePracticesIDHDFFile()
      {
         practicesFromHDF = false;

         ConfigNode practicesNode = root.ChildNodes.Find(FindPracticesBlock);
         if (practicesNode != null)
         {
            if (practicesNode["search.first", true].AsBool())
            {
               if (File.Exists(generalBoundary.Path + practicesNode["output.hdf"].AsFileName().FullName))
               {
                  if (FileTools.CopyFile(generalBoundary, root["local.boundary.conditions"].AsFilePath(),
                                          practicesNode["output.hdf"].AsFileName().FullName, Files.CopyOptions.OVERWRIGHT))
                  {
                     return true;
                  }
               }
            }

            Dictionary<string, string> replace_list = new Dictionary<string, string>();

            Console.WriteLine("Creating practices HDF file...");
            ExternalApp tool = new ExternalApp();

            replace_list.Clear();
            replace_list["<<grid.data>>"] = practicesNode["grid.data"].AsFileName().FullPath;
            replace_list["<<output.hdf>>"] = practicesNode["output.hdf"].AsFileName().FullPath;
            replace_list["<<start>>"] = mre.sim.Start.ToString("yyyy MM dd HH 00 00");
            replace_list["<<end>>"] = mre.sim.End.ToString("yyyy MM dd HH 00 00");
            replace_list["<<local.data>>"] = @"..\" + root["local.data"].AsFilePath().Path;

            string template = practicesNode["template", mre.sim.SimDirectory.Path + @"templates\tools\fillmatrix.template"].AsString();
            string action = practicesNode["config.file.name", "fillmatrix.dat"].AsString();

            Console.WriteLine("FillMatrix template {0}.", action);

            TextFile.Replace(template, practicesNode["working.folder", @"..\tools\fillmatrix\"].AsFilePath().Path + action, ref replace_list);

            tool.WorkingDirectory = practicesNode["working.folder", @"..\tools\fillmatrix\"].AsFilePath().Path;
            if (practicesNode.NodeData.ContainsKey("arguments"))
               tool.Arguments = practicesNode["arguments"].AsString();
            tool.Executable = root["exe.file", @"..\tools\fillmatrix\fillmatrix.exe"].AsString();
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "Finished..";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            bool result = tool.Run();
            if (!result)
            {
               Console.WriteLine("fillmatrix failed.");
               return false;
            }

            //Saves the created practices file in the folder
            if (!FileTools.CopyFile(root["local.boundary.conditions"].AsFilePath(), generalBoundary, 
                                    practicesNode["output.hdf"].AsFileName().FullName, Files.CopyOptions.OVERWRIGHT))
            {
               Console.WriteLine("Was not possible to copy the meteo file to the meteo storage folder.");
               return false;
            }

            practicesFromHDF = true;
         }

         return true;
      }

      protected bool SetupTemplates()
      {
         Console.WriteLine("Setup of templates...");
         if (mre.simID == 1)
         {
            foreach (InputFileTemplate itf in mre.templatesStart)
            {
               if (itf.Name.Contains("atmosphere"))
               {
                  itf.ReplaceList["<<meteo.sim.folder>>"] = root["meteo.sim.folder"].AsFilePath().Path;
               }
               else if (itf.Name.Contains("nomfich") || itf.Name.Contains("vegetation"))
               {
                  itf.ReplaceList["<<run.folder>>"] = root["run.folder"].AsFilePath().Path;
               }
            }
         }
         else
         {
            foreach (InputFileTemplate itf in mre.templatesContinuation)
            {
               if (itf.Name.Contains("atmosphere"))
               {
                  itf.ReplaceList["<<meteo.sim.folder>>"] = root["meteo.sim.folder"].AsFilePath().Path;
               }
               else if (itf.Name.Contains("nomfich") || itf.Name.Contains("vegetation"))
               {
                  itf.ReplaceList["<<run.folder>>"] = root["run.folder"].AsFilePath().Path;
               }
            }
         }

         return true;
      }

      protected bool CopyIniFiles()
      {
         Console.WriteLine("Copying initialization files...");
         if (mre.simID == 1 || use_spin_up)
         {
            if (root.NodeData.ContainsKey("spin.up.folder"))
            {
               FilePath orig = root["spin.up.folder"].AsFilePath();
               if (!FileTools.CopyFile(orig, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
               {
                  Console.WriteLine("Was not possible to copy the 'fin' files (from spin up) to the init folder.");
                  return false;
               }
            }

            use_spin_up = false;
         }
         else
         {
            FilePath orig = new FilePath(mre.storeFolder.Path + storeFolderOld);
            if (!FileTools.CopyFile(orig, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            {
               Console.WriteLine("Was not possible to copy the 'fin' files to the init folder.");
               return false;
            }
         }
         return true;
      }

      protected bool CopyMeteoFiles()
      {
         FileName orig = new FileName();
         FileName dest = new FileName();

         Console.Write("Looking for meteo files in {0}...", root["general.boundary.conditions"].AsFilePath().Path + storeFolder);

         orig.FullPath = root["general.boundary.conditions"].AsFilePath().Path + storeFolder + @"\meteo.hdf5";
         dest.FullPath = root["local.boundary.conditions"].AsFilePath().Path + @"\meteo.hdf5";

         if (!Directory.Exists(orig.Path))
            Directory.CreateDirectory(orig.Path);

         if (File.Exists(orig.FullPath))
         {
            Console.WriteLine("[Found]");
            FileTools.CopyFile(orig, dest, CopyOptions.OVERWRIGHT);
         }
         else
         {
            Console.WriteLine("[Not Found]");
            
            //Check to see if the second simulation already created the file. If so, copy and use it
            if (root.NodeData.ContainsKey("alternative.folder"))
            {
               Console.Write("Looking for meteo files in alternative folder...");
               FileName origAlt = new FileName();
               origAlt.FullPath = root["alternative.folder"].AsFilePath().Path + storeFolder + @"\meteo.hdf5";
               if (Directory.Exists(orig.Path))
               {
                  if (File.Exists(orig.FullPath))
                  {
                     Console.WriteLine("[Found]");
                     FileTools.CopyFile(origAlt, orig, CopyOptions.OVERWRIGHT);
                     FileTools.CopyFile(origAlt, dest, CopyOptions.OVERWRIGHT);

                     return true;
                  }
               }
               Console.WriteLine("[Not Found]");
            }

            Console.WriteLine("Creating meteo files...");
            if (!GenerateMeteoFilesFromSources())
            {
               Console.WriteLine("Was not possible to create the meteo files.");
               return false;
            }

            FileTools.CopyFile(orig, dest, CopyOptions.OVERWRIGHT);
         }

         return true;
      }

      protected bool GenerateMeteoFilesFromSources()
      {
         try
         {
            DateTime simStartDate = mre.sim.Start,
                     simEndDate = mre.sim.End,
                     simStart = simStartDate.AddSeconds(1),
                     startsim,
                     endsim;
            FileName file = new FileName();
            string input_start_tag;
            string input_end_tag;
            bool found_file = false;
            System.IO.FileInfo fi; 
            Dictionary<string, KeywordData> meteoFolderList;
            string dateStr;
            int maxSkips = root["max.skips", 10].AsInt();
            int numberOfSkips = 0;
            List<string> requiredFiles = new List<string>();
            bool isFirstInstant;
            long minimum_size;

            List<ConfigNode> sources = root.ChildNodes.FindAll(delegate(ConfigNode to_match) { return to_match.Name == "meteo.source"; });
            bool skipInterpolation = root["skip.interpolation", false].AsBool();

            if (sources == null)
               return false;
            if (sources.Count <= 0)
               return false;

            filesToGlue.Clear();

            //This will ensure that the first hour of simulation is included in the meteo file.
            startsim = simStart.AddHours(-5);
            endsim = startsim.AddHours(5);

            Console.WriteLine("Sim start date   : {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
            Console.WriteLine("Sim enddate      : {0}", simEndDate.ToString("yyyy/MM/dd HH:mm:ss"));
            Console.WriteLine("Sim lenght (days): {0}", mre.sim.SimLenght);

            isFirstInstant = true;
            file.FullPath = "";
            while (startsim < mre.sim.End || !found_file)
            {
               dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");               

               Console.WriteLine("Looking for file {0}", "*" + dateStr + "*");

               foreach (ConfigNode src in sources)
               {
                  input_start_tag = src["input.start.tag", "D3_"].AsString();
                  input_end_tag = src["input.end.tag", ""].AsString();
                  minimum_size = src["minimum.size", 2000000].AsLong();

                  ConfigNode folders_list = src.ChildNodes.Find(delegate(ConfigNode to_match) { return to_match.Name == "meteo.folders.list"; });
                  if (folders_list == null)
                  {
                     //If the list of folders were not provided, looks for the meteo.folder keyword.
                     //If the keyword is also missing, uses a default folder, that in this case will be 'H'
                     meteoFolderList = new Dictionary<string, KeywordData>();
                     meteoFolderList.Add("1", root["meteo.folder", @"H:\"]);
                  }
                  else
                  {
                     meteoFolderList = folders_list.NodeData;
                  }

                  foreach (KeywordData folder in meteoFolderList.Values)
                  {
                     found_file = false;
                     file.FullPath = folder.AsFilePath().Path + input_start_tag + dateStr + input_end_tag + ".hdf5";

                     if (System.IO.File.Exists(file.FullPath))
                     {
                        fi = new System.IO.FileInfo(file.FullPath);
                        if (fi.Length > minimum_size)
                        {
                           Console.WriteLine("  Found at {0} [{1}]", folder.AsFilePath().Path, src["name", ""].AsString());
                           if (!skipInterpolation)
                           {
                              try
                              {
                                 Console.Write("  Interpolating file....");
                                 if (!InterpolateMeteoFile(file, src))
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

                  if (found_file) break;
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

                  startsim = simStart.AddHours(1);
                  endsim = startsim.AddHours(5);
               }
               else if (startsim > mre.sim.End)
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

            //Now the interpolated files are glued
            HDFGlue glueTool = new HDFGlue(filesToGlue);
            glueTool.AppName = root["glue.exe.name", "glue.exe"].AsString();
            glueTool.AppPath = root["glue.exe.path", @"..\tools\glue\"].AsString();
            glueTool.WorkingDirectory = root["glue.working.folder", @"..\tools\glue\"].AsString();
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
               System.IO.DirectoryInfo directory = new DirectoryInfo(@"..\tools\interpolation\temp\");
               foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
                  fileToDelete.Delete();
            }
            catch (Exception ex)
            {
               Console.WriteLine("Was not possible to empty the temp directory due to an exception.");
               Console.WriteLine("Message returned was: {0}", ex.Message);
            }

            //Saves the created meteo file in the folder
            if (!FileTools.CopyFile(root["local.boundary.conditions"].AsFilePath(), generalBoundary, "meteo.hdf5", Files.CopyOptions.OVERWRIGHT))
            {
               Console.WriteLine("Was not possible to copy the meteo file to the meteo storage folder.");
               return false;
            }
            
            return true;
         }
         catch 
         {
            return false;
         }                  
      }

      protected bool GenerateMeteoFiles()
      {
         DateTime simStartDate = mre.sim.Start,
                  simEndDate = mre.sim.End,
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
            meteoFolderList.Add("1", root["meteo.folder", @"H:\"]);
         }
         else
         {
            meteoFolderList = foldersListNode.NodeData;
         }
         //--------------------------------------------------------------------------------------------------------------

         Console.WriteLine("Sim start date   : {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim enddate      : {0}", simEndDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim lenght (days): {0}", mre.sim.SimLenght);
         //Console.WriteLine("Generating Meteo file from {0} to {1}", startsim.ToString("yyyy/MM/dd HH:mm:ss"), mre.sim.End.ToString("yyyy/MM/dd HH:mm:ss"));
         
         bool skipInterpolation = root["skip.interpolation", false].AsBool();

         //This will ensure that the first hour of simulation is included in the meteo file.
         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);

         //--------------------------------------------------------------------------------------------------------------
         //2. Find the required files. The meteo file must contain data for the entire simulation period
         //--------------------------------------------------------------------------------------------------------------
         bool isFirstInstant;

         Console.WriteLine("Folder where meteo files are: {0}", root["meteo.folder", @"H:\"].AsString());
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
         while (startsim < mre.sim.End || !found_file)
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

            ////try to find the file (the period) in the "monthly" files in mohid land
            //if (!found_file && mm5NewFilesFolder != "")
            //{
            //   pathToFilesToGlue = root["mm5.months.folder"].AsFilePath().Path +
            //                       startsim.ToString("yyyy") + @"\";
            //   monthly_file = "MM5-D3-Portugal-" + startsim.ToString("yyyy-MM") + ".hdf5";

            //   if (System.IO.File.Exists(pathToFilesToGlue + monthly_file))
            //   {
            //      //find block with info for extractor
            //      ConfigNode extractorNode = root.ChildNodes.Find(FindHDFExtractorBlocks);
            //      if (extractorNode != null)
            //      {
            //         ExternalApp extractorTool = new ExternalApp();

            //         replace_list.Clear();
            //         replace_list["<<input.hdf>>"] = pathToFilesToGlue + monthly_file;
            //         replace_list["<<output.hdf>>"] = mm5NewFilesFolder + @"D3_" + dateStr + ".hdf5";
            //         replace_list["<<start>>"] = startsim.ToString("yyyy MM dd HH 00 00");
            //         replace_list["<<end>>"] = endsim.ToString("yyyy MM dd HH 00 00");

            //         string template = extractorNode["template", mre.sim.SimDirectory.Path + @"templates\tools\extractor.template"].AsString();
            //         string action = extractorNode["config.file.name", "extractor.cfg"].AsString();

            //         Console.WriteLine("Extracting template {0}.", action);

            //         TextFile.Replace(template, extractorNode["working.folder", @"..\tools\extraction\"].AsFilePath().Path + action, ref replace_list);

            //         extractorTool.WorkingDirectory = extractorNode["working.folder", @"..\tools\extraction\"].AsFilePath().Path;
            //         extractorTool.Executable = root["extractor.exe", @"..\tools\extraction\extractor.exe"].AsString();
            //         extractorTool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            //         extractorTool.TextToCheck = "successfully terminated";
            //         extractorTool.SearchTextOrder = SearchTextOrder.FROMEND;
            //         extractorTool.Wait = true;

            //         bool result = extractorTool.Run();
            //         if (!result)
            //         {
            //            Console.WriteLine("Extraction failed.");
            //         }
            //         else
            //         {
            //            requiredFiles.Add(mm5NewFilesFolder + file);
            //            found_file = true;
            //         }
            //      }
            //   }
            //}

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
            else if (startsim > mre.sim.End)
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
         glueTool.AppName = root["glue.exe.name", "glue.exe"].AsString();
         glueTool.AppPath = root["glue.exe.path", @"..\tools\glue\"].AsString();
         glueTool.WorkingDirectory = root["glue.working.folder", @"..\tools\glue\"].AsString();
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
            System.IO.DirectoryInfo directory = new DirectoryInfo(@"..\tools\interpolation\temp\");
            foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
               fileToDelete.Delete();
         }
         catch (Exception ex)
         {
            Console.WriteLine("Was not possible to empty the temp directory due to an exception.");
            Console.WriteLine("Message returned was: {0}", ex.Message);
         }

         //Saves the created meteo file in the folder
         if (!FileTools.CopyFile(root["local.boundary.conditions"].AsFilePath(), generalBoundary, "meteo.hdf5", Files.CopyOptions.OVERWRIGHT))
         {
            Console.WriteLine("Was not possible to copy the meteo file to the meteo storage folder.");
            return false;
         }

         return true;
      }

      protected bool InterpolateMeteoFile(FileName input, ConfigNode cfg)
      {
         {
            try
            {               
               replace_list.Clear();
               replace_list["<<input.hdf>>"] = input.FullPath;
               replace_list["<<father.grid>>"] = cfg["father.grid"].AsFileName().FullPath;
               replace_list["<<output.hdf>>"] = tempFolder + input.FullName;
               replace_list["<<model.grid>>"] = root["model.grid"].AsFileName().FullPath;

               string template = cfg["template"].AsFileName().FullPath;
               string action = root["action.file", @"..\tools\interpolation\converttoHDF5action.dat"].AsString();

               TextFile.Replace(template, action, ref replace_list);

               tool.Arguments = "";
               tool.WorkingDirectory = root["interpolation.working.folder", @"..\tools\interpolation\"].AsString();
               tool.Executable = root["interpolation.exe", @"..\tools\interpolation\interpolation.exe"].AsString();
               tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
               tool.TextToCheck = "successfully terminated";
               tool.SearchTextOrder = SearchTextOrder.FROMEND;
               tool.Wait = true;

               bool result = tool.Run();
               if (!result)
               {
                  return false;
               }

               filesToGlue.Add(pathToFilesToGlue + input.FullName);
            }
            catch (Exception ex)
            {
               Console.WriteLine("An exception has happened when interpolating file '{0}'", input.FullName);
               Console.WriteLine("The exception was: {0}.", ex.Message);
               return false;
            }
            return true;
         }
      }

      protected bool InterpolateMeteoFile(FileName file)
      {
         try
         {
            replace_list.Clear();
            replace_list["<<input.hdf>>"] = file.FullPath;
            replace_list["<<father.grid>>"] = root["father.grid", @"..\..\general.data\digital.terrain\mm5.dat"].AsString();
            replace_list["<<output.hdf>>"] = tempFolder + file.FullName;
            replace_list["<<model.grid>>"] = root["model.grid", @"..\..\general.data\digital.terrain\grid.dat"].AsString();

            string template = root["template", mre.sim.SimDirectory.Path + @"templates\tools\convert.to.HDF5.action.template"].AsString();
            string action = root["action.file", @"..\tools\interpolation\converttoHDF5action.dat"].AsString();

            TextFile.Replace(template, action, ref replace_list);

            tool.Arguments = "";
            tool.WorkingDirectory = root["interpolation.working.folder", @"..\tools\interpolation\"].AsString();
            tool.Executable = root["interpolation.exe", @"..\tools\interpolation\interpolation.exe"].AsString();
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

      public override bool OnSimEnd(object data)
      {
         bool res = true;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
         if (!FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT))
            res = false;

         //Delete all the files in the res directory
         try
         {
            System.IO.DirectoryInfo directory = new DirectoryInfo(mre.resFolder.Path);
            foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
               fileToDelete.Delete();
         }
         catch (Exception ex)
         {
            Console.WriteLine("Was not possible to empty the temp directory due to an exception.");
            Console.WriteLine("Message returned was: {0}", ex.Message);
         }

         return res;
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

