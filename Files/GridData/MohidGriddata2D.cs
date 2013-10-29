using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Mohid
{
   namespace Files
   {
      public class MohidGriddata2D
      {
         public List<string> Header;
         public List<double> Data;
         public MohidHorizontalGrid HorizGrid;
         public string DataFormat { get; set; }         

         public MohidGriddata2D()
         {
            Header = new List<string>();
            Data = new List<double>();
            HorizGrid = null;            
            DataFormat = "#.##############E+000";            
         }

         public void Load(FileName file, bool loadHG)
         {
            TextFile f = new TextFile(file);            
            string [] seps = new string[] {":", " "};
            string [] tokens;
            int i = 0, j = 0, elements = 0, pos = 0;
            string line, line_u;

            if (loadHG && HorizGrid == null) 
               HorizGrid = new MohidHorizontalGrid();
                        
            f.OpenToRead(FileShare.Read);
            Header.Clear();

            for(;;)
            {
               line = f.ReadLine();

               if (line == null) 
                  break;

               line = line.Trim();

               if (string.IsNullOrWhiteSpace(line))
                  continue;

               line_u = line.ToUpper();

               if (line_u == "<BEGINXX>")
               {
                  if (loadHG)
                  {                     
                     pos = 0;

                     for(;;)
                     {
                        line = f.ReadLine();

                        if (line == null) 
                           throw new Exception("Invalid GridData file.");

                        line = line.Trim();
                        if (line.ToUpper() == "<ENDXX>")
                           break;

                        HorizGrid.XX.Add(double.Parse(line, CultureInfo.InvariantCulture));
                        pos = pos + 1;
                     }
                  }
               }
               else if (line_u == "<BEGINYY>")
               {
                  if (loadHG) 
                  {                     
                     pos = 0;
                     for(;;)
                     {
                        line = f.ReadLine();                        
                        if (line == null)  
                           throw new Exception("Invalid GridData file");

                        line = line.Trim();
                        if (line.ToUpper() == "<ENDYY>")  
                           break;

                        HorizGrid.YY.Add(double.Parse(line, CultureInfo.InvariantCulture));
                        pos = pos + 1;
                     }
                  }                  
               }
               else if (line_u == "<BEGINGRIDDATA2D>")
               {
                  pos = 0;
                  for(;;)
                  {
                     line = f.ReadLine();
                     if (line == null)  
                        throw new Exception("Invalid GridData file.");

                     line = line.Trim();
                     if (line.ToUpper() == "<ENDGRIDDATA2D>")  
                        break;

                     Data.Add(double.Parse(line, CultureInfo.InvariantCulture));
                     pos = pos + 1;
                  }
               }
               else
               {
                  Header.Add(line);
                  tokens = line.ToUpper().Split(seps, StringSplitOptions.RemoveEmptyEntries);
                  switch (tokens[0])
                  {
                     case "ILB_IUB":
                        i = int.Parse(tokens[2]) - int.Parse(tokens[1]) + 1;
                        elements = i * j;
                        if (loadHG)
                        {
                           HorizGrid.ILB = int.Parse(tokens[1]);
                           HorizGrid.IUB = int.Parse(tokens[2]);
                           HorizGrid.Elements = elements;
                        }
                        break;
                     case "JLB_JUB":
                        j = int.Parse(tokens[2]) - int.Parse(tokens[1]) + 1;
                        elements = i * j;
                        if (loadHG) 
                        {
                           HorizGrid.JLB = int.Parse(tokens[1]);
                           HorizGrid.JUB = int.Parse(tokens[2]);
                           HorizGrid.Elements = elements;
                        }
                        break;
                   }
               }
            }

            f.Close();
         }

         public void Save(FileName file)
         {
            TextFile f = new TextFile(file);
            
            f.OpenNewToWrite(FileShare.None);

            foreach (string line in Header)
               f.WriteLine(line);            

            int i = 0;

            if (HorizGrid != null)
            {
               f.WriteLine("<BeginXX>");
               for (i = 0; i < HorizGrid.XX.Count; i++)
                  f.WriteLine(HorizGrid.XX[i].ToString(DataFormat));               
               f.WriteLine("<EndXX>");
               f.WriteLine("<BeginYY>");
               for (i = 0; i < HorizGrid.YY.Count; i++)
                  f.WriteLine(HorizGrid.YY[i].ToString(DataFormat));               
               f.WriteLine("<EndYY>");
            }

            f.WriteLine("<BeginGridData2D>");
            for (i = 0; i < Data.Count; i++)
               f.WriteLine(Data[i].ToString(DataFormat));
            f.WriteLine("<EndGridData2D>");
            f.Close();
         }

         public double this[int index]
         {
            get { return Data[index]; }
            set { Data[index] = value; }
         }
      }
   }
}
