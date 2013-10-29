//DLLNAME: MohidToolboxCore
//DLLNAME: System

using System;
using System.Collections.Generic;
using System.Text;

using Mohid;
using Mohid.Configuration;
using Mohid.Files;
using Mohid.Script;
using Mohid.Software;
using Mohid.HDF;

namespace MohidTaskInterfaces
{
   public class DateInterval
   {
      public DateTime Start;
      public DateTime End;
      public bool UseStart;
      public bool UseEnd;

      public DateInterval()
      {
         UseStart = false;
         UseEnd = false;
      }
   }

   public class InterpolateAndGlueMeteo : IMohidTask
   {
      Exception last_exception;

      Dictionary<string, ConfigNode> tools_blocks;
      ConfigNode tool_interpolate_info;
      ConfigNode tool_glue_info;

      Dictionary<string, string> replace_list;

      ExternalApp tool;
      HDFGlue tool_glue;

      FilePath tools_working_folder;

      DateInterval interval;

      public InterpolateAndGlueMeteo()
      {
         Reset();
      }

      public bool Run(ConfigNode cfg)
      {
         bool result = true;

         try
         {
            LoadGlobalKeywords(cfg);
            LoadToolsConfiguration(cfg);
            int task_number = 1;

            List<ConfigNode> tasks = cfg.ChildNodes.FindAll((node) => node.Name.StartsWith("task."));

            foreach (ConfigNode task in tasks)
            {
               switch(task.Name)
               {
                  case "task.interpolation":
                     result = TaskInterpolation(task);
                     break;
                  case "task.glue":
                     result = TaskGlue(task);
                     break;
                  default:
                     throw new Exception("Unknown task in configuration file.");
               }

               if (!result && cfg["stop.on.task.failure", true].AsBool())
               {
                  if (last_exception != null)
                     throw last_exception;
                  throw new Exception("Task " + task_number + " (" + task.Name + ") failed.");
               }

               task_number++;
            }
         }
         catch (Exception ex)
         {
            last_exception = ex;
            result = false;
         }

         return result;
      }

      protected void LoadInterval(ConfigNode cfg)
      {
         if (cfg.NodeData.ContainsKey("start"))
         {
            interval.Start = cfg["start"].AsDateTime("yyyy M d H m s");
            interval.UseStart = true;
         }
         else
         {
            interval.UseStart = false;
         }

         if (cfg.NodeData.ContainsKey("end"))
         {
            //Console.WriteLine("{0}", cfg["end"].AsString());
            interval.End = cfg["end"].AsDateTime("yyyy M d H m s");
            interval.UseEnd = true;
         }
         else
         {
            interval.UseEnd = false;
         }
      }

      protected bool TaskInterpolation(ConfigNode cfg)
      {
         bool result = true;
         bool r = true;

         try
         {
            LoadInterval(cfg);

            bool ignore_errors = cfg["ignore.errors", true].AsBool();
            string search_pattern = cfg["search.pattern", "*.hdf5"].AsString();
            bool overwrite = cfg["overwrite", false].AsBool();
            List<FileName> files_to_process;

            if (interval.UseStart || interval.UseEnd)
               files_to_process = CreateFileList(ListOfPaths(cfg, "search.path.list"), search_pattern, interval, true);            
            else
               files_to_process = CreateFileList(ListOfPaths(cfg, "search.path.list"), search_pattern, null, true);            

            FileName output = new FileName();
            int tag = 1;
            bool use_tag = cfg["use.tag", false].AsBool();
            FileName father_grid = cfg["input.grid"].AsFileName();
            FileName model_grid = cfg["output.grid"].AsFileName();
            FileName template = cfg["template.file"].AsFileName();
            FileName action = cfg["action.file", "converttohdf5action.dat"].AsFileName();

            bool process;
            foreach (FileName input in files_to_process)
            {
               process = true;
               output.Path = cfg["output.path"].AsString();

               if (use_tag)
                  output.FullName = input.Name + "_" + tag.ToString() + input.Extension;
               else
                  output.FullName = input.FullName;

               if (!overwrite)
                  if (System.IO.File.Exists(output.FullPath))
                     process = false;

               if (process)
               {
                  r = InterpolateFile(input, father_grid, output, model_grid, template, action);
                  if (!r && !ignore_errors)
                     throw last_exception;
               }
               tag++;
            }

         }
         catch (Exception ex)
         {
            last_exception = ex;
            result = false;
         }

         return result;
      }

