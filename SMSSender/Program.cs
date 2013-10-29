using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Mohid;
using Mohid.CommandArguments;
using Mohid.SMS;
using Mohid.Databases;
using Mohid.Log;
using Mohid.Files;

namespace SMSSender
{
   class Program
   {
      static int Main(string[] args)
      {
         TextFile log = null;
         CmdArgs cmd_args = null;
         Engine eng = null;
         int result = 0;
         bool debug = false;

         try
         {
            cmd_args = new CmdArgs(args);
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error when trying to parse the command line arguments.");
            Console.WriteLine("The message returned was:");
            Console.WriteLine(ex.Message);
            result = -1;
         }

         if (result == 0) 
            try
            {
               string log_file;
               if (cmd_args.HasParameter("log"))
               {
                  log_file = cmd_args.Parameters["log"];
               }               
               else
               {
                  if (cmd_args.HasParameter("logpath"))
                     log_file = (new FilePath(cmd_args.Parameters["logpath"])).Path + "log_";
                  else
                     log_file = "log_";

                  DateTime DateNow = DateTime.Now;

                  log_file += DateNow.Year.ToString("D4") + DateNow.Month.ToString("D2") + DateNow.Day.ToString("D2") + "-" +
                              DateNow.Hour.ToString("D2") + DateNow.Minute.ToString("D2") + DateNow.Second.ToString("D2") + ".dat";

                  //Console.WriteLine("WARNING: Missing 'log' parameter.");
                  //Console.WriteLine("A log file will be created with the name:");
                  //Console.WriteLine(log_file);
               }

               log = new TextFile(log_file);            
            }
            catch (Exception ex)
            {
               Console.WriteLine("Error when trying to create log file.");
               Console.WriteLine("The message returned was:");
               Console.WriteLine(ex.Message);
               result = -2;
            }

         bool verbose = false;
         if (cmd_args.HasOption("verbose")) verbose = true;

         if (cmd_args.HasOption("debug")) debug = true;

         if (result == 0)
            try 
	         {            
               DateTime d_and_t = DateTime.Now;

               if (!cmd_args.HasParameter("cfg"))
                  throw new Exception("No configuration file was provided.");
               if (cmd_args.HasParameter("date"))
                  d_and_t = DateTime.ParseExact(cmd_args.Parameters["date"], "yyyy mm dd", CultureInfo.InvariantCulture);

               if (verbose) Console.WriteLine("Starting sms engine.");
               eng = new Engine();
               eng.Debug = debug;

               if (verbose) Console.WriteLine("Loading configuration.");
               if (!eng.LoadConfig(new Mohid.Files.FileName(cmd_args.Parameters["cfg"])))
                  throw new Exception("Error when loading the configuration file.");

               if (verbose) Console.WriteLine("Sending messages.");
               if (!eng.SendMessages(d_and_t))
                  throw new Exception("Error when trying to send the messages.");		
	         }
	         catch (Exception ex)
	         {
               Console.WriteLine("Error when trying to send messages.");
               Console.WriteLine("The exception returned was:");
               Console.WriteLine(ex.Message);
               result = -3;
	         }

         if (log != null)
         {
            if (eng.HasErrors)
            {
               log.OpenNewToWrite();
               log.Write(eng.Errors);
               log.Close();
            }
         }

         return result;
      }
   }
}
