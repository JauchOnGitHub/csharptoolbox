using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid;
using Mohid.Configuration;
using Mohid.WebMail;

namespace MohidHDF5Processor
{
   public class MailEngine
   {
      protected Exception last_exception;

      public Exception LastException
      {
         get
         {
            Exception to_return = last_exception;
            last_exception = null;
            return to_return;
         }
      }

      public MailEngine()
      {
         last_exception = null;
      }

      public bool SendMail(ConfigNode cfg, Exception e = null)
      {
         try
         {
            MailSender ms = new MailSender();

            string sendTo,
                   header,
                   message;

            if (e != null)
            {
               header = "[ERROR] " + cfg["header", "MohidRun Report"].AsString();
               sendTo = "sendto.onerror";
               message = cfg["message", "Mohid Run Report"].AsString() + Environment.NewLine;
               message += "Exception raised: " + Environment.NewLine;
               message += e.Message;
            }
            else
            {
               header = "[SUCCESS] " + cfg["header", "MohidRun Report"].AsString();
               sendTo = "sendto.onsuccess";
               message = cfg["message", "Mohid Run Report"].AsString();
            }

            ms.SetFrom(cfg["from"].AsString(), cfg["display", cfg["from"].AsString()].AsString());
            ms.User = cfg["user", "mohid.operational@gmail.com"].AsString();
            ms.Password = cfg["pass", "MohidOperationalISTMARETEC2011"].AsString();
            ms.SetMessage(message, header);
            ms.Host = cfg["host", "smtp.gmail.com"].AsString();
            ms.Port = cfg["port", 587].AsInt();
            ms.EnableSSL = cfg["enable.ssl", true].AsBool();

            foreach (ConfigNode n in cfg.ChildNodes.FindAll(delegate(ConfigNode node) { return (node.Name == sendTo || node.Name == "sendto"); }))
            {
               if (!(n["bcc", ""].AsString() == ""))
                  ms.AddBCC(n["bcc"].AsString(), n["display", n["bcc"].AsString()].AsString());
               else if (!(n["cc", ""].AsString() == ""))
                  ms.AddCC(n["cc"].AsString(), n["display", n["cc"].AsString()].AsString());
               else
                  ms.AddTo(n["to"].AsString(), n["display", n["to"].AsString()].AsString());
            }

            ms.SendMail();
            last_exception = null;

            return true;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return false;
         }
      }
   }
}
