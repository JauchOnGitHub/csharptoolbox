using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.WebMail;

namespace Tests
{
   class Program
   {
      static void Main(string[] args)
      {
         MailSender ms = new MailSender();

         ms.SetFrom("mohid.operational@gmail.com", "MohidRun");
         ms.User = "mohid.operational@gmail.com";
         ms.Password ="MohidOperationalISTMARETEC2011";
         ms.SetMessage("teste", "MohidRun Tests");
         ms.Host = "smtp.gmail.com";
         ms.Port = 587;
         ms.AddTo("eduardo.jauch@ist.utl.pt", "Eduardo Jauch");
         ms.EnableSSL = true;
         ms.SendMail();

         Console.ReadKey(false);
      }
   }
}
