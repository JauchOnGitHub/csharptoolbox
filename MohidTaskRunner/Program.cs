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
using Mohid.Script;

namespace MohidHDF5Processor
{
   class Program
   {
      static void Main(string[] args)
      {
         CmdArgs cmdArgs = null;
         Exception last_exception = null;

         try
         {
            Setup.StandardSetup();
            cmdArgs = new CmdArgs(args);

            //======================================================================================
            //Load configuration
            //======================================================================================
            Config cfg = new Config(cmdArgs.Parameter("cfg"));
            if (!cfg.Load())
            {
               Console.WriteLine("[{0}] Was not possible to load the configuration file '{1}'", DateTime.Now, cmdArgs.Parameter("cfg"));
               return;
            }

            //======================================================================================
            //Execute tasks
            //======================================================================================
            List<ConfigNode> task_list = cfg.Root.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "task.config"; });

            if (task_list != null && task_list.Count > 0)
            {
               Tasks t_engine = new Tasks(task_list);
               if (!t_engine.RunTasks())
               {
                  if (t_engine.SuccessfullTasks == 0)
                     Console.WriteLine("All tasks failed");
                  else if(t_engine.SuccessfullTasks == 1)
                     Console.WriteLine("Only 1 task from a total of " + t_engine.NumberOfTasks + " were successfull");
                  else
                     Console.WriteLine("Only " + t_engine.SuccessfullTasks + " tasks from a total of " + t_engine.NumberOfTasks + " were successfull");

                  last_exception = t_engine.LastException;
               }
            }
            else
            {
               last_exception = new Exception("No task.config block found in configuration.");
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
                  return;
               }
            }
            else if (last_exception != null)
            {
               throw last_exception;
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("[{0}] An unexpected exception happened. The message returned was: {1}", DateTime.Now, ex.Message);
            return;
         }
      }


   }
}