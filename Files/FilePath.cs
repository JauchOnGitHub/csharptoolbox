using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Files
   {
      public class FilePath
      {
         protected string path;

         public FilePath()
         {
            path = ".\\";
         }
         public FilePath(string path)
         {
            Path = path;
         }

         public string Path
         {
            get
            {
               return path;
            }
            set
            {
               if (value == null) path = "";
               else
               {
                  if (value.Length == 0)
                  {
                     path = ".\\";
                  }
                  else if (value[value.Length - 1] != '\\')
                  {
                     path = value + "\\";
                  }
                  else
                  {
                     path = value;
                  }
               }
            }
         }
      }
   }
}