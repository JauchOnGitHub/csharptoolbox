using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace Mohid
{
   namespace WebMail
   {
      public class MailSender
      {
        protected MailMessage message;

        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public bool EnableSSL { get; set; }
        public string Password { get; set; }
        public int Timeout { get; set; }

        public MailSender()
        {
            message = new MailMessage();
            EnableSSL = false;
            Host = "";
            User = "";
            Port = -1;
            Password = "";
            Timeout = 200000;
            message.BodyEncoding = System.Text.Encoding.Default;
        }

        public void Reset()
        {
            message.To.Clear();
            message.CC.Clear();
            message.Bcc.Clear();
            message.Attachments.Clear();
            message.Body = "";
        }

        public void ResetTo()
        {
            message.To.Clear();
        }

        public void ResetCC()
        {
            message.CC.Clear();
        }

        public void ResetBcc()
        {
            message.Bcc.Clear();
        }

        public void ResetAttachments()
        {
            message.Attachments.Clear();
        }

        public void AddTo(string address, string display_name = "")
        {
            if (string.IsNullOrWhiteSpace(display_name)) 
               display_name = address;
            MailAddress new_mail = new MailAddress(address, display_name);
            message.To.Add(new_mail);
        }

        public void AddToList(string addresses)
        {
           message.To.Add(addresses);
        }

        public void AddCCList(string addresses)
        {
           message.CC.Add(addresses);
        }

        public void AddBccList(string addresses)
        {
           message.Bcc.Add(addresses);
        }

        public void AddCC(string address, string display_name = "")
        {
           if (string.IsNullOrWhiteSpace(display_name)) 
              display_name = address;
           MailAddress new_mail = new MailAddress(address, display_name);
           message.CC.Add(new_mail);
        }

        public void AddBCC(string address, string display_name = "")
        {
           if (string.IsNullOrWhiteSpace(display_name))
              display_name = address;
           MailAddress new_mail = new MailAddress(address, display_name);
           message.Bcc.Add(new_mail);
        }

        public void AddAttachment(string path)
        {
           Attachment new_attachment = new Attachment(path);
           message.Attachments.Add(new_attachment);
        }

        public void SetFrom(string address, string display_name = "")
        {
           if (string.IsNullOrWhiteSpace(display_name)) 
              display_name = address;
           MailAddress from = new MailAddress(address, display_name);
           message.From = from;
        }

        public void SetMessage(string text, string subject = "")
        {
           message.Body = text;
           message.Subject = subject;
        }

        public void SendMail()
        {
           SmtpClient smtp = new SmtpClient(Host, Port);

           smtp.Host = Host;
           smtp.Port = Port;
           smtp.Credentials = new NetworkCredential(User, Password);
           smtp.EnableSsl = EnableSSL;
           smtp.Timeout = Timeout;
           smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


           smtp.Send(message);
        }
      }
   }
}
