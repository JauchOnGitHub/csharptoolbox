using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Core;
using Mohid.CommandArguments;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;
using Mohid.Files;
using Mohid.Software;

namespace GetTSFromHDF
{
   class Program
   {
      static void Main(string[] args)
      {
         DateTime start, end;
         FilePath rootResultsPath;
         FileName hdfResultsFile;
         List<FilePath> timeseriesOutputPath = new List<FilePath>();
         FileName exporterEXE;
         FilePath exporterWorkingPath;
         bool joinTimeseries = false;
         bool useDateOnTimeseriesName = true;
         string folderFormat, startFormat, endFormat;
         StringBuilder exporterCFGBase = new StringBuilder();
         StringBuilder exporterCFG = new StringBuilder();
         List<ConfigNode> nodes;
         double runLenght;
         bool verbose;
         List<FileInfo> tsFileInfoList = new List<FileInfo>();
         Dictionary<string, TimeSeries> outTS = new Dictionary<string,TimeSeries>();
         TimeSeries newTS = new TimeSeries();
         TextFile txtFile, errors;
         int timeseriesSaveCounter = 1, saveCounter;
         int timeseriesNameCounter;
         errors = new TextFile("errors.log");
         errors.OpenNewToWrite();
         verbose = true;
         FilePath path;
         bool dateAndTimeInOutputFolder;
         string outputFolderFormat = "", outputFolderStartFormat = "", outputFolderEndFormat = "", outputPath, outputPath2;
         bool useCounter;

         try
         {
            CmdArgs cmdArgs = new CmdArgs(args);

            if (cmdArgs.HasOption("v"))
               verbose = true;
            else
               verbose = false;


            if (cmdArgs.HasParameter("cfg"))
            {
               if (verbose)
                  Console.Write("Reading configuration file...");

               Config conf = new Config(cmdArgs.Parameters["cfg"]);
               conf.Load();

               if (verbose)
                  Console.WriteLine("[OK]");

               if (verbose)
                  Console.Write("Looking for 'timeseries.to.extract' blocks...");
               
               List<ConfigNode> tsList = conf.Root.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "timeseries.to.extract"; });

               if (tsList == null || tsList.Count <= 0)
                  throw new Exception("No 'timeseries.to.extract' block found in configuration file.");

               if (verbose)
               {
                  Console.WriteLine("[OK]");
                  Console.WriteLine("{0} 'timeseries.to.extract' block(s) found.", tsList.Count);
               }

               dateAndTimeInOutputFolder = conf.Root["user.def.output.folder", false].AsBool();
               if (dateAndTimeInOutputFolder)
               {
                   outputFolderFormat = conf.Root["user.def.output.folder.format", "start-end"].AsString();
                   outputFolderStartFormat = conf.Root["user.def.output.start.format", "yyyyMMddHH"].AsString();
                   outputFolderEndFormat = conf.Root["user.def.output.end.format", "yyyyMMddHH"].AsString();
               }

               int tscCount = 1;
               foreach (ConfigNode tsc in tsList)
               {
                  timeseriesNameCounter = 1;

                  if (verbose)
                     Console.Write("Processing 'timeseries.to.extract' block {0}...", tscCount);

                  start                   = tsc["start"].AsDateTime();
                  end                     = tsc["end"].AsDateTime();
                  rootResultsPath         = tsc["root.results.path", @".\"].AsFilePath();
                  hdfResultsFile          = tsc["hdf.results.file"].AsFileName();
                  if (tsc.Contains("backup_path"))
                  {
                      path = tsc["backup_path"].AsFilePath();
                      timeseriesOutputPath.Add(path);
                  }
                  if (tsc.Contains("storage_path"))
                  {
                      path = tsc["storage_path"].AsFilePath();
                      timeseriesOutputPath.Add(path);
                  }
                  if (tsc.Contains("publish_path"))
                  {
                      path = tsc["publish_path"].AsFilePath();
                      timeseriesOutputPath.Add(path);
                  }

                  if (timeseriesOutputPath.Count <= 0)
                  {
                      throw new Exception("An error was found in the configuration file. Missing keyword." + "At least ONE of the following keywords must be provided:" +
                                          "BACKUP_PATH, STORAGE_PATH, PUBLISH_PATH");                      
                  }
                  exporterEXE             = tsc["exporter.exe"].AsFileName();
                  exporterWorkingPath     = tsc["exporter.working.path"].AsFilePath();
                  joinTimeseries          = tsc["join.timeseries", true].AsBool();
                  if (joinTimeseries && dateAndTimeInOutputFolder)
                  {
                      throw new Exception("join.timeseries can't be used with user.def.output.folder");
                  }
                  folderFormat            = tsc["folder.format", "start-end"].AsString();
                  startFormat             = tsc["start.format", "yyyyMMddHH"].AsString();
                  endFormat               = tsc["end.format", "yyyyMMddHH"].AsString();
                  runLenght               = tsc["run.lenght"].AsDouble();
                  useCounter              = tsc["use.counter.on.names", false].AsBool();

                  if (joinTimeseries)
                  {
                     outTS = new Dictionary<string, TimeSeries>();
                     newTS = new TimeSeries();

                     timeseriesSaveCounter = tsc["save.counter"].AsInt();
                  }
                  else
                  {
                     useDateOnTimeseriesName = tsc["use.date.on.timeseries.name", true].AsBool();
                  }

                  exporterCFGBase.Append("EXPORT_TYPE      : "); exporterCFGBase.AppendLine(tsc["export.type", 1].AsString());
                  exporterCFGBase.Append("COMPUTE_RESIDUAL : "); exporterCFGBase.AppendLine(tsc["compute.residual", 0].AsString());
                  exporterCFGBase.Append("VARIABLE_GRID    : "); exporterCFGBase.AppendLine(tsc["variable.grid", 0].AsString());
                  exporterCFGBase.Append("WATERPOINTS_NAME : "); exporterCFGBase.AppendLine(tsc["points.name", "WaterPoints2D"].AsString());
                  exporterCFGBase.Append("GRID_FILENAME    : "); exporterCFGBase.AppendLine(tsc["grid.file.name", "grid.dat"].AsString());

                  nodes = tsc.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "timeseries"; });
                  
                  if (nodes == null || nodes.Count <= 0)
                     throw new Exception("No 'timeseries' block found in configuration file.");

                  foreach (ConfigNode param in nodes)
                  {
                     exporterCFGBase.AppendLine();
                     exporterCFGBase.AppendLine("<BeginTimeSerie>");
                     exporterCFGBase.Append("  NAME            : "); exporterCFGBase.AppendLine(param["name"].AsString());

                     if (param.NodeData.ContainsKey("coord.y")) 
                     { 
                        exporterCFGBase.Append("  COORD_Y        : "); 
                        exporterCFGBase.AppendLine(param["coord.y"].AsString()); 
                     }
                     if (param.NodeData.ContainsKey("coord.x")) 
                     { 
                        exporterCFGBase.Append("  COORD_X        : "); 
                        exporterCFGBase.AppendLine(param["coord.x"].AsString()); 
                     }
                     if (param.NodeData.ContainsKey("depth.level"))
                     {
                        exporterCFGBase.Append("  DEPTH_LEVEL    : ");
                        exporterCFGBase.AppendLine(param["depth.level"].AsString());
                     }
                     if (param.NodeData.ContainsKey("localization.i"))
                     {
                        exporterCFGBase.Append("  LOCALIZATION_I : "); 
                        exporterCFGBase.AppendLine(param["localization.i"].AsString());
                     }
                     if (param.NodeData.ContainsKey("localization.j"))
                     {
                        exporterCFGBase.Append("  LOCALIZATION_J : ");
                        exporterCFGBase.AppendLine(param["localization.j"].AsString());
                     }
                     if (param.NodeData.ContainsKey("localization.k"))
                     {
                        exporterCFGBase.Append("  LOCALIZATION_K : ");
                        exporterCFGBase.AppendLine(param["localization.k"].AsString());
                     }
                     if (param.NodeData.ContainsKey("latitude"))
                     {
                        exporterCFGBase.Append("  LATITUDE       : ");
                        exporterCFGBase.AppendLine(param["latitude"].AsString());
                     }
                     if (param.NodeData.ContainsKey("longitude"))
                     {
                        exporterCFGBase.Append("  LONGITUDE      : ");
                        exporterCFGBase.AppendLine(param["longitude"].AsString());
                     }

                     exporterCFGBase.AppendLine("<EndTimeSerie>");

                     if (joinTimeseries)
                     {
                        outTS[param["name"].AsString()] = new TimeSeries();
                     }
                  }

                  nodes = tsc.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "parameter"; });

