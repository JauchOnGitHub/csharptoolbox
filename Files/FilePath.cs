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
            path = "." + System.IO.Path.DirectorySeparatorChar;
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
                     path = "." + System.IO.Path.DirectorySeparatorChar;
                  }
                  else if (value[value.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                  {
                     path = value + System.IO.Path.DirectorySeparatorChar;
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