using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Mohid.Core;
using Mohid.Files;
using Mohid.MohidTimeSeries;

namespace Mohid
{

   public partial class JoinTimeseriesByFolderForm : Form
   {
      private FileName output;     

      public JoinTimeseriesByFolderForm()
      {
         InitializeComponent();

         output = new FileName("joined.timeseries.tsr");
         OutputTimeseriesTextbox.Text = output.FullPath;
         TimeUnitsCombobox.SelectedIndex = 0;
      }

      private void OutputButton_Click(object sender, EventArgs e)
      {
         FindOutputDialog.FileName = output.FullPath;
         if (FindOutputDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            OutputTimeseriesTextbox.Text = FindOutputDialog.FileName;
         }
      }

      private void AddButton_Click(object sender, EventArgs e)
      {
         if (FindFoldersDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            FoldersList.Items.Add(FindFoldersDialog.SelectedPath); 
           

         }
      }

      private void ExcludeButton_Click(object sender, EventArgs e)
      {
         while (FoldersList.SelectedItems.Count > 0)
         {
            FoldersList.Items.Remove(FoldersList.SelectedItem);
         }
      }

      private void CancelButton_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void JoinButton_Click(object sender, EventArgs e)
      {
         try
         {
            if (FoldersList.Items.Count <= 0)
               throw new GeneralException("No folders to search timeseries were defined.", ExceptionType.WARNING);

            if (string.IsNullOrWhiteSpace(OutputTimeseriesTextbox.Text))
               throw new GeneralException("You must define the output timeseries.", ExceptionType.WARNING);
            else
            {
               try
               {
                  output.FullPath = OutputTimeseriesTextbox.Text;

                  if (!System.IO.Directory.Exists(output.Path))
                     throw new GeneralException("The path '" + output.Path + "' doesn't exist.", ExceptionType.WARNING);
                  else if (System.IO.File.Exists(output.FullPath))
                  {
                     if (MessageBox.Show("The file '" + output.FullName + "' exists. Do you want to OVERWRITE it?", "WARNING", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                        return;
                  }
               }
               catch (GeneralException)
               {
                  throw;
               }
               catch (Exception ex)
               {
                  throw new GeneralException(ex.Message, ExceptionType.ERROR);
               }

               if (string.IsNullOrWhiteSpace(FilterTextbox.Text))
                  throw new GeneralException("Filter was not set.", ExceptionType.WARNING);
            }

            FileName file = new FileName();
            FilePath path = new FilePath();
            List<FileInfo> files = new List<FileInfo>();
            List<TimeSeries> timeSeries = new List<TimeSeries>();
            System.IO.SearchOption so; 

            if (SearchSubFoldersCheckbox.Checked)
               so = System.IO.SearchOption.AllDirectories;
            else
               so = System.IO.SearchOption.TopDirectoryOnly;

            foreach (object ts in FoldersList.Items)
            {
               path.Path = (string)ts;
               FileTools.FindFiles(ref files, path, FilterTextbox.Text, true, null, so); 

               foreach(FileInfo fi in files)
               {
                  file.FullPath = fi.FileName.FullPath;
                  TimeSeries newTS = new TimeSeries();
                  newTS.Load(file);
                  timeSeries.Add(newTS);
               }
            }

            if (timeSeries.Count <= 0)
               throw new GeneralException("No timeseries were found.", ExceptionType.WARNING);

            DateTime start = timeSeries[0].StartInstant;
            for (int i = 1; i < timeSeries.Count; i++)
            {
               if (timeSeries[i].StartInstant < start)
                  start = timeSeries[i].StartInstant;
            }

            TimeSeries outTS = new TimeSeries();
            outTS.StartInstant = start;
            outTS.TimeUnits = (TimeUnits)Enum.Parse(typeof(TimeUnits), (string)TimeUnitsCombobox.SelectedItem, true);

            foreach (Column col in timeSeries[0].Columns)
            {
               Column newCol = new Column(col.ColumnType);
               newCol.Header = col.Header;
               outTS.AddColumn(newCol);
            }

            foreach (TimeSeries toJoin in timeSeries)
               outTS.AddTimeSeries(toJoin);

            output.FullPath = OutputTimeseriesTextbox.Text;

            outTS.Save(output);
            MessageBox.Show("Proccess Completed.");
         }
         catch (GeneralException gex)
         {
            switch (gex.Type)
            {
               case ExceptionType.ERROR:
                  MessageBox.Show(gex.Message, "ERROR", MessageBoxButtons.OK);
                  break;
               case ExceptionType.WARNING:
                  MessageBox.Show(gex.Message, "WARNING", MessageBoxButtons.OK);
                  break;
               default:
                  MessageBox.Show("An unknown error happened. The message returned was: " + gex.Message, "ERROR", MessageBoxButtons.OK);
                  break;
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show("An unknown error happened. The message returned was: " + ex.Message, "ERROR", MessageBoxButtons.OK);
         }
      }
   }
}
