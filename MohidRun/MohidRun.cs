using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid;
using Mohid.Core;
using Mohid.CommandArguments;
using Mohid.WebMail;
using Mohid.Simulation;
using Mohid.Configuration;

namespace MohidRun
{
   class MohidRun
   {
      static void Main(string[] args)
      {
         CmdArgs cmdArgs = null;
         Exception e = null;
         bool sendMail = false;
         bool sendSMS = false;
         bool verbose = false;
         IMohidSim test = null;

         try
         {
            Setup.StandartSetup();
            cmdArgs = new CmdArgs(args);

            if (cmdArgs.HasParameter("mailcfg"))
               sendMail = true;

            if (cmdArgs.HasParameter("smscfg"))
               sendSMS = true;

            if (cmdArgs.HasOption("verbose"))
               verbose = true;

            if (cmdArgs.HasOption("test"))
               test = (IMohidSim)new Test();
         }
         catch(Exception ex)
         {
            Console.WriteLine("Exception raised during initialization: {0}", ex.Message);
            return;
         }

         try
         {
            MohidRunEngine mre = new MohidRunEngine();
            mre.Verbose = verbose;
            mre.Run(cmdArgs, test);               
         }
         catch (Exception ex)
         {
            e = ex;
         }

         if (sendMail)
         {    
            try
            {
               Config cfg = new Config(cmdArgs.Parameter("mailcfg"));
               if (!cfg.Load())
               {
                  Console.WriteLine("[{0}] Was not possible to load the mail configuration file '{1}'", DateTime.Now, cmdArgs.Parameter("mailcfg"));            
               }
               else
               {
                  MailSender ms = new MailSender();
                  
                  string sendTo,
                         header,
                         message;

                  if (e != null)
                  {
                     header = "[ERROR] " + cfg.Root["header", "MohidRun Report"].AsString();
                     sendTo = "sendto.onerror";
                     message = cfg.Root["message", "Mohid Run Report"].AsString() + Environment.NewLine;
                     message += "An exception happened" + e.Message + Environment.NewLine;

                     while (e != null)
                     {
                        message += "  => " + e.Message + Environment.NewLine;
                        e = e.InnerException;
                     }
                  }
                  else
                  {
                     header = "[SUCCESS] " + cfg.Root["header", "MohidRun Report"].AsString();
                     sendTo = "sendto.onsuccess";
                     message = cfg.Root["message", "Mohid Run Report"].AsString();
                  }

                  ms.SetFrom(cfg.Root["from"].AsString(), cfg.Root["display", cfg.Root["from"].AsString()].AsString());
                  ms.User = cfg.Root["user", "mohid.operational@gmail.com"].AsString();
                  ms.Password = cfg.Root["pass", "MohidOperationalISTMARETEC2011"].AsString();
                  ms.SetMessage(message, header);
                  ms.Host = cfg.Root["host", "smtp.gmail.com"].AsString();
                  ms.Port = cfg.Root["port", 587].AsInt();
                  ms.EnableSSL = cfg.Root["enable.ssl", true].AsBool();

                  foreach (ConfigNode n in cfg.Root.ChildNodes.FindAll(delegate(ConfigNode node) { return (node.Name == sendTo || node.Name == "sendto"); }))
                  {
                     if (!(n["bcc", ""].AsString() == ""))
                        ms.AddBCC(n["bcc"].AsString(), n["display", n["bcc"].AsString()].AsString());
                     else if (!(n["cc", ""].AsString() == ""))
                        ms.AddCC(n["cc"].AsString(), n["display", n["cc"].AsString()].AsString());
                     else
                        ms.AddTo(n["to"].AsString(), n["display", n["to"].AsString()].AsString());
                  }

                  
                  ms.SendMail();
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine("[{0}] Was not possible to send the mail. An EXCEPTION happened. The message returned was:", DateTime.Now);
               Console.WriteLine("{0}", ex.Message);
            }
         }

         if (sendSMS)
         {
         }

         if (test != null)
         {
            Console.WriteLine("Press any key...");
            Console.ReadKey();
         }
      }
   }
}
