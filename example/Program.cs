using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Eventing;

namespace example
{
   public class Program
   {
      static void Main(string[] args)
      {

         Searcher searcher = new Searcher();
         Executer executer = new Executer();

      }      
   }

   public class ConfigFile
   {
      protected string file_path;
      protected string exe_path;

      public ConfigFile()
      {
      }

      public string FilePath
      {
         get
         {
            return file_path;
         }

         set
         {
            file_path = value;
         }
      }

      public bool Load()
      {
      }
   }

   public class Searcher
   {
      protected string file_path;

      public Searcher()
      {
      }

      public string FilePath 
      { 
         get 
         { 
            return file_path; 
         } 
         
         set 
         { 
            file_path = value; 
         } 
      }

      public bool FileExist()
      {
         return File.Exists(file_path);
      }
   }

   public class Executer
   { 
      protected string exe_path;
      protected string working_path;

      public string ExePath
      {
         get
         {
            return exe_path;
         }
         set
         {
            exe_path = value;
         }
      }

      public string WorkingPath
      {
         get
         {
            return working_path;
         }
         set
         {
            working_path = value;
         }
      }

      public Executer()
      { 
      }

      public void Execute()
      {
         Process objProcess = new Process();

         objProcess.StartInfo.UseShellExecute = false;
         objProcess.StartInfo.FileName = exe_path;
         objProcess.StartInfo.WorkingDirectory = working_path;

         objProcess.WaitForExit();
      }
   }
}