                  if (nodes == null || nodes.Count <= 0)
                     throw new Exception("No 'parameter' block found in configuration file.");

                  foreach (ConfigNode param in nodes)
                  {
                     exporterCFGBase.AppendLine();
                     exporterCFGBase.AppendLine("<BeginParameter>");
                     exporterCFGBase.Append("  HDF_GROUP : "); exporterCFGBase.AppendLine(param["hdf.group"].AsString());
                     exporterCFGBase.Append("  PROPERTY  : "); exporterCFGBase.AppendLine(param["hdf.property"].AsString());
                     exporterCFGBase.AppendLine("<EndParameter>");
                  }


                  txtFile = new TextFile(exporterWorkingPath.Path + "nomfich.dat");
                  txtFile.OpenNewToWrite();
                  txtFile.WriteLine("IN_MODEL : hdfexporter.dat");
                  txtFile.Close();

                  ExternalApp hdf5exporter = new ExternalApp();
                  hdf5exporter.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
                  hdf5exporter.TextToCheck = "Program HDF5Exporter successfully terminated";
                  hdf5exporter.Wait = true;
                  hdf5exporter.WorkingDirectory = exporterWorkingPath.Path;
                  hdf5exporter.Executable = exporterEXE.FullPath;

                  bool fail;
                  DateTime actual = start;
                  FilePath hdfResultsPath = new FilePath();
                  saveCounter = 1;
                  while (actual.AddDays(runLenght) <= end)
                  {
                     fail = false;

                     hdfResultsPath.Path = rootResultsPath.Path + 
                           (folderFormat.Replace("start", actual.ToString(startFormat)).Replace("end", actual.AddDays(runLenght).ToString(endFormat)));

                     if (System.IO.Directory.Exists(hdfResultsPath.Path))
                     {

                        exporterCFG.Clear();
                        exporterCFG.Append("START_TIME : "); exporterCFG.AppendLine(actual.ToString("yyyy M d H m s"));
                        exporterCFG.Append("END_TIME : "); exporterCFG.AppendLine(actual.AddDays(runLenght).ToString("yyyy M d H m s"));
                        exporterCFG.AppendLine();
                        exporterCFG.AppendLine("<BeginHDF5File>");
                        exporterCFG.Append("  NAME : "); exporterCFG.AppendLine(hdfResultsPath.Path + hdfResultsFile.FullName);
                        exporterCFG.AppendLine("<EndHDF5File>");

                        txtFile.File.FullPath = exporterWorkingPath.Path + "hdfexporter.dat";
                        txtFile.OpenNewToWrite();
                        txtFile.Write(exporterCFGBase.ToString());
                        txtFile.Write(exporterCFG.ToString());
                        txtFile.Close();

                        try
                        {
                           bool res = hdf5exporter.Run();
                           if (!res)
                           {
                              throw new Exception("Unsuccessfull HDF5Exporter run");
                           }
                        }
                        catch (Exception e_run)
                        {
                           errors.WriteLine("[" + DateTime.Now.ToString() + "] HDF5Exporter Run Exception when processing '" + hdfResultsFile + "' on results folder '" + hdfResultsPath.Path + "'");
                           errors.WriteLine("[" + DateTime.Now.ToString() + "] The exception returned this message: " + e_run.Message);
                           fail = true;
                        }

                        if (!fail)
                        {
                           FileTools.FindFiles(ref tsFileInfoList, exporterWorkingPath, "*.ets", true, "", System.IO.SearchOption.TopDirectoryOnly);

                           if (joinTimeseries)
                           {
                              foreach (FileInfo file in tsFileInfoList)
                              {
                                 if (outTS[file.FileName.Name].NumberOfInstants > 0)
                                 {
                                    saveCounter++;

                                    try
                                    {
                                       newTS.Load(file.FileName);
                                       outTS[file.FileName.Name].AddTimeSeries(newTS);
                                    }
                                    catch
                                    {
                                       errors.WriteLine("[" + DateTime.Now.ToString() + "] Was not possible to read timeseries '" + file.FileName.FullName + "' from HDF file '" + hdfResultsFile + "' on results folder '" + hdfResultsPath.Path + "'");
                                    }

                                    System.IO.File.Delete(file.FileName.FullPath);

                                    if (saveCounter > timeseriesSaveCounter)
                                    {
                                       try
                                       {
                                           if (dateAndTimeInOutputFolder)
                                           {
                                               outputPath = timeseriesOutputPath[0].Path +
                                                   (outputFolderFormat.Replace("start", actual.ToString(outputFolderStartFormat)).Replace("end", actual.AddDays(runLenght).ToString(outputFolderEndFormat))) + System.IO.Path.DirectorySeparatorChar;
                                           }
                                           else
                                           {
                                               outputPath = timeseriesOutputPath[0].Path;
                                           }
                                           if (!System.IO.Directory.Exists(outputPath))
                                               System.IO.Directory.CreateDirectory(outputPath);

                                          outTS[file.FileName.Name].Save(new FileName(outputPath + file.FileName.FullName));
                                          saveCounter = 1;
                                          if (timeseriesOutputPath.Count > 1)
                                          {
                                              for (int i = 1; i < timeseriesOutputPath.Count; i++)
                                              {
                                                  outputPath2 = timeseriesOutputPath[i].Path +
                                                   (outputFolderFormat.Replace("start", actual.ToString(outputFolderStartFormat)).Replace("end", actual.AddDays(runLenght).ToString(outputFolderEndFormat))) + System.IO.Path.DirectorySeparatorChar;
                                                  if (!System.IO.Directory.Exists(outputPath2))
                                                      System.IO.Directory.CreateDirectory(outputPath2);
                                                  FileTools.CopyFile(new FileName(outputPath + file.FileName.FullName),
                                                                     new FileName(outputPath2 + file.FileName.FullName), CopyOptions.OVERWRIGHT);
                                              }
                                          }

                                       }
                                       catch (Exception ex)
                                       {
                                          errors.WriteLine("[" + DateTime.Now.ToString() + "] Was not possible to save joined timeseries '" + file.FileName.FullName + "' from HDF file '" + hdfResultsFile + "' when processing results folder '" + hdfResultsPath.Path + "'");
                                          errors.WriteLine("[" + DateTime.Now.ToString() + "] The exception returned this message: " + ex.Message);
                                       }
                                    }
                                 }
                                 else
                                 {
                                    outTS[file.FileName.Name].Load(file.FileName);
                                    System.IO.File.Delete(file.FileName.FullPath);
                                 }
                              }
                           }
                           else
                           {
                              FileName timeseriesTarget = new FileName();

                              foreach (FileInfo file in tsFileInfoList)
                              {
                                  if (dateAndTimeInOutputFolder)
                                  {
                                      outputPath = timeseriesOutputPath[0].Path +
                                          (outputFolderFormat.Replace("start", actual.ToString(outputFolderStartFormat)).Replace("end", actual.AddDays(runLenght).ToString(outputFolderEndFormat))) + System.IO.Path.DirectorySeparatorChar;
                                  }
                                  else
                                  {
                                      outputPath = timeseriesOutputPath[0].Path;
                                  }
                                  timeseriesTarget.Path = outputPath;
                                  if (!System.IO.Directory.Exists(outputPath))
                                      System.IO.Directory.CreateDirectory(outputPath);
                                  if (useDateOnTimeseriesName)
                                 {
                                     
                                    timeseriesTarget.FullName = file.FileName.FullName.Insert(file.FileName.FullName.LastIndexOf('.'), actual.ToString(startFormat) + "_" + actual.AddDays(runLenght).ToString(endFormat));
                                 }
                                  else if (useCounter)
                                  {
                                      timeseriesTarget.FullName = file.FileName.FullName.Insert(file.FileName.FullName.LastIndexOf('.'), timeseriesNameCounter.ToString());
                                      timeseriesNameCounter++;
                                  }
                                  else
                                  {
                                      timeseriesTarget.FullName = file.FileName.FullName;
                                  }
                                 FileTools.CopyFile(file.FileName, timeseriesTarget, CopyOptions.OVERWRIGHT);
                                 if (timeseriesOutputPath.Count > 1)
                                 {
                                     for (int i = 1; i < timeseriesOutputPath.Count; i++)
                                     {
                                         outputPath2 = timeseriesOutputPath[i].Path +
                                          (outputFolderFormat.Replace("start", actual.ToString(outputFolderStartFormat)).Replace("end", actual.AddDays(runLenght).ToString(outputFolderEndFormat))) + System.IO.Path.DirectorySeparatorChar;
                                         if (!System.IO.Directory.Exists(outputPath2))
                                             System.IO.Directory.CreateDirectory(outputPath2);
                                         FileTools.CopyFile(new FileName(outputPath + file.FileName.FullName),
                                                            new FileName(outputPath2 + file.FileName.FullName), CopyOptions.OVERWRIGHT);
                                     }
                                 }
                              }
                           }
                        }
                     }

                     actual = actual.AddDays(runLenght);
                  }

