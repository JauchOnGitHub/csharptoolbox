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
         int task_index = 1;

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
                  task_i = LoadScript(task_script);
               }
               else
                  task_i = new CPTEC2HDF_48h_v2.CPTEC2HDFv2();

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
            Assembly ass = sc.Compile(script_file_path);
            si = new ScriptInfo();
            si.ScriptFile = script_file_path;
            si.Interface = (IMohidTask)sc.FindScriptInterface("IMohidTask", ass);
            return si.Interface;
         }
      }
   }
}
