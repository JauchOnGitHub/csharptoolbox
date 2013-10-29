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
   public class FileInfo
   {
      public FileName FileName { get; set; }
      public string FileExtension { get; set; }
      public DateTime DateOnFileName { get; set; }
      public string DateOnFileMask { get; set; }
      public bool Processed { get; set; }

      protected virtual void Init()
      {
         FileName = new FileName();
         FileExtension = "";
         DateOnFileName = DateTime.Today;
         DateOnFileMask = ""; // "yyyy_MM_dd_HH_mm_ss";
         Processed = false;
      }

      public FileInfo()
      {
         Init();
      }

      public FileInfo(FileName fileName, string dateOnFileMask = null)
      {
         Init();

         if (string.IsNullOrWhiteSpace(fileName.FullName))
            throw new Exception("File name parameter is missing");

         FileName = new FileName(fileName.FullPath);
         FileExtension = fileName.Extension;
         DateOnFileMask = dateOnFileMask;

         if (!string.IsNullOrWhiteSpace(DateOnFileMask))
            DateOnFileName = DateTime.ParseExact(FileName.Name.Substring(FileName.Name.Length - DateOnFileMask.Length), DateOnFileMask, null);
      }

   }

   public class CPTEC2HDFv1 : IMohidTask
   {
      Exception last_exception;
      ConfigNode cfg;

      bool use_start_date;
      DateTime start_date;

      protected Dictionary<string, string> replace_list;
      protected ExternalApp tool;
      StringBuilder str;
      List<FileInfo> files_to_process;

      bool clean_working_folder;

      FileName grib_to_grib2_exe;
      FileName grib2_to_netcdf_exe;
      FileName netcdf_to_hdf5_exe;
      FileName netcdf_to_hdf5_template;
      FilePath working_folder;
      FilePath store_folder;
      FilePath store_path;

      string last_store_name;

      int starting_hour;
      int hours_to_glue;
      bool glue_all;
      bool replace_glued_files;
      bool first_glue;
      FilePath store_glue_folder;
      int actual_index;
      string last_stored_path;
      List<FileName> files_to_glue;
      List<string> files_temp;
      FileName glue_exe;
      bool store_files_for_glue;


      bool glue_files;
      bool replace_existing_files;
      bool ignore_errors;
      bool search_sub_folders;
      List<FilePath> folders_to_search;

      bool process_hdf;
      bool replace_processed_files;
      FileName hdf_processor_exe;
      FileName hdf_processor_template;
      string output_tag;

      StringBuilder log;
      bool do_log;
      bool log_only_failures;
      bool add_date_time;
      FileName log_file;
      string log_dateonfile_format;

      bool glue_as_mm5;
      bool glue_on_separeted_folders;

      bool first_glue_as_mm5;
      bool second_glue_as_mm5;

      public CPTEC2HDFv1()
      {
         last_exception = null;
         replace_list = new Dictionary<string, string>();
         tool = new ExternalApp();
         str = new StringBuilder();
         replace_existing_files = false;
         search_sub_folders = false;
         folders_to_search = new List<FilePath>();
         files_to_process = new List<FileInfo>();
         ignore_errors = true;
         store_path = new FilePath();
         last_stored_path = "";
         glue_files = true;
         files_to_glue = new List<FileName>();
         files_temp = new List<string>();
         store_files_for_glue = true;
         actual_index = 1;
         first_glue = true;
         process_hdf = false;
         replace_processed_files = false;
         log = new StringBuilder();
         log_only_failures = false;
         do_log = false;
         add_date_time = true;
         log_dateonfile_format = "yyyyMMdd.HHmmss";
         use_start_date = false;
         glue_as_mm5 = false;
         clean_working_folder = true;
         glue_on_separeted_folders = false;
         actual_index = 1;
      }

      bool IMohidTask.Run(ConfigNode cfg)
      {
         last_exception = null;
         this.cfg = cfg;
         bool result = true;

         if (!LoadGlobalConfig())
            return false;

         files_to_process.Clear();

         for (; ; )
         {
            if (!CreateFileList())
            {
               if (last_exception == null)
               {
                  result = true;
                  break;
               }
               else
               {
                  result = false;
                  break;
               }
            }

            if (!RunForFileList())
            {
               result = false;
               break;
            }
         }

         if (do_log && log.Length > 0)
         {
            try
            {
               if (add_date_time)
                  log_file.FullName = log_file.Name + "_" + DateTime.Now.ToString(log_dateonfile_format) + "." + log_file.Extension;

               TextFile log_f = new TextFile(log_file);
               log_f.OpenNewToWrite();

               log_f.Write(log);

               log_f.Close();
            }
            catch (Exception ex)
            {
               last_exception = ex;
               result = false;
            }
         }

         return result;
      }

      protected bool LoadGlobalConfig()
      {
         try
         {
            Console.Write("Loading Global Configuration......");
            use_start_date = cfg.NodeData.ContainsKey("start.date");
            if (use_start_date)
               start_date = cfg["start.date"].AsDateTime("dd/MM/yyyy");
            grib_to_grib2_exe = cfg["grib.to.grib2.exe"].AsFileName();
            grib2_to_netcdf_exe = cfg["grib2.to.netcdf.exe"].AsFileName();
            netcdf_to_hdf5_exe = cfg["netcdf.to.hdf5.exe"].AsFileName();
            netcdf_to_hdf5_template = cfg["netcdf.to.hdf5.template"].AsFileName();
            working_folder = cfg["working.folder"].AsFilePath();
            store_folder = cfg["store.folder"].AsFilePath();
            replace_existing_files = cfg["replace.files", false].AsBool();
            ignore_errors = cfg["ignore.errors", true].AsBool();
            search_sub_folders = cfg["search.sub.folders", false].AsBool();
            clean_working_folder = cfg["clean.working.folder", true].AsBool();

            ConfigNode glue_block = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "glue.config"; });
            if (glue_block != null)
            {
               glue_files = true;
               glue_exe = glue_block["glue.exe"].AsFileName();
               starting_hour = glue_block["starting.hour.index", 1].AsInt();
               hours_to_glue = glue_block["hours.to.glue.by.folder", 12].AsInt();
               replace_glued_files = glue_block["replace.files", false].AsBool();
               glue_all = glue_block["glue.all", false].AsBool();
               store_glue_folder = glue_block["store.glue.path"].AsFilePath();
               glue_as_mm5 = glue_block["glue.as.mm5", false].AsBool();
               glue_on_separeted_folders = glue_block["use.sub.folders", false].AsBool();
            }
            else
               glue_files = false;

            ConfigNode process_block = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "hdf5.process"; });
            if (process_block != null)
            {
               process_hdf = true;
               replace_processed_files = process_block["replace.files", false].AsBool();
               hdf_processor_exe = process_block["process.exe"].AsFileName();
               hdf_processor_template = process_block["config.template"].AsFileName();
               output_tag = process_block["output.tag", "_mohid"].AsString();
            }
            else
               process_hdf = false;

            ConfigNode log_block = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "log.config"; });
            if (log_block != null)
            {
               do_log = true;
               add_date_time = log_block["add.date.time", true].AsBool();
               log_file = log_block["log.file"].AsFileName();
               log_only_failures = log_block["only.failures", false].AsBool();
               log_dateonfile_format = log_block["date.format", "yyyyMMdd.HHmmss"].AsString();
            }
            else
               do_log = false;

            ConfigNode folders_block = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "folders.to.search"; });
            foreach (KeywordData folder in folders_block.NodeData.Values)
            {
               folders_to_search.Add(new FilePath(folder.AsString()));
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("[FAIL]");
            last_exception = ex;
            return false;
         }
         Console.WriteLine("[OK]");
         return true;
      }

      protected bool CreateFileList()
      {
         try
         {
            int found = 0;

            Console.Write("Searching files to process........");

            System.IO.SearchOption so;

            if (search_sub_folders)
               so = System.IO.SearchOption.AllDirectories;
            else
               so = System.IO.SearchOption.TopDirectoryOnly;

            foreach (FilePath path in folders_to_search)
            {
               found += FindFiles(path, "*.grb", false, so);
            }

            if (found <= 0)
            {
               last_exception = null;
               Console.WriteLine("[NO NEW FILES]");
               return false;
            }

            if (found == 1)
               Console.WriteLine("[1 NEW FILE FOUND]");
            else
               Console.WriteLine("[{0} NEW FILES FOUND]", found);

            return true;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            Console.WriteLine("[FAIL]");
            return false;
         }

      }

      protected int FindFiles(FilePath path,
                               string searchPattern,
                               bool clear,
                               System.IO.SearchOption so = System.IO.SearchOption.TopDirectoryOnly)
      {
         try
         {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path.Path);
            System.IO.FileInfo[] aryFi = di.GetFiles(searchPattern, so);

            if (clear)
               files_to_process.Clear();

            List<FileInfo> temp = new List<FileInfo>();

            int found = 0;

            foreach (System.IO.FileInfo fi in aryFi)
            {

               if (!files_to_process.Exists(delegate(FileInfo file) { return file.FileName.FullPath == fi.FullName; }))
               {
                  FileInfo file = new FileInfo(new FileName(fi.FullName));
                  temp.Add(file);
                  found++;
               }
            }

            if (found > 0)
            {
               files_to_process.AddRange(temp);
            }

            last_exception = null;
            return found;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return 0;
         }
      }

      protected bool RunForFileList()
      {
         bool errors = false;
         int index = 0;

         try
         {
            Console.WriteLine("");
            Console.WriteLine("=========================");
            Console.WriteLine("Starting file processing.");
            Console.WriteLine("=========================");
            Console.WriteLine("");

            actual_index = 0;
            foreach (FileInfo file_info in files_to_process)
            {
               index++;
               actual_index++;

               if (!file_info.Processed)
               {
                  file_info.Processed = true;

                  try
                  {
                     Console.WriteLine("  Processing file: {0}", file_info.FileName.FullName);

                     if (!RunForFile(new FileName(file_info.FileName.FullPath)))
                     {
                        if (do_log)
                        {
                           log.AppendLine(index.ToString() + ": [FAIL] " + file_info.FileName.FullPath);
                           if (last_exception != null)
                              log.AppendLine(last_exception.Message);
                        }
                        if (!ignore_errors)
                           return false;
                        errors = true;
                     }
                     else if (do_log && !log_only_failures)
                     {
                        log.AppendLine(index.ToString() + ": [SUCCESS] " + file_info.FileName.FullPath);
                     }

                  }
                  catch (Exception ex)
                  {
                     if (!ignore_errors)
                     {
                        last_exception = ex;
                        return false;
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("File processing finished with ERRORS.");
            last_exception = ex;
            return false;
         }

         if (errors)
            Console.WriteLine("File processing finished with ERRORS.");
         else
            Console.WriteLine("File processing finished OK.");

         return true;
      }

      protected bool RunForFile(FileName file_to_process)
      {
         bool result = true;
         bool skip_conversion = false;
         try
         {
            Console.Write("    Preparing store folder............");
            if (!SetupStoreFolder(file_to_process))
            {
               Console.WriteLine("[FAIL]");
               result = false;
            }
            else
            {
               Console.WriteLine("[OK]");
            }

            if (result && !replace_existing_files)
            {
               Console.Write("    Checking if file already exists...");
               string file_to_search;

               file_to_search = store_path.Path + file_to_process.Name + ".hdf5";

               if (System.IO.File.Exists(file_to_search))
               {
                  Console.WriteLine("[SKIPPED]");
                  skip_conversion = true;
                  result = true;
               }
               else
                  Console.WriteLine("[OK]");
            }

            if (result)
            {
               Console.Write("    Copying file to process...........");
               try
               {
                  System.IO.File.Copy(file_to_process.FullPath, working_folder.Path + file_to_process.FullName, true);
                  Console.WriteLine("[OK]");
               }
               catch (Exception ex)
               {
                  Console.WriteLine("[FAIL]");
                  last_exception = ex;
                  result = false;
               }
            }

            if (result && !skip_conversion)
            {
               Console.Write("    Converting from GRIB to GRIB2.....");
               file_to_process = ConvertGribToGrib2(file_to_process);
               if (file_to_process == null)
               {
                  Console.WriteLine("[FAIL]");
                  result = false;
               }
               else
                  Console.WriteLine("[OK]");
            }

            if (result && !skip_conversion)
            {
               Console.Write("    Converting from GRIB2 to NETCDF...");
               file_to_process = ConvertGrib2ToNETCDF(file_to_process);
               if (file_to_process == null)
               {
                  Console.WriteLine("[FAIL]");
                  result = false;
               }
               else
                  Console.WriteLine("[OK]");
            }

            if (result && !skip_conversion)
            {
               Console.Write("    Converting from NETCDF to HDF5....");
               file_to_process = ConvertNETCDFToHDF5(file_to_process);
               if (file_to_process == null)
               {
                  Console.WriteLine("[FAIL]");
                  result = false;
               }
               else
                  Console.WriteLine("[OK]");
            }

            if (result && process_hdf && !skip_conversion)
            {
               Console.Write("    Processing HDF5 file..............");
               file_to_process = ProcessHDF5(file_to_process);
               if (file_to_process == null)
               {
                  Console.WriteLine("[FAIL]");
                  result = false;
               }
               else
                  Console.WriteLine("[OK]");
            }

            if (result && !skip_conversion)
            {
               Console.Write("    Storing HDF5 file.................");
               file_to_process = StoreHDF5(file_to_process);
               if (file_to_process == null)
               {
                  Console.WriteLine("[FAIL]");
                  result = false;
               }
               else
                  Console.WriteLine("[OK]");
            }

            if (skip_conversion)
            {
               if (process_hdf)
                  file_to_process.FullPath = store_path.Path + file_to_process.Name + output_tag + ".hdf5";
               else
                  file_to_process.FullPath = store_path.Path + file_to_process.Name + ".hdf5";
            }

            if (result && glue_files)
            {
               if (glue_as_mm5)
               {
                  if (!GlueFilesAsMM5(file_to_process))
                     result = false;
               }
               else
               {
                  //if (!GlueFiles(file_to_process))
                  //   result = false;
               }
            }
            else
            {
               if (actual_index >= 2 && actual_index <= 7)
                  first_glue_as_mm5 = false;
               else if (actual_index >= 8 && actual_index <= 13)
                  second_glue_as_mm5 = false;
            }
         }
         catch (Exception ex)
         {
            last_exception = ex;
            result = false;
         }

         Console.Write("    Cleaning working folder...........");
         if (!ClearWorkingFolder())
         {
            Console.WriteLine("[FAIL]");
            result = false;
         }
         else
            Console.WriteLine("[OK]");

         Console.WriteLine("");

         return result;
      }

      protected bool SetupStoreFolder(FileName file_to_process)
      {
         try
         {
            last_store_name = file_to_process.Name.Substring(file_to_process.Name.Length - 21, 10);
            store_path.Path = store_folder.Path + last_store_name;

            if (last_stored_path != store_path.Path)
            {
               actual_index = 1;
               last_stored_path = store_path.Path;
               first_glue_as_mm5 = true;
               second_glue_as_mm5 = true;
            }

            first_glue_as_mm5 = true;
            second_glue_as_mm5 = true;

            if (!System.IO.Directory.Exists(store_path.Path))
               System.IO.Directory.CreateDirectory(store_path.Path);
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return false;
         }
         return true;
      }

      protected FileName ConvertGribToGrib2(FileName file_to_process)
      {
         try
         {
            str.Clear();
            str.Append("-g12 ");
            str.Append(file_to_process.FullName + " ");
            str.Append(file_to_process.Name + ".grb2");

            tool.Arguments = str.ToString();
            tool.WorkingDirectory = working_folder.Path;
            tool.Executable = grib_to_grib2_exe.FullPath;
            tool.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
            tool.Wait = true;

            tool.Run();

            if (!System.IO.File.Exists(working_folder.Path + file_to_process.Name + ".grb2"))
               throw new Exception(grib_to_grib2_exe.Name + " failed to convert " + file_to_process.Name);

            //Check if conversion was successfull
            System.IO.FileInfo fi = new System.IO.FileInfo(working_folder.Path + file_to_process.Name + ".grb2");
            if (fi == null)
               throw new Exception(grib_to_grib2_exe.Name + " failed to convert " + file_to_process.Name);

            if (fi.Length <= 0)
               throw new Exception(grib_to_grib2_exe.Name + " failed to convert " + file_to_process.Name);

            return new FileName(file_to_process.Name + ".grb2");
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return null;
         }
      }

      protected FileName ConvertGrib2ToNETCDF(FileName file_to_process)
      {
         try
         {
            str.Clear();
            str.Append(file_to_process.FullName);
            str.Append(" -netcdf ");
            str.Append(file_to_process.Name + ".nc");

            tool.Arguments = str.ToString();
            tool.WorkingDirectory = working_folder.Path;
            tool.Executable = grib2_to_netcdf_exe.FullPath;
            tool.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
            tool.UseShell = false;
            tool.SaveDefaultOutput = true;
            tool.SaveErrorOutput = true;
            tool.Verbose = false;
            tool.Wait = true;

            tool.Run();

            if (!System.IO.File.Exists(working_folder.Path + file_to_process.Name + ".nc"))
               throw new Exception(grib2_to_netcdf_exe.Name + " failed to convert " + file_to_process.Name);

            return new FileName(file_to_process.Name + ".nc");
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return null;
         }
      }

      protected FileName ConvertNETCDFToHDF5(FileName file_to_process)
      {
         try
         {
            replace_list.Clear();
            replace_list["<<input>>"] = file_to_process.FullName;
            replace_list["<<output>>"] = file_to_process.Name + ".hdf5";

            TextFile.Replace(netcdf_to_hdf5_template.FullPath, working_folder.Path + "ConvertToHDF5Action.dat", ref replace_list);

            tool.Arguments = "";
            tool.WorkingDirectory = working_folder.Path;
            tool.Executable = netcdf_to_hdf5_exe.FullPath;
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "successfully terminated";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            if (!tool.Run())
               throw new Exception(netcdf_to_hdf5_exe.Name + " failed to convert " + file_to_process.Name);

            if (!System.IO.File.Exists(working_folder.Path + file_to_process.Name + ".nc"))
               throw new Exception(netcdf_to_hdf5_exe.Name + " failed to convert " + file_to_process.Name);

            return new FileName(file_to_process.Name + ".hdf5");
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return null;
         }
      }

      protected FileName ProcessHDF5(FileName file_to_process)
      {
         try
         {
            if (!replace_processed_files)
            {
               //Not implemented yet
            }

            replace_list.Clear();
            replace_list["<<input>>"] = file_to_process.FullName;
            replace_list["<<output>>"] = file_to_process.Name + output_tag + ".hdf5";

            string process_template_output = working_folder.Path + hdf_processor_template.Name + ".cfg";
            TextFile.Replace(hdf_processor_template.FullPath, process_template_output, ref replace_list);

            ExternalApp tool = new ExternalApp();

            tool.Arguments = "\\verbose --cfg " + process_template_output;
            tool.WorkingDirectory = working_folder.Path;
            tool.Executable = hdf_processor_exe.FullPath;
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "MohidHDF5Processor SUCCESSFULLY completed the process.";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            if (!tool.Run())
               throw new Exception(hdf_processor_exe.Name + " failed to convert " + file_to_process.Name);

            if (!System.IO.File.Exists(working_folder.Path + file_to_process.Name + output_tag + ".hdf5"))
               throw new Exception(hdf_processor_exe.Name + " failed to convert " + file_to_process.Name);

            return new FileName(file_to_process.Name + output_tag + ".hdf5");
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return null;
         }
      }

      protected FileName StoreHDF5(FileName file_to_process)
      {
         try
         {
            string output;
            if (process_hdf)
               output = store_path.Path + file_to_process.Name.Substring(0, file_to_process.Name.Length - output_tag.Length) + "." + file_to_process.Extension;
            else
               output = store_path.Path + file_to_process.FullName;

            System.IO.File.Copy(working_folder.Path + file_to_process.FullName, output, true);
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return null;
         }

         return new FileName(store_path.Path + file_to_process.FullName);
      }

      protected bool GlueFilesAsMM5(FileName file_to_process)
      {
         bool result = true;
         bool glue_now = true;
         string output;


         try
         {

            //if (last_stored_path != store_path.Path)
            //{
            //   last_stored_path = store_path.Path;
            //   actual_index = 1;
            //}
            if ((actual_index >= 2 && actual_index <= 7 && first_glue_as_mm5) ||
               (actual_index >= 8 && actual_index <= 13 && second_glue_as_mm5))
            {
               string file_name;

               if (process_hdf)
                  file_name = file_to_process.Path + file_to_process.Name.Substring(0, file_to_process.Name.Length - output_tag.Length) + "." + file_to_process.Extension;
               else
                  file_name = file_to_process.FullPath;

               files_to_glue.Add(new FileName(file_name));
            }
            else
               files_to_glue.Clear();

            //actual_index++;

            if (files_to_glue.Count == 6)
            {
               string path;

               if (glue_on_separeted_folders)
                  path = store_glue_folder.Path + last_store_name;
               else
                  path = store_glue_folder.Path;

               //Console.WriteLine("{0}", store_glue_folder.Path);
               //Console.WriteLine("{0}", last_store_name);
               //Console.WriteLine("{0}", files_to_glue[0]);
               //Console.WriteLine("{0}", files_to_glue[0].Substring(files_to_glue[0].Length-10-output_tag.Length, 10));

               //string first = files_to_glue[0].Name.Substring(files_to_glue[0].Name.Length-10-output_tag.Length, 10);
               //string last = files_to_glue[5].Name.Substring(files_to_glue[5].Name.Length-10-output_tag.Length, 10);

               string first = files_to_glue[0].Name.Substring(files_to_glue[0].Name.Length - 10, 10);
               string last = files_to_glue[5].Name.Substring(files_to_glue[5].Name.Length - 10, 10);

               output = path + "\\" + files_to_glue[0].Name.Substring(0, files_to_glue[0].Name.Length - 21) + "_" + first + "_" + last + ".hdf5";

               if (!System.IO.Directory.Exists(path))
                  System.IO.Directory.CreateDirectory(path);

               if (!replace_glued_files)
                  if (System.IO.File.Exists(output))
                     glue_now = false;

               files_temp.Clear();
               foreach (FileName f in files_to_glue)
               {
                  files_temp.Add(f.FullPath);
               }

               HDFGlue glueTool = new HDFGlue(files_temp);

               glueTool.AppName = glue_exe.FullName;
               glueTool.AppPath = glue_exe.Path;
               glueTool.WorkingDirectory = working_folder.Path;
               glueTool.Output = output;
               glueTool.Is3DFile = false;
               glueTool.ThrowExceptionOnError = false;
               glueTool.Verbose = false;

               if (glue_now)
               {
                  Console.WriteLine("    ----------------------------------");
                  Console.Write("    Gluing actual folder files as mm5.");

                  if (glueTool.Glue() != 0)
                     result = false;

                  if (result)
                     Console.WriteLine("[OK]");
                  else
                     Console.WriteLine("[FAIL]");
               }

               files_to_glue.Clear();
               store_files_for_glue = false;

               if (result && glue_all)
               {
                  if (first_glue)
                  {
                     Console.Write("    Gluing final file.................");
                     System.IO.File.Copy(output, store_glue_folder.Path + "glued.hdf5", true);
                     Console.WriteLine("[OK]");
                     first_glue = false;
                  }
                  else
                  {
                     Console.Write("    Gluing final file.................");
                     files_temp.Clear();
                     files_temp.Add(store_glue_folder.Path + "glued.hdf5");
                     files_temp.Add(output);
                     glueTool.FilesToGlue = files_temp;
                     glueTool.Output = working_folder.Path + "new_glued.hdf5";

                     if (glueTool.Glue() != 0)
                        result = false;

                     if (result)
                        Console.WriteLine("[OK]");
                     else
                        Console.WriteLine("[FAIL]");

                     if (result)
                     {
                        Console.Write("    Replacing final file..............");
                        System.IO.File.Copy(working_folder.Path + "new_glued.hdf5", store_glue_folder.Path + "glued.hdf5", true);
                        Console.WriteLine("[OK]");
                     }

                     files_to_glue.Clear();
                  }
               }
               Console.WriteLine("    ----------------------------------");
            }
         }
         catch (Exception ex)
         {
            last_exception = ex;
            files_to_glue.Clear();
            Console.WriteLine("[FAIL]");
            Console.WriteLine("    ----------------------------------");
            result = false;
         }

         return result;

      }

      protected bool GlueFiles(FileName file_to_process)
      {
         bool result = true;
         bool glue_now = true;
         string output;

         try
         {
            if (last_stored_path != store_path.Path)
            {
               last_stored_path = store_path.Path;
               store_files_for_glue = true;
               actual_index = 1;
            }

            if (store_files_for_glue)
            {
               if (actual_index >= starting_hour)
                  files_to_glue.Add(file_to_process);

               actual_index++;

               if (files_to_glue.Count >= hours_to_glue)
               {
                  string path;

                  if (glue_on_separeted_folders)
                     path = store_glue_folder.Path + last_store_name;
                  else
                     path = store_glue_folder.Path;

                  output = path + "\\" + last_store_name + ".hdf5";

                  if (!System.IO.Directory.Exists(path))
                     System.IO.Directory.CreateDirectory(path);

                  if (!replace_glued_files)
                     if (System.IO.File.Exists(output))
                        glue_now = false;

                  files_temp.Clear();
                  foreach (FileName f in files_to_glue)
                  {
                     files_temp.Add(f.FullPath);
                  }
                  HDFGlue glueTool = new HDFGlue(files_temp);

                  glueTool.AppName = glue_exe.FullName;
                  glueTool.AppPath = glue_exe.Path;
                  glueTool.WorkingDirectory = working_folder.Path;
                  glueTool.Output = output;
                  glueTool.Is3DFile = false;
                  glueTool.ThrowExceptionOnError = false;
                  glueTool.Verbose = false;

                  if (glue_now)
                  {
                     Console.WriteLine("    ----------------------------------");
                     Console.Write("    Gluing actual folder files........");

                     if (glueTool.Glue() != 0)
                        result = false;

                     if (result)
                        Console.WriteLine("[OK]");
                     else
                        Console.WriteLine("[FAIL]");
                  }

                  files_to_glue.Clear();
                  store_files_for_glue = false;

                  if (result && glue_all)
                  {
                     if (first_glue)
                     {
                        Console.Write("    Gluing final file.................");
                        System.IO.File.Copy(output, store_glue_folder.Path + "glued.hdf5", true);
                        Console.WriteLine("[OK]");
                        first_glue = false;
                     }
                     else
                     {
                        Console.Write("    Gluing final file.................");
                        files_temp.Add(store_glue_folder.Path + "glued.hdf5");
                        files_temp.Add(output);
                        glueTool.FilesToGlue = files_temp;
                        glueTool.Output = working_folder.Path + "new_glued.hdf5";

                        if (glueTool.Glue() != 0)
                           result = false;

                        if (result)
                           Console.WriteLine("[OK]");
                        else
                           Console.WriteLine("[FAIL]");

                        if (result)
                        {
                           Console.Write("    Replacing final file..............");
                           System.IO.File.Copy(working_folder.Path + "new_glued.hdf5", store_glue_folder.Path + "glued.hdf5", true);
                           Console.WriteLine("[OK]");
                        }

                        files_to_glue.Clear();
                     }
                  }
                  Console.WriteLine("    ----------------------------------");
               }
            }
         }
         catch (Exception ex)
         {
            last_exception = ex;
            files_to_glue.Clear();
            Console.WriteLine("[FAIL]");
            Console.WriteLine("    ----------------------------------");
            result = false;
         }

         return result;
      }

      protected bool ClearWorkingFolder()
      {
         if (!clean_working_folder)
            return true;

         System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(working_folder.Path);
         foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
            fileToDelete.Delete();

         return true;
      }

      Exception IMohidTask.LastException
      {
         get { return last_exception; }
      }

      bool IMohidTask.Reset()
      {
         return true;
      }
   }
}
