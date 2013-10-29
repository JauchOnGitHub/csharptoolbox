//DLLNAME: MohidToolboxCore
//DLLNAME: System
//DLLNAME: HDF5DotNet

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.AccessControl;

using HDF5DotNet;

using Mohid;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Simulation;
using Mohid.Core;
using Mohid.Log;
using Mohid.Software;
using Mohid.HDF;
using Mohid.MohidTimeSeries;

namespace Mohid
{
   public class ScriptV6 : BCIMohidSim
   {
      #region DATA
      protected bool exception_raised;
      protected Exception exception;
      Dictionary<string, string> replace_list;
      List<FileName> files_to_glue;
      ExternalApp tool;
      HDFGlue tool_glue;
      bool standard_names;

      bool save_data_files;

      protected MohidRunEngineData mred;
      protected ConfigNode script_block;
      protected ConfigNode practices_block;

      protected Dictionary<string, ConfigNode> tools_blocks;
      protected ConfigNode tool_fillmatrix_info;
      protected ConfigNode tool_interpolate_info;
      protected ConfigNode tool_glue_info;
      protected ConfigNode tool_jointimeseries_info;

      protected string store_folder; //Folder where the results will be stored after the run. It is composed by the start and end date of the run.
      protected string store_folder_old; //Folder where the results of the last run were stored.

      protected FilePath general_data;
      protected FilePath general_boundary_data;
      protected FilePath run_boundary_data;
      protected FilePath tools_working_folder;

      //Practices related variables      
      protected FileName practices_file; //Indicate the path (and name of the file) for the practices hdf file that will be used in the run.

      //Meteo reletade variables
      protected bool meteo_task;
      protected ConfigNode meteo_block;
      protected List<ConfigNode> meteo_sources_blocks;
      protected FileName meteo_file_name;
      protected bool meteo_backup_files;
      protected bool meteo_backup_interpolated_files;
      protected FilePath meteo_store_path;
      protected FilePath meteo_store_int_path;
      protected FilePath meteo_interpolation_path;
      protected int meteo_max_contiguous_skips;
      protected int meteo_hour_correction;
      #endregion DATA

      public ScriptV6()
      {
         exception_raised = false;

         tool = new ExternalApp();
         files_to_glue = new List<FileName>();
         replace_list = new Dictionary<string, string>();
         tools_blocks = new Dictionary<string, ConfigNode>();
         practices_block = null;

         save_data_files = true;

         tool_fillmatrix_info = null;
         tool_interpolate_info = null;
         tool_glue_info = null;
         tool_jointimeseries_info = null;

         meteo_block = null;
         meteo_sources_blocks = null;
         meteo_file_name = null;
         meteo_backup_files = false;
         meteo_store_path = null;
         meteo_backup_interpolated_files = false;
         meteo_store_int_path = null;
         meteo_interpolation_path = null;
         meteo_max_contiguous_skips = 8;
         meteo_task = false;
         tool_glue = new HDFGlue();

         standard_names = true;
      }

      public override bool AfterInitialization(object data)
      {
         try
         {
            mred = (MohidRunEngineData)data;

            script_block = mred.cfg.Root.ChildNodes.Find(delegate(ConfigNode cfg) { return cfg.Name == "script.data"; });
            if (script_block == null)
               throw new Exception("Block 'script.data' was not found in the config file.");

            LoadGlobalConfiguration();
            LoadToolsConfiguration();
            LoadTasksConfiguration();

            return true;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.AfterInitialization", ex);
            return false;
         }
      }

      public override bool OnSimStart(object data)
      {
         try
         {
            SetupRun();

            return true;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.OnSimStart", ex);
            return false;
         }
      }

