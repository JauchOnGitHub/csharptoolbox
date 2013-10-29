using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Software;
using Mohid.Files;
using Mohid.CommandArguments;
using Mohid.Configuration;

namespace InterpolateRain
{
   class InterpolateRain
   {
      static void Main(string[] args)
      {
         CmdArgs cmdArgs = new CmdArgs(args);
         Dictionary<String, String> replace_list = new Dictionary<string, string>();
         DateTime end, actual_start, actual_end;

         if (!cmdArgs.HasParameter("cfg"))
         {
            Console.WriteLine("Configuration file was not provided. Use --cfg [file] to provide one.");
            Console.WriteLine("Operation Aborted.");
            return;
         }

         Config cfg = new Config(cmdArgs.Parameter("cfg"));
         if (!cfg.Load())
         {
            Console.WriteLine("Was not possible to load '" + cmdArgs.Parameter("cfg") + "'.");
            if (!string.IsNullOrWhiteSpace(cfg.ExceptionMessage))
            {
               Console.WriteLine("The message returned was:");
               Console.WriteLine(cfg.ExceptionMessage);
               Console.WriteLine("Operation Aborted.");
               return;
            }
         }

         ConfigNode root = cfg.Root;

         int id = root["start.id", 1].AsInt();
         string template = root["template", "FillMatrix.template"].AsString();
         string dateFormat = root["dateFormat.format", "yyyy-MM-dd HH:mm:ss"].AsString();
         actual_start = root["start"].AsDateTime(dateFormat);         
         end = root["end"].AsDateTime(dateFormat);
         int interval;
         if (root.NodeData.ContainsKey("interval.days"))
         {
            interval = root["interval.days"].AsInt();
            actual_end = actual_start.AddDays(interval);
         }
         else
         {
            interval = 10;
            actual_end = end;
         }
         string interpolation = root["interpolation.method", "2"].AsString();

         replace_list.Add("<<start>>", actual_start.ToString("yyyy M d H m s"));
         replace_list.Add("<<end>>", actual_end.ToString("yyyy M d H m s"));
         replace_list.Add("<<id>>", id.ToString());
         replace_list.Add("<<interpolation>>", interpolation);

         ExternalApp fillmatrix = new ExternalApp();

         while (actual_end <= end)
         {
            
            TextFile.Replace(template, "FillMatrix.dat", ref replace_list);

            fillmatrix.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
            fillmatrix.Wait = true;
            fillmatrix.WorkingDirectory = @".\";
            fillmatrix.Executable = @".\fillmatrix.exe";

            try
            {
               fillmatrix.Run();
            }
            catch (Exception ex)
            {
               Console.WriteLine("Erro detectado. Mensagem de erro:");
               Console.WriteLine(ex.Message);
               Console.WriteLine("ID    : {0}", id.ToString());
               Console.WriteLine("Start : {0}", actual_start.ToString());
               Console.WriteLine("End   : {0}", actual_end.ToString());
               Console.WriteLine("");
            }

            id++;
            actual_start = actual_end;
            actual_end = actual_start.AddDays(14);

            if (actual_start < end && actual_end > end)
               actual_end = end;

            replace_list["<<start>>"] = actual_start.ToString("yyyy M d H m s");
            replace_list["<<end>>"] = actual_end.ToString("yyyy M d H m s");
            replace_list["<<id>>"] = id.ToString();
         }
      }
   }
}
