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
   public class ScriptV5OnlyTSGlue : BCIMohidSim
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
      #endregion DATA

      public ScriptV5OnlyTSGlue()
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
         return true;
      }

      public override bool OnSimStart(object data)
      {
         return true;
      }

      public override bool OnSimEnd(object data)
      {
         return true;
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

      public override bool OnRunFail(object data)
      {
         return true;
      }

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

   }
}
