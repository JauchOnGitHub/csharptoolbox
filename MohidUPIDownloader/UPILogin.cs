using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace MohidUPIDownloader
{
   public struct UPILoginData
   {
      public string Server,
                    User,
                    Pass,
                    Version,
                    Mode;
      public int Timeout;
      
   }

   public class UPILogin
   {
      public UPILoginData Data;
      protected string session_string;
      protected WebClient wc;

      public UPILogin()
      {
         Data.Version = "1.2";
         Data.Timeout = 3600;
         Data.Mode = "t";         
      }

      public string Login()
      {
         try
         {
            wc = new WebClient();

            string address = string.Format(Data.Server + "addUPI?function=login&user={0}&passwd={1}&timeout={2}&mode={3}&version={4}",
                                           Uri.EscapeDataString(Data.User),
                                           Uri.EscapeDataString(Data.Pass),
                                           Uri.EscapeDataString(Data.Timeout.ToString()),
                                           Uri.EscapeDataString(Data.Mode),
                                           Uri.EscapeDataString(Data.Version));

            string result = wc.DownloadString(address);

            XPathDocument doc = new XPathDocument(new StringReader(result));
            XPathNavigator nav = doc.CreateNavigator();

            nav.MoveToRoot();
            nav.MoveToFirstChild();

            if (nav.LocalName != "response") throw new Exception("Expecting 'response' but found '" + nav.LocalName);

            nav.MoveToFirstChild();
            if (nav.LocalName != "result") throw new Exception("Expecting 'result' but found '" + nav.LocalName);

            nav.MoveToFirstChild();
            if (nav.LocalName != "string") throw new Exception("Expecting 'string' but found '" + nav.LocalName);

            session_string = nav.Value;

            return session_string;
         }
         catch (Exception ex)
         {
            throw new Exception("UPILogin.Login() failed.", ex);
         }
      }

      public void Logout()
      {
         try
         {
            wc = new WebClient();

            string address = string.Format(Data.Server + "addUPI?function=logout&session-id={0}&mode={1}",
                                           Uri.EscapeDataString(session_string),
                                           Uri.EscapeDataString(Data.Mode));

            string result = wc.DownloadString(address);

            XPathDocument doc = new XPathDocument(new StringReader(result));
            XPathNavigator nav = doc.CreateNavigator();

            nav.MoveToRoot();
            nav.MoveToFirstChild();

            if (nav.LocalName != "response") throw new Exception("Expecting 'response' but found '" + nav.LocalName);

            nav.MoveToFirstChild();
            if (nav.LocalName != "result") throw new Exception("Expecting 'result' but found '" + nav.LocalName);

            if (!nav.IsEmptyElement) throw new Exception("Expecting an empty 'result' element but found '" + nav.Value);
         }
         catch (Exception ex)
         {
            throw new Exception("UPILogin.Logout() failed.", ex);
         }
      }
   }
}
