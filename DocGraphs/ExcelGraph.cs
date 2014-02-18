using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;

using Mohid;
using Mohid.Core;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;

namespace Mohid.Graphs
{
   public class LineFormat
   {
      public bool Visible;
      public int LineStyle;
      
   }

   public class SeriesFormat
   {

   }

   public class Series
   {
      public SeriesFormat Format;
      public TimeSeries TimeSeries;
      public int Column;
      public string Legend;
   }

   public class ExcelGraph
   {
      protected Exception f_exception;

      public ConfigNode Task;
      public Exception Exception { get { return f_exception; } }

      public ExcelGraph()
      {
         f_exception = null;
         Task = null;
      }

      public bool Run(bool throw_exception = false)
      {
         try
         {
            CreateNewChart();

            return true;
         }
         catch (Exception ex)
         {
            if (throw_exception) throw;

            f_exception = ex;
            return false;
         }
      }

      protected void CreateNewChart()
      {

         Application xlApp;
         Workbook xlWorkBook;
         Worksheet xlWorkSheet;
         object misValue = System.Reflection.Missing.Value;

         xlApp = new Application();
         xlWorkBook = xlApp.Workbooks.Add(misValue);
         xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);

         xlWorkSheet.Cells[1, 1] = "";
         xlWorkSheet.Cells[1, 2] = "Student1";
         xlWorkSheet.Cells[1, 3] = "Student2";
         xlWorkSheet.Cells[1, 4] = "Student3";

         xlWorkSheet.Cells[2, 1] = "Term1";
         xlWorkSheet.Cells[2, 2] = "80";
         xlWorkSheet.Cells[2, 3] = "65";
         xlWorkSheet.Cells[2, 4] = "45";

         xlWorkSheet.Cells[3, 1] = "Term2";
         xlWorkSheet.Cells[3, 2] = "78";
         xlWorkSheet.Cells[3, 3] = "72";
         xlWorkSheet.Cells[3, 4] = "60";

         xlWorkSheet.Cells[4, 1] = "Term3";
         xlWorkSheet.Cells[4, 2] = "82";
         xlWorkSheet.Cells[4, 3] = "80";
         xlWorkSheet.Cells[4, 4] = "65";

         xlWorkSheet.Cells[5, 1] = "Term4";
         xlWorkSheet.Cells[5, 2] = "75";
         xlWorkSheet.Cells[5, 3] = "82";
         xlWorkSheet.Cells[5, 4] = "68";

         Range chartRange;

         ChartObjects xlCharts = (ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
         ChartObject myChart = (ChartObject)xlCharts.Add(10, 80, 300, 250);
         _Chart chartPage = myChart.Chart;

         chartRange = xlWorkSheet.get_Range("A1", "d5");
         chartPage.SetSourceData(chartRange, misValue);
         chartPage.ChartType = XlChartType.xlColumnClustered;

         Microsoft.Office.Interop.Excel.Series serie = chartPage.SeriesCollection(1);
         serie.

         xlWorkBook.SaveAs(@"d:\test.xlsx", XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
         xlWorkBook.Close(true, misValue, misValue);
         xlApp.Quit();

         releaseObject(xlWorkSheet);
         releaseObject(xlWorkBook);
         releaseObject(xlApp);

      }

      private void releaseObject(object obj)
      {
         try
         {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            obj = null;
         }
         catch (Exception ex)
         {
            obj = null;
         }
         finally
         {
            GC.Collect();
         }
      }
   }
}