      protected bool TaskGlue(ConfigNode cfg)
      {
         bool result = true;
         bool r = true;

         try
         {
            //Console.WriteLine("1");
            LoadInterval(cfg);

            bool ignore_errors = cfg["ignore.errors", true].AsBool();
            string search_pattern = cfg["search.pattern", "*.hdf5"].AsString();

            List<FileName> files_to_process;
            //Console.WriteLine("2");
            if (interval.UseStart || interval.UseEnd)
               files_to_process = CreateFileList(ListOfPaths(cfg, "search.path.list"), search_pattern, interval, true);
            else
               files_to_process = CreateFileList(ListOfPaths(cfg, "search.path.list"), search_pattern, null, true);

            //Console.WriteLine("3");
            if (files_to_process == null)
            {
               if (last_exception != null)
                  throw last_exception;
               return true;
            }

            files_to_process.Sort((x, y) => string.Compare(x.FullName, y.FullName));

            //Console.WriteLine("4");
            r = GlueFiles(files_to_process, cfg["output.file"].AsFileName(), cfg["is.3d", false].AsBool());
            if (!r && !ignore_errors)
               throw last_exception;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            result = false;
         }

         return result;
      }

      protected List<FilePath> ListOfPaths(ConfigNode cfg, string name)
      {
         List<FilePath> folders_to_search = new List<FilePath>();

         ConfigNode folders_block = cfg.ChildNodes.Find((node) => node.Name == name);
         foreach (KeywordData folder in folders_block.NodeData.Values)
         {
            folders_to_search.Add(new FilePath(folder.AsString()));
         }

         return folders_to_search;
      }

      public Exception LastException
      {
         get { return last_exception; }
      }

      public bool Reset()
      {         
         last_exception = null;

         tools_blocks = new Dictionary<string, ConfigNode>();
         tool_interpolate_info = null;
         tool_glue_info = null;

         replace_list = new Dictionary<string, string>();
         tool = new ExternalApp();
         tool_glue = new HDFGlue();

         interval = new DateInterval();

         return true;
      }

      protected void LoadGlobalKeywords(ConfigNode cfg)
      {
         try
         {
            tools_working_folder = cfg["tools.working.folder", ".\\"].AsFilePath();
         }
         catch (Exception ex)
         {            
            throw new Exception ("Global keywords load failed with the message " + ex.Message);
         }
      }

      protected void LoadToolsConfiguration(ConfigNode cfg)
      {
         try
         {
            List<ConfigNode> temp = cfg.ChildNodes.FindAll((node) => node.Name == "tools.info");
            foreach (ConfigNode cn in temp)
            {
               tools_blocks[cn["tool.name"].AsString()] = cn;

               switch (cn["tool.name"].AsString().ToLower())
               {
                  case "interpolate":
                     tool_interpolate_info = cn;
                     break;
                  case "glue":
                     tool_glue_info = cn;
                     break;
                  default:
                     throw new Exception("Unknown tool information found.");
               }
            }
         }
         catch (Exception ex)
         {
            throw new Exception("Tools configuration load failed with the message '" + ex.Message + "'");
         }
      }

