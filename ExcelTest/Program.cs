using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Graphs;

namespace ExcelTest
{
   class Program
   {
      static void Main(string[] args)
      {
         ExcelGraph eg = new ExcelGraph();

         eg.Run(true);
      }
   }
}
