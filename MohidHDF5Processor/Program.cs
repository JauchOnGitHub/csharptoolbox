using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid;
using Mohid.Core;
using Mohid.WebMail;
using Mohid.Configuration;
using Mohid.Files;
using Mohid.CommandArguments;
using HDF5DotNet;

namespace MohidHDF5Processor
{
   class Program
   {
      static int Main(string[] args)
      {
         CmdArgs cmdArgs = null;
         Exception last_exception = null;
         string task_block;


         try
         {
            Setup.StandartSetup();
            cmdArgs = new CmdArgs(args);

            //======================================================================================
            //Load configuration
            //======================================================================================
            Config cfg = new Config(cmdArgs.Parameter("cfg"));
            if (!cfg.Load())
            {
               Console.WriteLine("[{0}] Was not possible to load the configuration file '{1}'", DateTime.Now, cmdArgs.Parameter("cfg"));
               return -1;
            }

            //======================================================================================
            //Check to see if there are a specific name for the task block
            //======================================================================================
            if (cmdArgs.HasParameter("task"))
               task_block = cmdArgs.Parameter("task");
            else
               task_block = "task.config";

            //======================================================================================
            //Execute task
            //======================================================================================
            ConfigNode task_cfg = cfg.Root.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == task_block; });
            if (task_cfg != null)
            {
               TaskEngine te = new TaskEngine();
               if (!te.LoadConfig(task_cfg))
               {
                  if ((last_exception = te.LastException) == null)
                     last_exception = new Exception("Unknow error during process of task configuration.");
               }
               if (!te.CreateNewHDF())
               {
                  if ((last_exception = te.LastException) == null)
                     last_exception = new Exception("Unknow error during process of task execution.");
               }
               te.End();
            }
            else
            {
               last_exception = new Exception("No task.config block found in configuration.");
            }

            if (cmdArgs.HasOption("verbose"))
            {
               if (last_exception != null)
                  Console.WriteLine("MohidHDF5Processor FAILED to complete the process.");
               else
                  Console.WriteLine("MohidHDF5Processor SUCCESSFULLY completed the process.");
            }

            //======================================================================================
            //Send STATUS e-mail if mail.config block exists
            //======================================================================================
            ConfigNode mail_cfg = cfg.Root.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "mail.config"; });
            if (mail_cfg != null)
            {
               MailEngine mail_engine = new MailEngine();

               if (!mail_engine.SendMail(mail_cfg, last_exception))
               {
                  Console.WriteLine("[{0}] Was not possible to send the status e-mail.", DateTime.Now);
                  if ((last_exception = mail_engine.LastException) != null)
                     Console.WriteLine("The message returned was: {0}", last_exception);
                  return -1;
               }
            }
            else if (last_exception != null)
            {
               throw last_exception;
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine("[{0}] An unexpected exception happened. The message returned was: {1}", DateTime.Now, ex.Message);
            return -1;
         }

         return 0;

      //   HDFEngine engine = new HDFEngine();
      //   Exception last_ex = null;

      //   Console.WriteLine("Starting...");

      //   if (!engine.InitializeLibrary())
      //   {
      //      Console.WriteLine("Library start failed.");
      //      if ((last_ex = engine.LastException) != null)
      //         Console.WriteLine("Message: {0}", last_ex.Message);
      //   }

      //   if (!engine.OpenHDF(new FileName(@"E:\Development\Tests\HDF5\basin.evtp.hdf5")))
      //   {
      //      Console.WriteLine("File Open failed.");
      //      if ((last_ex = engine.LastException) != null)
      //         Console.WriteLine("Message: {0}", last_ex.Message);
      //   }

      //   Console.WriteLine("ROOT: '/'");
      //   List<HDFObjectInfo> list = engine.GetTree(null, engine.FileID, "/");
      //   PrintList(list, 0);

      //   Console.WriteLine("");

      //   Console.WriteLine("ROOT: '/Grid/'");
      //   list.Clear();
      //   list = engine.GetTree(null, engine.FileID, "/Grid/");
      //   PrintList(list, 1);

      //   if (!engine.CloseHDF())
      //   {
      //      Console.WriteLine("File Close failed.");
      //      if ((last_ex = engine.LastException) != null)
      //         Console.WriteLine("Message: {0}", last_ex.Message);
      //   }

      //   if (!engine.CloseLibrary())
      //   {
      //      Console.WriteLine("Library close failed.");
      //      if ((last_ex = engine.LastException) != null)
      //         Console.WriteLine("Message: {0}", last_ex.Message);
      //   }

      //   Console.WriteLine("End. Press any key.");
      //   Console.ReadKey();
      //}

      //public static void PrintList(List<HDFObjectInfo> list, int level)
      //{
      //   foreach (HDFObjectInfo oi in list)
      //   {
      //      Console.WriteLine("{0}: {1}", level, oi.Name);
      //      if (oi.Children != null)
      //         PrintList(oi.Children, level + 1);
      //   }
      }
   }
}