      protected bool GlueFiles(List<FileName> files_to_glue, FileName output, bool is_3d)
      {
         //Console.WriteLine("5");
         bool result = true;

         try
         {
            Console.Write("");
            Console.Write("Glueing files...");
            tool_glue.Reset();

            tool_glue.FilesToGlue2 = files_to_glue;

            tool_glue.AppName = tool_glue_info["exe.path"].AsFileName().FullName;
            tool_glue.AppPath = tool_glue_info["exe.path"].AsFileName().Path;
            tool_glue.WorkingDirectory = tool_glue_info["working.folder", tools_working_folder].AsFilePath().Path;
            tool_glue.Output = output.FullPath;
            tool_glue.Is3DFile = is_3d;
            tool_glue.ThrowExceptionOnError = true;

            tool_glue.Glue();

            Console.WriteLine(" [   OK   ]");
         }
         catch (Exception ex)
         {
            last_exception = ex;
            Console.WriteLine(" [  FAIL  ]");
            result = false;
         }

         return result;
      }

      protected bool InterpolateFile(FileName input, FileName father_grid, FileName output, FileName model_grid, FileName template_file, FileName action_file)
      {
         bool result = true;

         try
         {
            Console.Write("Interpolating '{0}'", input.FullName);

            replace_list.Clear();
            replace_list["<<input.hdf>>"] = input.FullPath;
            replace_list["<<father.grid>>"] = father_grid.FullPath;
            replace_list["<<output.hdf>>"] = output.FullPath;
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

            bool r = tool.Run();
            if (!r)
            {
               if (tool.Exception != null)
                  throw tool.Exception;
               else
                  throw new Exception("Unknown error.");
            }

            Console.WriteLine(" [   OK   ]");
         }
         catch (Exception ex)
         {
            last_exception = ex;
            result = false;
            Console.WriteLine(" [  FAIL  ]");
         }

         return result;
      }

      protected List<FileName> CreateFileList(List<FilePath> folders_to_search, string search_pattern, DateInterval interval = null, bool search_sub_folders = true)
      {
         try
         {
            System.IO.SearchOption so;
            int found = 0;
            List<FileName> files_to_process = new List<FileName>();            
            
            last_exception = null;

            if (search_sub_folders)
               so = System.IO.SearchOption.AllDirectories;
            else
               so = System.IO.SearchOption.TopDirectoryOnly;

            foreach (FilePath path in folders_to_search)
            {               
               found += FindFiles(files_to_process, path, search_pattern, false, so, interval);
            }

            if (found < 0)               
               return null;

            return files_to_process;
         }
         catch (Exception ex)
         {
            last_exception = ex;            
            return null;
         }         
      }

      protected int FindFiles(List<FileName> files_to_process,
                              FilePath path,
                              string searchPattern,
                              bool clear,
                              System.IO.SearchOption so = System.IO.SearchOption.TopDirectoryOnly,
                              DateInterval interval = null)
      {
         try
         {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path.Path);
            System.IO.FileInfo[] aryFi = di.GetFiles(searchPattern, so);

            if (clear)
               files_to_process.Clear();

            int found = 0;
            bool use;            
            DateTime date;

            foreach (System.IO.FileInfo fi in aryFi)
            {
               use = true;

               if (interval != null)
               {
                  if (interval.UseStart)
                  {
                     date = DateTime.ParseExact(fi.Name.Substring(fi.Name.Length - 26, 10), "yyyyMMddHH", System.Globalization.CultureInfo.InvariantCulture);
                     if (date < interval.Start)
                        use = false;
                  }

                  if (interval.UseEnd)
                  {                     
                     date = DateTime.ParseExact(fi.Name.Substring(fi.Name.Length - 15, 10), "yyyyMMddHH", System.Globalization.CultureInfo.InvariantCulture);
                     if (date > interval.End)
                        use = false;
                  }
               }

               if (use)
               {
                  //Console.WriteLine("{0}", fi.FullName);
                  files_to_process.Add(new FileName(fi.FullName));
                  found++;
               }
            }

            last_exception = null;
            return found;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return -1;
         }
      }
   }
}
