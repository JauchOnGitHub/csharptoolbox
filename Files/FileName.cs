using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mohid
{
   namespace Files
   {
      public class FileName
      {
         protected string path,
                          name,
                          ext;

         public FileName()
         {
            path = "";
            name = "";
            ext = "";
         }

         public FileName(string fullPath)
         {
            FullPath = fullPath;
         }

         public string Extension
         {
            get { return ext; }
            set { ext = value; }
         }
         public string Name
         {
            get { return name; }
            set 
            {               
               name = value; 
            }
         }
         public string Path
         {
            get { return path; }
            set
            {
               if (string.IsNullOrWhiteSpace(value))
                  path = "";
               if (!value.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                  path = value + System.IO.Path.DirectorySeparatorChar.ToString();
               else
                  path = value;
            }
         }
         public string FullName
         {
            get
            {
               if (string.IsNullOrWhiteSpace(ext))
               {
                  return name;
               }
               else
                  return name + "." + ext;
            }
            set
            {
               if (string.IsNullOrWhiteSpace(value))
               {
                  name = "";
                  ext = "";
                  return;
               }

               int ne = value.LastIndexOf('.');
               if (ne < 0)
               {
                  name = value;
                  ext = "";
               }
               else
               {
                  name = value.Substring(0, ne);
                  ext = value.Substring(ne + 1);
               }
            }
         }
         
         public string FullPath
         {
            get 
            {
               string extT;
               if (string.IsNullOrWhiteSpace(ext))
                  extT = "";
               else
                  extT = "." + ext;

               return path + name + extT; 
            }
            set
            {
               if (string.IsNullOrWhiteSpace(value))
               {
                  path = "";
                  name = "";
                  ext = "";
                  return;
               }

               int pe = value.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
               if (pe < 0)
                  path = "." + System.IO.Path.DirectorySeparatorChar;
               else
                  path = value.Substring(0, pe + 1);

               int ne = value.LastIndexOf('.');
               if (ne > pe)
               {
                  name = value.Substring(pe + 1, ne - pe - 1);
                  ext = value.Substring(ne + 1);
               }
               else
               {
                  name = value.Substring(pe + 1);
                  ext = "";
               }               
            }
         }
      }
   }
}
