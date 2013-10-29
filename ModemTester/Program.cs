using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Core;
using Mohid.SMS;
using Mohid.CommandArguments;

namespace ModemTester
{
   class Program
   {
      static void Main(string[] args)
      {
         try
         {
            SMSEngine sms = new SMSEngine();
            CmdArgs cmdArgs = new CmdArgs(args);
            string number, message;

            //PORT NUMBER
            if (cmdArgs.HasParameter("port"))
            {
               sms.Port.PortName = "COM" + cmdArgs.Parameters["port"];
            }
            else
               throw new Exception("'port' parameter is missing.");

            //BAUDRATE
            if (cmdArgs.HasParameter("baudrate"))
            {
               sms.Baudrate = int.Parse(cmdArgs.Parameters["baudrate"]);
            }
            else
               throw new Exception("'baudrate' parameter is missing.");

            //SMS CENTER
            if (cmdArgs.HasParameter("sms.center"))
            {
               sms.SMSCenter = cmdArgs.Parameters["sms.center"];
            }
            else
               throw new Exception("'sms.center' parameter is missing.");

            //PIN CODE
            if (cmdArgs.HasParameter("pin"))
            {
               sms.PinCode = cmdArgs.Parameters["pin"];
            }
            else
               throw new Exception("'pin' parameter is missing.");

            //TIMEOUT
            if (cmdArgs.HasParameter("time.out"))
            {
               sms.TimeOut = int.Parse(cmdArgs.Parameters["time.out"]);
            }

            //WAIT TIME
            if (cmdArgs.HasParameter("wait.time"))
            {
               sms.WaitTime = int.Parse(cmdArgs.Parameters["wait.time"]);
            }

            //INTERVAL BETWEEN MESSAGES
            if (cmdArgs.HasParameter("interval.time"))
            {
               sms.IntervalBetweenMessages = int.Parse(cmdArgs.Parameters["interval.time"]);
            }

            //NUMBER
            if (cmdArgs.HasParameter("number") && cmdArgs.HasParameter("message"))
            {
               number = cmdArgs.Parameters["number"];
               message = cmdArgs.Parameters["message"];
               sms.SendSMS(number, message);
            }
            else
               throw new Exception("No NUMBER or MESSAGE parameter found");
         }
         catch (Exception ex)
         {
            Console.WriteLine("An EXCEPTION was raised. The message returned was:");
            Console.WriteLine(ex.Message);
         }
      }
   }
}
