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
         string stepMessage = "";
         List<ConfigNode> timeseries, parameters;
         List<string> cfg = new List<string>();
         List<string> blocks = new List<string>();
         DateTime start, end, actual;
         FilePath root;
         bool hasFolders;
         TextFile config, errors;
         string hdfTag;
         FileName hdf5EXE = new FileName();
         FilePath workingFolder = new FilePath();
         FilePath tsPath;
         Dictionary<string, TimeSeries> outTS = new Dictionary<string, TimeSeries>();
         TimeSeries newTS = new TimeSeries();
         List<string> failedPeriods = new List<string>();
         int count_to_save;
         bool use_year_on_path;
         bool join_time_series;
         string startFormat, endFormat, folderFormat;

         errors = new TextFile("errors.log");
         errors.OpenNewToWrite();
         count_to_save = 0;

         try
         {
            stepMessage = "command line arguments loading.";
            CmdArgs cmdArgs = new CmdArgs(args);

            if (cmdArgs.HasParameter("cfg"))
            {
               stepMessage = "configuration file loading.";
               Config conf = new Config(cmdArgs.Parameters["cfg"]);
               conf.Load();

               stepMessage = "configuration file parsing.";
               ConfigNode tsc = conf.Root.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "timeseries.to.extract"; });

               if (tsc == null)
                  throw new Exception("block 'timeseries.to.extract' is missing.");

               start = tsc["start.date"].AsDateTime();
               end = tsc["end.date"].AsDateTime();
               hdfTag = tsc["hdf.tag"].AsString();
               hdf5EXE = tsc["exporter.exe"].AsFileName();
               workingFolder = tsc["exporter.working"].AsFilePath();
               tsPath = tsc["timeseries.output.path"].AsFilePath();
               //root = tsc["root", ".\\"].AsFilePath();
               hasFolders = tsc["has.folders", true].AsBool();
               use_year_on_path = tsc["use.year.on.path", true].AsBool();
               join_time_series = tsc["join.timeseries", true].AsBool();
               folderFormat = tsc["folder.format", "{start}_{end}"].AsString();
               startFormat = tsc["start.format", "yyyyMMddHH"].AsString();
               endFormat = tsc["end.format", "yyyyMMddHH"].AsString();

               blocks.Add("EXPORT_TYPE      : " + tsc["export.type", 1].AsString());
               blocks.Add("COMPUTE_RESIDUAL : " + tsc["compute.residual", 0].AsString());
               blocks.Add("VARIABLE_GRID    : " + tsc["variable.grid", 0].AsString());
               blocks.Add("WATERPOINTS_NAME : " + tsc["points.name", "WaterPoints2D"].AsString());
               blocks.Add("GRID_FILENAME    : " + tsc["grid.name", "grid.dat"].AsString());
               blocks.Add("");


               timeseries = tsc.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "timeseries"; });
               if (timeseries.Count < 1)
                  throw new Exception("Block 'timeseries' is missing. There must be at least one.");

               //Creates the blocks of timeseries
               foreach (ConfigNode n in timeseries)
               {
                  blocks.Add("<BeginTimeSerie>");
                  blocks.Add("  NAME    : " + n["name"].AsString());
                  blocks.Add("  COORD_Y : " + n["y"].AsString());
                  blocks.Add("  COORD_X : " + n["x"].AsString());
                  blocks.Add("<EndTimeSerie>");
                  blocks.Add("");

                  outTS[n["name"].AsString()] = new TimeSeries();
                  if (System.IO.File.Exists(tsPath.Path + n["name"].AsString() + ".ets"))
                  {
                     outTS[n["name"].AsString()].Load(new FileName(tsPath.Path + n["name"].AsString() + ".ets"));
                  }
               }

               parameters = tsc.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "parameters"; });
               if (parameters.Count < 1)
                  throw new Exception("Block 'parameters' is missing. There must be at least one.");

               //Creates the blocks of parameters
               foreach (ConfigNode n in parameters)
               {
                  blocks.Add("<BeginParameter>");
                  blocks.Add("  HDF_GROUP : " + n["group"].AsString());
                  blocks.Add("  PROPERTY  : " + n["property"].AsString());
                  blocks.Add("<EndParameter>");
                  blocks.Add("");
               }

               config = new TextFile();
               config.File.FullPath = workingFolder.Path + "HDF5Exporter.dat";

               bool quit = false;
               actual = start;
               FilePath yearFolder = new FilePath();
               ExternalApp hdf5exporter = new ExternalApp();
               hdf5exporter.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
               hdf5exporter.TextToCheck = "Program HDF5Exporter successfully terminated";
               hdf5exporter.Wait = true;
               hdf5exporter.WorkingDirectory = workingFolder.Path;
               hdf5exporter.Executable = hdf5EXE.FullPath;

               FilePath wp = workingFolder;
               List<FileInfo> tsList = new List<FileInfo>();
               bool failed, res;
               do
               {
                  failed = false;
                  if (use_year_on_path)
                     yearFolder.Path = root.Path + actual.Year.ToString();
                  else
                     yearFolder.Path = root.Path;

                  if (FileTools.FolderExists(yearFolder))
                  {
                     if (System.IO.File.Exists(yearFolder.Path + hdfTag + actual.ToString("yyyyMMddHH") + "_" + actual.AddHours(5).ToString("yyyyMMddHH") + ".hdf5"))
                     {
                        config.OpenNewToWrite();

                        cfg.Clear();
                        cfg.Add("");
                        cfg.Add("START_TIME : " + actual.ToString("yyyy M d H m s"));
                        cfg.Add("END_TIME   : " + actual.AddHours(5).ToString("yyyy M d H m s"));
                        cfg.Add("");
                        cfg.Add("<BeginHDF5File>");
                        cfg.Add("  NAME : " + yearFolder.Path + hdfTag + actual.ToString("yyyyMMddHH") + "_" + actual.AddHours(5).ToString("yyyyMMddHH") + ".hdf5");
                        cfg.Add("<EndHDF5File>");
                        cfg.Add("");
                        config.WriteLines(cfg);
                        config.WriteLines(blocks);
                        
                        config.Close();

                        //executes HDF5Exporter                        
                        try
                        {
                           Console.Write("Running HDF5Exporter to file " + hdfTag + actual.ToString("yyyyMMddHH") + "_" + actual.AddHours(5).ToString("yyyyMMddHH") + ".hdf5 ...");
                           res = hdf5exporter.Run();
                           if (!res)
                           {
                              errors.WriteLine("Unsuccessfull HDF5Exporter run on file '" + hdfTag + actual.ToString("yyyyMMddHH") + "_" + actual.AddHours(5).ToString("yyyyMMddHH") + ".hdf5'");
                              failed = true;
                              Console.WriteLine("[Failed]");
                           }
                           else
                           {
                              Console.WriteLine("[OK]");
                           }
                        }
                        catch(Exception e_run) 
                        {
                           errors.WriteLine("HDF5Exporter Run Exception on file '" + hdfTag + actual.ToString("yyyyMMddHH") + "_" + actual.AddHours(5).ToString("yyyyMMddHH") + ".hdf5'");
                           errors.WriteLine("Exception returned this message: " + e_run.Message);
                           failed = true;
                           Console.WriteLine("[Exception]");
                        }

                        if (!failed)
                        {
                           FileTools.FindFiles(ref tsList, wp, "*.ets", true, "", System.IO.SearchOption.TopDirectoryOnly);
                           foreach (FileInfo file in tsList)
                           {
                              if (outTS[file.FileName.Name].NumberOfInstants > 0)
                              {
                                 try
                                 {
                                    newTS.Load(file.FileName);
                                    //outTS.Load(new FileName(tsPath.Path + file.FileName.FullName));
                                    outTS[file.FileName.Name].AddTimeSeries(newTS);
                                 }
                                 catch
                                 {
                                    errors.WriteLine("Was not possible to read timeseries '" + file.FileName + "' from HDF file '" + hdfTag + actual.ToString("yyyyMMddHH") + "_" + actual.AddHours(5).ToString("yyyyMMddHH") + ".hdf5'");
                                 }
                                 //outTS.Save(new FileName(tsPath.Path + file.FileName.FullName));
                                 System.IO.File.Delete(file.FileName.FullPath);
                              }
                              else
                              {
                                 //FileTools.CopyFile(file.FileName, new FileName(tsPath.Path + file.FileName.FullName), CopyOptions.OVERWRIGHT);
                                 //outTS[file.FileName.Name].Load(new FileName(tsPath.Path + file.FileName.FullName));
                                 
                                 outTS[file.FileName.Name].Load(file.FileName);
                                 System.IO.File.Delete(file.FileName.FullPath);
                              }
                           }
                           
                           count_to_save++;
                           if (count_to_save >= 60)
                           {
                              count_to_save = 0;
                              foreach (FileInfo file in tsList)
                              {
                                 Console.Write("Saving Timeseries '" + file.FileName.Name + "' ...");
                                 try
                                 {
                                    outTS[file.FileName.Name].Save(new FileName(tsPath.Path + file.FileName.Name + ".ets"));
                                    Console.WriteLine("[OK]");
                                    FileTools.CopyFile(new FileName(tsPath.Path + file.FileName.Name + ".ets"), new FileName(tsPath.Path + "bkp\\" + file.FileName.Name + ".ets"), CopyOptions.OVERWRIGHT);
                                 }
                                 catch(Exception e_run)
                                 {
                                    Console.WriteLine("[FAILED]");
                                    errors.WriteLine("Was not possible to save timeseries '" + file.FileName.Name + ".ets'");
                                    errors.WriteLine("Exception returned this message: " + e_run.Message);
                                 }
                              }
                           }
                        }
                     }
                  }

                  actual = actual.AddHours(6);
                  if (actual >= end)
                     quit = true;
               }
               while (!quit);

               foreach (KeyValuePair<string, TimeSeries> pair in outTS)
               {
                  pair.Value.Save(new FileName(tsPath.Path + pair.Key + ".ets"));
               }

               //TimeUnits timeUnits = (TimeUnits)Enum.Parse(typeof(TimeUnits), conf.Root["time.units", "seconds"].AsString(), true);





               //   List<FileName> list = new List<FileName>();
               //   foreach (KeyValuePair<string, KeywordData> item in nodeList.NodeData)
               //      list.Add(item.Value.AsFileName());

               //   if (list.Count <= 1)
               //      throw new Exception("Block 'timeseries.to.join' must contain at least 2 entries");

               //   stepMessage = "loading timeseries.";
               //   List<TimeSeries> timeSeries = new List<TimeSeries>();

               //   foreach (FileName ts in list)
               //   {
               //      TimeSeries newTS = new TimeSeries();
               //      newTS.Load(ts);
               //      timeSeries.Add(newTS);
               //   }

               //   start = timeSeries[0].StartInstant;
               //   for (int i = 1; i < timeSeries.Count; i++)
               //   {
               //      if (timeSeries[i].StartInstant < start)
               //         start = timeSeries[i].StartInstant;
               //   }

               //   stepMessage = "creating output timeseries.";
               //   TimeSeries outTS = new TimeSeries();
               //   outTS.StartInstant = start;
               //   outTS.TimeUnits = timeUnits;

               //   foreach (Column col in timeSeries[0].Columns)
               //   {
               //      Column newCol = new Column(col.ColumnType);
               //      newCol.Header = col.Header;
               //      outTS.AddColumn(newCol);
               //   }

               //   foreach (TimeSeries toJoin in timeSeries)
               //      outTS.AddTimeSeries(toJoin);

               //   stepMessage = "saving output timeseries.";
               //   outTS.Save(outputFileName);

               //   Console.WriteLine("Process complete with success.");
               //}
               //else
               //{
               //   Console.WriteLine("Parameter --cfg is missing.");
               //   Console.WriteLine("Execution aborted.");
               //   return;
               //}
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("An exception was raised while {0}", stepMessage);
            Console.WriteLine("Exception message: {0}", ex.Message);
         }

         errors.Close();
      }
   }
}
