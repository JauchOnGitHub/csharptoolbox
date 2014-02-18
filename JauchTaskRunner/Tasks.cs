using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

using Mohid;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.Files;

namespace Mohid
{
   public class ScriptInfo
   {
      public FileName ScriptFile;
      public IMohidTask Interface;

      public ScriptInfo()
      {
         ScriptFile = null;
         Interface = null;
      }
   }

   public class Tasks
   {
      List<ConfigNode> task_list;
      Exception last_exception;
      int successfull_tasks = 0;
      List<ScriptInfo> scripts;

      public int NumberOfTasks
      {
         get
         {
            return task_list.Count;
         }
      }

      public Exception LastException
      {
         get
         {
            return last_exception;
         }
      }

      public int FailedTasksCount
      {
         get
         {
            return task_list.Count - successfull_tasks;
         }
      }

      public int SuccessfullTasks
      {
         get
         {
            return successfull_tasks;
         }
      }

      public Tasks(List<ConfigNode> task_list)
      {
         this.task_list = task_list;
         last_exception = null;
         scripts = new List<ScriptInfo>();
      }

      public bool RunTasks()
      {         
         int task_index = 0;

         foreach (ConfigNode task in task_list)
         {
            bool ignore_exception = false;
            bool task_completed = false;
            try
            {
               task_index++;
               ignore_exception = task["ignore.exception", false].AsBool();
               IMohidTask task_i;

               if (task.Contains("script"))
               {
                  FileName task_script = task["script"].AsFileName();
                  try
                  {
                     task_i = LoadScript(task_script);
                  }
                  catch (Exception ex)
                  {
                     throw new Exception("LoadScript failed with the message: " + ex.Message);
                  }                  
               }
               else if (task.Contains("library"))
               {
                  FileName task_library = task["library"].AsFileName();
                  string task_class = task["class"].AsString();
                  try
                  {
                     task_i = LoadLibrary(task_library, task_class);
                  }
                  catch (Exception ex)
                  {
                     throw new Exception("LoadScript failed with the message: " + ex.Message);
                  }
               }
               else
               {
                  task_i = null;
                  throw new Exception("No script file provided.");
               }

               if (task_i == null)
                  throw new Exception("Invalid Script");
               if (!task_i.Run(task))
               {
                  last_exception = task_i.LastException;
               }
               else
               {
                  task_completed = true;
                  successfull_tasks++;
               }
            }
            catch (Exception ex)
            {
               last_exception = ex;
            }

            if (!task_completed)
            {
               Console.WriteLine("WARNING: Task " + task_index + " failed. The message returned was:");
               Console.WriteLine(last_exception.Message);

               if (!ignore_exception)
                  break;
            }
         }

         if (successfull_tasks < task_list.Count)
            return false;

         return true;
      }

      protected IMohidTask LoadLibrary(FileName library_file_path, string task_class)
      {
         ScriptInfo si = scripts.Find(delegate(ScriptInfo info) { return info.ScriptFile.FullPath == library_file_path.FullPath; });
         if (si != null)
         {
            si.Interface.Reset();
            return si.Interface;
         }
         else
         {            
            Assembly ass;
            try
            {
               ass = Assembly.LoadFrom(library_file_path.FullPath);
            }
            catch (Exception ex)
            {
               throw new Exception("Error when trying to compile " + library_file_path.FullPath + ". The message returned was " + ex.Message);
            }

            si = new ScriptInfo();
            si.ScriptFile = library_file_path;

            Type t = ass.GetType(task_class);
            if (t.GetInterface("IMohidTask", true) != null)
               si.Interface = (IMohidTask)ass.CreateInstance(t.FullName);

            scripts.Add(si);

            return si.Interface;
         }
      }

      protected IMohidTask LoadScript(FileName script_file_path)
      {
         ScriptInfo si = scripts.Find(delegate(ScriptInfo info) { return info.ScriptFile.FullPath == script_file_path.FullPath;  });
         if (si != null)
         {
            si.Interface.Reset();
            return si.Interface;
         }
         else
         {
            ScriptCompiler sc = new ScriptCompiler();
            Assembly ass;
            try
            {
               ass = sc.Compile(script_file_path);
            }
            catch (Exception ex)
            {               
               throw new Exception("Error when trying to compile " + script_file_path.FullPath + ". The message returned was " + ex.Message);
            }
            
            si = new ScriptInfo();
            si.ScriptFile = script_file_path;
            si.Interface = (IMohidTask)sc.FindScriptInterface("IMohidTask", ass);

            scripts.Add(si);

            return si.Interface;
         }
      }
   }
}