      public override bool OnSimEnd(object data)
      {
         try
         {
            bool result = true;

            FilePath store = FileTools.CreateFolder(store_folder, mred.storeFolder);
            FileName dest = new FileName();

            List<FileName> files_to_process = new List<FileName>();
            int found = FindFiles(mred.resFolder, files_to_process, "*.*");

            foreach (FileName file in files_to_process)
            {
               dest.Path = store.Path;
               dest.FullName = file.FullName;

               if (standard_names)
               {
                  if (file.Name.EndsWith("_old"))
                     continue;
               }

               File.Copy(file.FullPath, dest.FullPath, true);
            }

            if (save_data_files)
            {
               files_to_process.Clear();
               found = FindFiles(mred.sim.DataDirectory, files_to_process, "*.*");

               foreach (FileName file in files_to_process)
               {
                  dest.Path = store.Path;
                  dest.FullName = file.FullName;

                  File.Copy(file.FullPath, dest.FullPath, true);
               }
            }

            ClearFolder(mred.resFolder);

            return result;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.OnSimEnd", ex);
            return false;
         }
      }

      public override bool OnEnd(object data)
      {
         try
         {
            List<ConfigNode> join_ts_list = script_block.ChildNodes.FindAll(delegate(ConfigNode cfg) { return cfg.Name == "task.join.ts"; });

            if (join_ts_list != null && join_ts_list.Count > 0)
            {
               Console.WriteLine("-----------------------------------------------------");
               Console.WriteLine("Started Join Timeseries Task.");
               Console.WriteLine("");

               foreach (ConfigNode j_ts in join_ts_list)
               {
                  FileName output = j_ts["output.file"].AsFileName();
                  string filter = j_ts["filter"].AsString();
                  bool search_sub_folders = j_ts["search.sub.folders", true].AsBool();
                  bool overwrite = j_ts["overwrite", true].AsBool();
                  TimeUnits tu = (TimeUnits)Enum.Parse(typeof(TimeUnits), (string)j_ts["output.time.units", "seconds"].AsString(), true);
                  bool ignore_wrong_path = j_ts["ignore.wrong.path", true].AsBool();

                  Console.Write("Processing file '{0}'", output.FullName);

                  if (!overwrite && System.IO.File.Exists(output.FullPath))
                  {
                     Console.WriteLine(" [ SKIPPED ]");
                     continue;
                  }

                  List<FilePath> to_search = new List<FilePath>();
                  ConfigNode search_path_list = j_ts.ChildNodes.Find(delegate(ConfigNode to_match) { return to_match.Name == "search.path.list"; });
                  if (search_path_list != null && search_path_list.NodeData.Count > 0)
                  {
                     foreach (KeywordData folder in search_path_list.NodeData.Values)
                     {
                        if (ignore_wrong_path)
                        {
                           if (!System.IO.Directory.Exists(folder.AsFilePath().Path))
                              continue;
                        }

                        to_search.Add(folder.AsFilePath());
                     }
                  }

                  if (to_search.Count <= 0)
                  {
                     to_search.Add(mred.resFolder);
                     search_sub_folders = true;
                  }

                  JoinTimeseriesByFolder(output, filter, to_search, search_sub_folders, overwrite, tu);
                  Console.WriteLine(" [ OK ]");
               }
            }
            Console.WriteLine("Finished Join Timeseries Task.");
            Console.WriteLine("-----------------------------------------------------");
            return true;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.OnEnd", ex);
            return false;
         }
      }

      //public override bool OnRunFail(object data)
      //{
      //   return base.OnRunFail(data);
      //}

      protected void LoadGlobalConfiguration()
      {
         try
         {
            save_data_files = script_block["save.data.files", true].AsBool();

            //load path for important folders.
            general_data = script_block["general.data.path"].AsFilePath();
            general_boundary_data = script_block["general.boundary.data.path"].AsFilePath();

            run_boundary_data = script_block["run.boundary.data.path", general_boundary_data.Path].AsFilePath();

            tools_working_folder = script_block["tools.working.folder.path"].AsFilePath();

            standard_names = script_block["standard.names", true].AsBool();
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.LoadGlobalConfiguration", ex);
            throw exception;
         }
      }

