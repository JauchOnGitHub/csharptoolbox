using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Mohid
{
   namespace FTP
   {
      public class FTPTools
      {
         private string user, pass;
         private Ftp


         public string Server;
         public string User { set { user = value; } }
         public string Pass { set { pass = value; } }

         public bool Open()
         {
         }

         public bool Login()
         {
         }

         public void Upload(string file)
         {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Server);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(user, pass);

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(file);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
         }
      }
   }
}
