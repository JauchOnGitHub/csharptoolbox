using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlueCPTECFiles
{
   class Program
   {
      static void Main(string[] args)
      {
         if (args.Length < 5)
            throw new Exception("Missing arguments");

         DateTime start = DateTime.ParseExact(args[0], "yyyyMMdd", null);

      }
   }
}