                  tscCount++;

                  if (joinTimeseries)
                  {
                     foreach (KeyValuePair<string, TimeSeries> pair in outTS)
                     {
                         if (dateAndTimeInOutputFolder)
                         {
                             outputPath = timeseriesOutputPath[0].Path +
                                 (outputFolderFormat.Replace("start", actual.ToString(outputFolderStartFormat)).Replace("end", actual.AddDays(runLenght).ToString(outputFolderEndFormat))) + System.IO.Path.DirectorySeparatorChar;
                         }
                         else
                         {
                             outputPath = timeseriesOutputPath[0].Path;
                         }
                         if (!System.IO.Directory.Exists(outputPath))
                             System.IO.Directory.CreateDirectory(outputPath);
                         pair.Value.Save(new FileName(outputPath + pair.Key + ".ets"));
                         for (int i = 1; i < timeseriesOutputPath.Count; i++)
                         {
                             outputPath2 = timeseriesOutputPath[i].Path +
                              (outputFolderFormat.Replace("start", actual.ToString(outputFolderStartFormat)).Replace("end", actual.AddDays(runLenght).ToString(outputFolderEndFormat))) + System.IO.Path.DirectorySeparatorChar;
                             if (!System.IO.Directory.Exists(outputPath2))
                                 System.IO.Directory.CreateDirectory(outputPath2);
                             FileTools.CopyFile(new FileName(outputPath + pair.Key + ".ets"),
                                                new FileName(outputPath2 + pair.Key + ".ets"), CopyOptions.OVERWRIGHT);
                         }
                     }
                  }  

               }
            }
            Console.WriteLine("[OK]");
         }
         catch(Exception ex)
         {
            if (verbose)
            {
               Console.WriteLine("[FAIL]");
               Console.WriteLine("");
               Console.WriteLine("An EXCEPTION was raised. The message returned was:");
               Console.WriteLine(ex.Message);
            }
         }

         Console.WriteLine("GetTSFromModelResultsHDF finished.");
         errors.Close();
      }
   }
}