      protected void LoadToolsConfiguration()
      {
         try
         {
            List<ConfigNode> temp = script_block.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "tools.info"; });
            foreach (ConfigNode cn in temp)
            {
               tools_blocks[cn["tool.name"].AsString()] = cn;

               switch (cn["tool.name"].AsString().ToLower())
               {
                  case "fillmatrix":
                     tool_fillmatrix_info = cn;
                     break;
                  case "interpolate":
                     tool_interpolate_info = cn;
                     break;
                  case "glue":
                     tool_glue_info = cn;
                     break;
                  case "jointimeseries":
                     tool_jointimeseries_info = cn;
                     break;
               }
            }
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.LoadToolsConfiguration", ex);
            throw exception;
         }
      }

      protected void LoadTasksConfiguration()
      {
         try
         {
            practices_block = script_block.ChildNodes.Find(delegate(ConfigNode cfg) { return cfg.Name == "task.practices"; });
            LoadMeteoTaskConfiguration();
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.LoadTasksConfiguration", ex);
            throw exception;
         }
      }

      protected void LoadMeteoTaskConfiguration()
      {
         try
         {
            meteo_block = script_block.ChildNodes.Find(delegate(ConfigNode cfg) { return cfg.Name == "task.meteo"; });

            if (meteo_block == null)
               return;

            meteo_sources_blocks = meteo_block.ChildNodes.FindAll(delegate(ConfigNode to_match) { return to_match.Name == "source"; });
            if (meteo_sources_blocks == null || meteo_sources_blocks.Count <= 0)
               throw new Exception("No meteo source configuration block found.");

            meteo_hour_correction = meteo_block["hours_correction", 0].AsInt();
            meteo_file_name = meteo_block["hdf.file.name", "meteo.hdf5"].AsFileName();
            meteo_max_contiguous_skips = meteo_block["max.contiguous.skips", 8].AsInt();

            if (meteo_block.NodeData.ContainsKey("store.path"))
            {
               meteo_backup_files = true;
               meteo_store_path = meteo_block["store.path"].AsFilePath();
            }
            else
            {
               meteo_backup_files = false;
            }

            if (meteo_block.NodeData.ContainsKey("store.interpolated.path"))
            {
               meteo_backup_interpolated_files = true;
               meteo_store_int_path = meteo_block["store.interpolated.path"].AsFilePath();
            }
            else
            {
               meteo_backup_interpolated_files = false;
               meteo_interpolation_path = meteo_block["interpolation.path"].AsFilePath();
            }

            meteo_task = true;
            return;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.LoadMeteoTaskConfiguration", ex);
            throw exception;
         }
      }

      protected void SetupRun()
      {
         try
         {
            //Sets the name of the folder where the results of this run will be stored.
            store_folder = mred.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mred.sim.End.ToString("yyyyMMdd.HHmmss");

            //If this is not the first simulation, sets the name of the folder where the last run where stored.
            if (mred.simID != 1)
            {
               store_folder_old = mred.sim.Start.AddDays(-mred.log[mred.log.LastIndex].NodeData["sim.lenght"].AsDouble()).ToString("yyyyMMdd.HHmmss") +
                                  "-" +
                                  mred.sim.Start.ToString("yyyyMMdd.HHmmss");
            }

            //Creates the "store folder" in the general boundary folder
            if (!Directory.Exists(general_boundary_data.Path + store_folder))
               Directory.CreateDirectory(general_boundary_data.Path + store_folder);

            CopyIniFiles();

            CreatePracticesIDHDFFile();

            if (meteo_task)
            {
               Console.Write("Checking if the meteo file is available...");
               if (meteo_backup_files && MeteoFileExists())
               {
                  Console.WriteLine("[  OK  ]");
               }
               else
               {
                  Console.WriteLine("[ FAIL ]");

                  Console.Write("Creating the meteo file................");
                  CreateMeteoFile();
                  Console.WriteLine("[  OK  ]");
               }
            }

            SetupTemplates();
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.SetupRun", ex);
            throw exception;
         }
      }

      protected void SetupTemplates()
      {
         try
         {
            Console.WriteLine("Setup of templates...");

            if (mred.simID == 1 || !mred.changeTemplates)
            {
               foreach (InputFileTemplate itf in mred.templatesStart)
               {
                  if (standard_names)
                     itf.OutputName = itf.Name.Substring(0, itf.Name.Length - 2);

                  if (itf.Name.Contains("atmosphere"))
                  {
                     if (!meteo_backup_files)
                        itf.ReplaceList["<<meteo.sim.folder>>"] = run_boundary_data.Path;
                     else
                        itf.ReplaceList["<<meteo.sim.folder>>"] = run_boundary_data.Path + store_folder;

                  }
                  else if (itf.Name.Contains("nomfich") || itf.Name.Contains("vegetation"))
                  {
                     itf.ReplaceList["<<run.folder>>"] = mred.sim.SimDirectory.Path;
                  }
               }
            }
            else
            {
               foreach (InputFileTemplate itf in mred.templatesContinuation)
               {
                  if (standard_names)
                     itf.OutputName = itf.Name.Substring(0, itf.Name.Length - 2);

                  if (itf.Name.Contains("atmosphere"))
                  {
                     if (!meteo_backup_files)
                        itf.ReplaceList["<<meteo.sim.folder>>"] = run_boundary_data.Path;
                     else
                        itf.ReplaceList["<<meteo.sim.folder>>"] = run_boundary_data.Path + store_folder;
                  }
                  else if (itf.Name.Contains("nomfich") || itf.Name.Contains("vegetation"))
                  {
                     itf.ReplaceList["<<run.folder>>"] = mred.sim.SimDirectory.Path;
                  }
               }
            }
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.SetupTemplates", ex);
            throw exception;
         }
      }

      protected void CopyIniFiles()
      {
         try
         {
            bool copy_ini = false;
            string tag, dest_path;
            FilePath path = null;

            Console.WriteLine("Copying initialization files...");

            if (mred.simID == 1)
            {
               if (script_block.NodeData.ContainsKey("spin.up.folder"))
               {
                  path = script_block["spin.up.folder"].AsFilePath();
                  copy_ini = true;
               }
            }
            else //ToDo: If a "force restart" keyword is active, this must be ignored and the previous case must be used?
            {
               path = new FilePath(mred.storeFolder.Path + store_folder_old);
               copy_ini = true;
            }

            if (copy_ini)
            {
               List<FileName> file_list = new List<FileName>();
               if (FindFiles(path, file_list, "*.fin*") > 0)
               {
                  dest_path = mred.oldFolder.Path;

                  if (standard_names)
                  {
                     tag = "_old.";
                  }
                  else
                     tag = ".";

                  FileName dest = new FileName();
                  foreach (FileName orig in file_list)
                  {
                     dest.FullPath = dest_path + orig.Name + tag + orig.Extension;
                     File.Copy(orig.FullPath, dest.FullPath, true);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.CopyIniFiles", ex);
            throw exception;
         }
      }

      protected void CreatePracticesIDHDFFile()
      {
         try
         {
            if (practices_block != null)
            {
               if (tool_fillmatrix_info == null)
                  throw new Exception("Fillmatrix info block is missing.");

               practices_file = practices_block["file.name"].AsFileName();

               if (practices_block["search.first", true].AsBool())
               {
                  if (File.Exists(general_boundary_data.Path + store_folder + "\\" + practices_file.FullName))
                  {
                     if (general_boundary_data.Path != run_boundary_data.Path)
                        File.Copy(general_boundary_data.Path + store_folder + "\\" + practices_file.FullName, run_boundary_data.Path + practices_file.FullName, true);

                     return;
                  }
               }

               Console.WriteLine("Creating practices HDF file...");

               FillMatrix(script_block["model.grid.data"].AsFileName(),
                          new FileName(practices_file.Path + store_folder + "\\" + practices_file.FullName),
                          mred.sim.Start, mred.sim.End, practices_block["fillmatrix.template"].AsFileName(),
                          practices_block["config.file.name", "fillmatrix.dat"].AsFileName());

               if (general_boundary_data.Path != run_boundary_data.Path)
                  File.Copy(general_boundary_data.Path + store_folder + "\\" + practices_file.FullName, run_boundary_data.Path + practices_file.FullName, true);
            }
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.CreatePracticesIDHDFFile", ex);
            throw exception;
         }
      }

      protected bool MeteoFileExists()
      {
         try
         {
            if (File.Exists(general_boundary_data.Path + store_folder + "\\" + meteo_file_name.FullName))
               return true;

            return false;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.MeteoFileExists", ex);
            throw exception;
         }
      }

      protected void CreateMeteoFile()
      {
         try
         {
            DateTime start,
                     end;
            bool file_found = false,
                 is_first_instant = true,
                 exit = false;
            string date,
                   input_start_tag,
                   input_end_tag;
            long minimum_size;
            int skips = 0;
            FileName file = new FileName();
            FilePath meteo_bkp_folder = null,
                     output_path = new FilePath();
            ConfigNode folders_list;
            System.IO.FileInfo fi;

            files_to_glue.Clear();

            start = DefineCorrectStartDateForMeteoFilesSearch(mred.sim.Start);
            end = start.AddHours(5);

            Console.WriteLine("");
            while (!exit)
            {
               date = start.ToString("yyyyMMddHH") + "_" + end.ToString("yyyyMMddHH");

               foreach (ConfigNode src in meteo_sources_blocks)
               {
                  input_start_tag = src["start.tag", "D3_"].AsString();
                  input_end_tag = src["end.tag", ""].AsString();
                  minimum_size = src["minimum.size", 0].AsLong();

                  folders_list = src.ChildNodes.Find(delegate(ConfigNode to_match) { return to_match.Name == "folders.list"; });
                  if (folders_list == null) throw new Exception("No folders.list block found");

                  foreach (KeywordData folder in folders_list.NodeData.Values)
                  {
                     file_found = false;
                     file.FullPath = folder.AsFilePath().Path + input_start_tag + date + input_end_tag + ".hdf5";

                     Console.Write("Looking for file {0} in {1}", file.FullName, file.Path);
                     //Test tool see if Path exists                     
                     try
                     {
                        DirectoryInfo dInfo = new DirectoryInfo(file.Path);
                        DirectorySecurity dSecurity = dInfo.GetAccessControl();
                     }
                     catch (Exception ex)
                     {
                        Console.WriteLine("");
                        Console.WriteLine("Checking of path '{0}' throw an exception: {1}", file.Path, ex.Message);
                        continue;
                     }

                     if (System.IO.File.Exists(file.FullPath))
                     {
                        fi = new System.IO.FileInfo(file.FullPath);
                        if (fi.Length > minimum_size)
                        {
                           //look to see if there is already an interpolated file
                           if (meteo_backup_interpolated_files)
                           {
                              meteo_bkp_folder = src["backup_folder", meteo_backup_interpolated_files].AsFilePath();
                              if (System.IO.File.Exists(meteo_bkp_folder.Path + file.FullName))
                              {
                                 file.FullPath = "..\\" + meteo_bkp_folder.Path + file.FullName;
                                 file_found = true;
                                 Console.WriteLine("[ FOUND ]");
                              }
                           }

                           if (!file_found)
                           {

                              try
                              {
                                 if (CheckMeteoHDF(file, 6))
                                 {

                                    if (meteo_backup_interpolated_files)
                                       output_path.Path = "..\\" + meteo_bkp_folder.Path;
                                    else
                                       output_path.Path = "..\\" + meteo_interpolation_path.Path;

                                    FileName fgd = src["grid.data.file"].AsFileName();
                                    FileName tpl = src["template.file"].AsFileName();
                                    FileName act = src["action.file.name", "converttohdf5.dat"].AsFileName();

                                    InterpolateFile(file, fgd, output_path, script_block["model.grid.data"].AsFileName(), tpl, act);
                                    file.FullPath = output_path.Path + file.FullName;
                                    file_found = true;
                                    Console.WriteLine("[ INTERPOLATED ]");
                                 }
                              }
                              catch (Exception ex)
                              {
                                 Console.WriteLine("[ ERROR ]");
                                 while (ex != null)
                                 {
                                    Console.WriteLine("Message: {0}", ex.Message);
                                    ex = ex.InnerException;
                                 }
                              }


                           }
                        }
                     }

                     if (!file_found)
                        Console.WriteLine("[ NOT FOUND ]");
                     else
                        break;
                  }

                  if (file_found)
                     break;
               }

               if (!file_found)
               {
                  if ((++skips) > meteo_max_contiguous_skips)
                     throw new Exception("Max number of skips reached during meteo creation file.");

                  if (is_first_instant) //first file in the list
                  {
                     end = start.AddHours(-1);
                     start = start.AddHours(-6);
                  }
                  else
                  {
                     start = end.AddHours(1);
                     end = start.AddHours(5);
                  }
               }
               else
               {
                  if (is_first_instant)
                  {
                     is_first_instant = false;

                     if (skips > 0)
                     {
                        start = DefineCorrectStartDateForMeteoFilesSearch(mred.sim.Start);
                        end = start.AddHours(5);
                     }
                     else
                     {
                        start = end.AddHours(1);
                        end = start.AddHours(5);
                     }
                  }
                  else
                  {
                     start = end.AddHours(1);
                     end = start.AddHours(5);
                  }

                  skips = 0;
                  files_to_glue.Add(new FileName(file.FullPath));

                  if (start >= mred.sim.End)
                     exit = true;
               }
            }

            if (!meteo_backup_files)
               GlueFiles(files_to_glue, new FileName("..\\" + run_boundary_data.Path + meteo_file_name.FullName), false);
            else
               GlueFiles(files_to_glue, new FileName("..\\" + run_boundary_data.Path + store_folder + "\\" + meteo_file_name.FullName), false);

            if (meteo_backup_files && run_boundary_data.Path != general_boundary_data.Path)
               File.Copy(run_boundary_data.Path + store_folder + "\\" + meteo_file_name.FullName, general_boundary_data.Path + store_folder + "\\" + meteo_file_name.FullName, true);

         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.MeteoFileExists", ex);
            throw exception;
         }
      }

      protected void GlueFiles(List<FileName> files_to_glue, FileName output, bool is_3d)
      {
         try
         {
            tool_glue.Reset();

            tool_glue.FilesToGlue2 = files_to_glue;

            tool_glue.AppName = tool_glue_info["exe.path"].AsFileName().FullName;
            tool_glue.AppPath = tool_glue_info["exe.path"].AsFileName().Path;
            tool_glue.WorkingDirectory = tool_glue_info["working.folder", tools_working_folder].AsFilePath().Path;
            tool_glue.Output = output.FullPath;
            tool_glue.Is3DFile = is_3d;
            tool_glue.ThrowExceptionOnError = true;

            tool_glue.Glue();
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.GlueFiles", ex);
            throw exception;
         }
      }

      protected void InterpolateFile(FileName input, FileName father_grid, FilePath output_path, FileName model_grid, FileName template_file, FileName action_file)
      {
         try
         {
            replace_list.Clear();
            replace_list["<<input.hdf>>"] = input.FullPath;
            replace_list["<<father.grid>>"] = father_grid.FullPath;
            replace_list["<<output.hdf>>"] = output_path.Path + input.FullName;
            replace_list["<<model.grid>>"] = model_grid.FullPath;

            string working_folder = tool_interpolate_info["working.folder", tools_working_folder].AsFilePath().Path;

            TextFile.Replace(template_file.FullPath, working_folder + action_file.FullName, ref replace_list);

            tool.Reset();

            tool.Arguments = "";
            tool.WorkingDirectory = working_folder;
            tool.Executable = tool_interpolate_info["exe.path"].AsString();
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "successfully terminated";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            bool result = tool.Run();
            if (!result)
            {
               if (tool.Exception != null)
                  throw tool.Exception;
               else
                  throw new Exception("Unknown error.");
            }
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.InterpolateFile", ex);
            throw exception;
         }
      }

      protected void FillMatrix(FileName model_grid, FileName output_file, DateTime start, DateTime end, FileName template_file, FileName action_file)
      {
         try
         {
            replace_list.Clear();
            replace_list["<<grid.data>>"] = model_grid.FullPath;
            replace_list["<<output.hdf>>"] = output_file.FullPath;
            replace_list["<<start>>"] = start.ToString("yyyy MM dd HH 00 00");
            replace_list["<<end>>"] = end.ToString("yyyy MM dd HH 00 00");

            string working_folder = tool_fillmatrix_info["working.folder", tools_working_folder].AsFilePath().Path;

            TextFile.Replace(template_file.FullPath, working_folder + action_file.FullName, ref replace_list);

            tool.Reset();

            tool.Arguments = "";
            tool.WorkingDirectory = working_folder;
            tool.Executable = tool_fillmatrix_info["exe.path"].AsString();
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "Finished..";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            bool result = tool.Run();
            if (!result)
            {
               if (tool.Exception != null)
                  throw tool.Exception;
               else
                  throw new Exception("Fillmatrix failed");
            }
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.FillMatrix", ex);
            throw exception;
         }
      }

      protected void JoinTimeseriesByFolder(FileName output, string filter, List<FilePath> folders_to_search, bool search_sub_folders = true, bool overwrite = true, TimeUnits tu = TimeUnits.SECONDS)
      {
         try
         {
            if (folders_to_search == null || folders_to_search.Count <= 0)
               throw new Exception("No folders to search timeseries were defined.");

            if (string.IsNullOrWhiteSpace(output.FullPath))
               throw new Exception("No timeseries output was defined");

            if (!System.IO.Directory.Exists(output.Path))
               throw new Exception("The path '" + output.Path + "' doesn't exist.");

            if (System.IO.File.Exists(output.FullPath) && !overwrite)
               throw new Exception("The file '" + output.FullName + "' exists and overwrite is set to false.");

            if (string.IsNullOrWhiteSpace(filter))
               throw new Exception("Filter was not set.");

            int found = 0;
            List<FileName> files = new List<FileName>();
            List<TimeSeries> timeSeries = new List<TimeSeries>();
            System.IO.SearchOption so;

            if (search_sub_folders)
               so = System.IO.SearchOption.AllDirectories;
            else
               so = System.IO.SearchOption.TopDirectoryOnly;

            foreach (FilePath path in folders_to_search)
            {
               found = FindFiles(path, files, filter, so);

               foreach (FileName fi in files)
               {
                  TimeSeries newTS = new TimeSeries();
                  newTS.Load(fi);
                  timeSeries.Add(newTS);
               }
            }

            if (timeSeries.Count <= 0)
               return;

            //Finds the Start Date for the output timeseries in the list of loaded timeseries
            DateTime start = timeSeries[0].StartInstant;
            for (int i = 1; i < timeSeries.Count; i++)
            {
               if (timeSeries[i].StartInstant < start)
                  start = timeSeries[i].StartInstant;
            }

            TimeSeries outTS = new TimeSeries();
            outTS.StartInstant = start;
            outTS.TimeUnits = tu;

            foreach (Column col in timeSeries[0].Columns)
            {
               Column newCol = new Column(col.ColumnType);
               newCol.Header = col.Header;
               outTS.AddColumn(newCol);
            }

            foreach (TimeSeries toJoin in timeSeries)
               outTS.AddTimeSeries(toJoin);

            outTS.Save(output);
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.JoinTimeseriesByFolder", ex);
            throw exception;
         }
      }

      protected DateTime DefineCorrectStartDateForMeteoFilesSearch(DateTime run_start)
      {
         try
         {
            int hour;

            hour = run_start.Hour;

            if (hour >= 1 && hour < 7)
               return new DateTime(run_start.Year, run_start.Month, run_start.Day, 1, 0, 0);

            if (hour >= 7 && hour < 13)
               return new DateTime(run_start.Year, run_start.Month, run_start.Day, 7, 0, 0);

            if (hour >= 13 && hour < 19)
               return new DateTime(run_start.Year, run_start.Month, run_start.Day, 13, 0, 0);

            if (hour >= 19)
               return new DateTime(run_start.Year, run_start.Month, run_start.Day, 19, 0, 0);

            DateTime new_date = run_start.AddDays(-1);
            return new DateTime(new_date.Year, new_date.Month, new_date.Day, 19, 0, 0);
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.DefineCorrectStartDateForMeteoFilesSearch", ex);
            throw exception;
         }
      }

      protected void ClearFolder(FilePath folderToClean)
      {
         try
         {
            System.IO.DirectoryInfo directory = new DirectoryInfo(folderToClean.Path);
            foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
               fileToDelete.Delete();
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.ClearFolder", ex);
            throw exception;
         }
      }

      protected int FindFiles(FilePath path,
                              List<FileName> listOfFiles,
                              string searchPattern,
                              System.IO.SearchOption so = System.IO.SearchOption.TopDirectoryOnly)
      {
         try
         {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path.Path);
            System.IO.FileInfo[] aryFi = di.GetFiles(searchPattern, so);

            foreach (System.IO.FileInfo fi in aryFi)
               listOfFiles.Add(new FileName(fi.FullName));

            return aryFi.Length;
         }
         catch (Exception ex)
         {
            exception_raised = true;
            exception = new Exception("ScriptV5.FindFiles", ex);
            throw exception;
         }
      }

      public override Exception ExceptionRaised()
      {
         if (exception_raised)
            return exception;

         return null;
      }

      public bool CheckMeteoHDF(FileName file, int number_of_instants, bool exception_warn = false)
      {
         try
         {
            HDFObjectInfo group, dataset;

            HDF.HDF input = new HDF.HDF();
            HDFObjectInfo input_root = new HDFObjectInfo();

            input.InitializeLibrary();

            input.OpenHDF(file, H5F.OpenMode.ACC_RDONLY);
            input.GetTree(input_root, input.FileID, "/", "/");

            input.CloseHDF();

            HDFObjectInfo time = input_root.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Time"; });
            if (time == null) throw new Exception("Missing 'time' group.");
            if (time.Children == null) throw new Exception("Missing 'time' datasets.");
            if (time.Children.Count != number_of_instants) throw new Exception("Wrong number of 'time' datasets");

            HDFObjectInfo results = input_root.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Results"; });
            if (results == null) throw new Exception("Missing 'results' group.");
            if (results.Children == null) throw new Exception("Missing 'results' datasets.");
            if (results.Children.Count < 5) throw new Exception("Wrong number of 'results' datasets");
            group = results.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "air temperature"; });
            if (group == null) throw new Exception("Missing 'air temperature' group.");
            if (group.Children == null) return false;
            if (group.Children.Count != number_of_instants) return false;
            group = results.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "precipitation"; });
            if (group == null) throw new Exception("Missing 'precipitation' group.");
            if (group.Children == null) return false;
            if (group.Children.Count != number_of_instants) return false;
            group = results.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "relative humidity"; });
            if (group == null) throw new Exception("Missing 'relative humidity' group.");
            if (group.Children == null) return false;
            if (group.Children.Count != number_of_instants) return false;
            group = results.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "solar radiation"; });
            if (group == null) throw new Exception("Missing 'solar radiation' group.");
            if (group.Children == null) return false;
            if (group.Children.Count != number_of_instants) return false;
            group = results.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "wind modulus"; });
            if (group == null) throw new Exception("Missing 'wind modulus' group.");
            if (group.Children == null) return false;
            if (group.Children.Count != number_of_instants) return false;

            HDFObjectInfo grid = input_root.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Grid"; });
            if (grid == null) return false;
            if (grid.Children == null) return false;
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Bathymetry"; });
            if (dataset == null) throw new Exception("Missing 'Bathymetry' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "ConnectionX"; });
            if (dataset == null) throw new Exception("Missing 'ConnectionX' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "ConnectionY"; });
            if (dataset == null) throw new Exception("Missing 'ConnectionY' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Define Cells"; });
            if (dataset == null) throw new Exception("Missing 'Define Cells' dataset.");
            //dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "LandUse"; });
            //if (dataset == null) throw new Exception("Missing 'LandUse' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Latitude"; });
            if (dataset == null) throw new Exception("Missing 'Latitude' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "Longitude"; });
            if (dataset == null) throw new Exception("Missing Longitude' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "WaterPoints2D"; });
            if (dataset == null) throw new Exception("Missing 'WaterPoints2D' dataset.");
            dataset = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "WaterPoints3D"; });
            if (dataset == null) throw new Exception("Missing 'WaterPoints3D' dataset.");
            //group = grid.Children.Find(delegate(HDFObjectInfo node) { return node.Name == "VerticalZ"; });
            //if (group == null) throw new Exception("Missing 'VerticalZ' group.");
            //if (group.Children == null) throw new Exception("Missing 'VerticalZ' datasets.");
            //if (group.Children.Count != number_of_instants) throw new Exception("Wrong number of 'VerticalZ' datasets.");

            return true;
         }
         catch (Exception ex)
         {
            if (exception_warn)
            {
               Console.WriteLine("");
               Console.WriteLine(ex.Message);
               Console.WriteLine("");
            }
            return false;
         }
      }
   }
}
