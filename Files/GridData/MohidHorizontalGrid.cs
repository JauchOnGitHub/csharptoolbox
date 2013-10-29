using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Files
   {
      public class MohidHorizontalGrid
      {
         public FileName GridDataFile { get; set; }
         public FileName GridFile { get; set; }
         public int ILB { get; set; }
         public int IUB { get; set; }
         public int JLB { get; set; }
         public int JUB { get; set; }
         public int Elements { get; set; }
         public List<double> XX { get; set; }
         public List<double> YY { get; set; }

         public MohidHorizontalGrid()
         {
            XX = new List<double>();
            YY = new List<double>();
         }

         public void LoadFromGridDataFile()
         {
         }

         public void ParseHeader(ref List<string> header)
         {
         }

         public void LoadFromGridFile()
         {
         }

         public void SaveToGridFile()
         {
         }
      }
   }
}
