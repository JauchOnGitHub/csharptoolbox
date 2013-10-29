using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Mohid.Software;
using Mohid.Files;

namespace Mohid
{
   public partial class ExportHDFByFolderForm : Form
   {
      protected bool EditingTimeSeries;
      protected bool EditingParameter;
      protected TimeseriesBlock EditingTSB;
      protected ParameterBlock EditingPB;

      public ExportHDFByFolderForm()
      {
         InitializeComponent();

         HDFSelectionCombobox.SelectedIndex = 0;
         ExportTypeCombobox.SelectedIndex = 0;
         
         UseStartCheckbox_CheckedChanged(null, null);
         UseEndCheckbox_CheckedChanged(null, null);

         UseIndexesCheckbox_CheckedChanged(null, null);

         CancelTSEditButton.Enabled = false;
         CancelParameterEditButton.Enabled = false;
      }

      private void ExportTypeCombobox_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (ExportTypeCombobox.SelectedIndex == 0) //By Coordinates
         {
            ExportByMaskOptionsGroupbox.Enabled = false;
            UsePointsCheckbox.Enabled = false;

            IndexesGroupbox.Enabled = true;
            CoordinatesGroupbox.Enabled = true;
            UseIndexesCheckbox.Enabled = true;
            TSMaskGroupbox.Enabled = false;

            JoinTimeseriesCheckbox.Enabled = false;
            TSOutputFolderTextbox.Enabled = true;
            label6.Enabled = true;
            FindTSOutputFolderButton.Enabled = true;
         }
         else //By Area
         {
            ExportByMaskOptionsGroupbox.Enabled = true;            
            UsePointsCheckbox.Enabled = true;
            UsePointsCheckbox_CheckedChanged(sender, e);

            IndexesGroupbox.Enabled = false;
            CoordinatesGroupbox.Enabled = false;
            UseIndexesCheckbox.Enabled = false;

            TSMaskGroupbox.Enabled = true;

            JoinTimeseriesCheckbox.Enabled = true;
            JoinTimeseriesCheckbox_CheckedChanged(null, null);
         }
      }

      private void UseTimeWindowCheckbox_CheckedChanged(object sender, EventArgs e)
      {

      }

      private void UsePointsCheckbox_CheckedChanged(object sender, EventArgs e)
      {
         if (UsePointsCheckbox.Checked || ExportTypeCombobox.SelectedIndex != 1)
         {
            PointsNameCombobox.Enabled = true;
            PointsGroupTextbox.Enabled = true;
         }
         else
         {
            PointsNameCombobox.Enabled = false;
            PointsGroupTextbox.Enabled = false;
         }
      }

      //private void ExportByFolderList()
      //{
      //   string config_name = "";
      //   TextFile cfg = new TextFile();
      //   ExternalApp app = new ExternalApp();

      //   System.IO.SearchOption so;
      //   FileName file = new FileName();
      //   FilePath path = new FilePath();
      //   List<Mohid.Files.FileInfo> files = new List<Mohid.Files.FileInfo>();

      //   if (SearchSubFoldersCheckbox.Checked)
      //      so = System.IO.SearchOption.AllDirectories;
      //   else
      //      so = System.IO.SearchOption.TopDirectoryOnly;

      //   foreach (string folder in FoldersCheckedlistbox.CheckedItems)
      //   {
      //      path.Path = folder;
      //      FileTools.FindFiles(ref files, path, Path.GetFileName(HDFMaskTextbox.Text), true, null, so);

      //      foreach (Mohid.Files.FileInfo fi in files)
      //      {
      //         //Find a file name for the configuration file.
      //         bool Found = false;
      //         int n = 1;
      //         int tentatives = 0;
      //         while (!Found)
      //         {
      //            config_name = ConfigInputFileNameTextbox.Text + n + ".cfg";
      //            if (System.IO.File.Exists(WorkingFolderTextbox.Text + config_name))
      //            {
      //               n++;
      //            }
      //            else
      //            {
      //               try
      //               {
      //                  cfg.File.FullPath = WorkingFolderTextbox.Text + config_name;
      //                  cfg.OpenNewToWrite();
      //                  Found = true;
      //               }
      //               catch
      //               {
      //                  n++;
      //                  tentatives++;

      //                  if (tentatives > 20)
      //                  {
      //                     MessageBox.Show("Was not possibe to create the configuration file to the HDFExporter tool.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //                     return;
      //                  }
      //               }
      //            }
      //         }               
                           
      //         //Create the Config Data File
      //         cfg.WriteLine("!File created using Mohid Toolbox");   

      //         file.FullPath = fi.FileName.FullPath;
      //         cfg.WriteLine("<BeginHDF5File>");
      //         cfg.WriteLine("  NAME : " + file.FullPath);
      //         cfg.WriteLine("<EndHDF5File>");

      //         if (ExportTypeCombobox.SelectedIndex == 1) //by Mask
      //         {
      //            cfg.WriteLine("EXPORT_TYPE       : 2");
      //            cfg.WriteLine("MASK_GRID         : " + MaskFileTextbox.Text);
      //            cfg.WriteLine("AREA_FILL_VALUE   : " + AreaFillValueTextbox.Text);
      //            if (UsePointsCheckbox.Checked)
      //            {
      //               cfg.WriteLine("USE_POINTS        : 1");
      //               cfg.WriteLine("WATERPOINTS_NAME  : " + PointsNameCombobox.Text);
      //               cfg.WriteLine("WATERPOINTS_GROUP : " + PointsGroupTextbox.Text);
      //            }
      //            else
      //               cfg.WriteLine("USE_POINTS        : 0");
      //         }
      //         else //By coordinates
      //         {
      //            cfg.WriteLine("EXPORT_TYPE       : 1");
      //            cfg.WriteLine("WATERPOINTS_NAME  : " + PointsNameCombobox.Text);
      //            cfg.WriteLine("WATERPOINTS_GROUP : " + PointsGroupTextbox.Text);
      //         }

      //         if (CheckPropertyNameCheckbox.Checked)
      //            cfg.WriteLine("CHECK_PROPERTY    : 1");
      //         else
      //            cfg.WriteLine("CHECK_PROPERTY    : 0");

      //         if (UseStartCheckbox.Checked)
      //            cfg.WriteLine("START_TIME        : " + StartDatetimepicker.Value.ToString("yyyy MM dd HH mm ss"));
      //         if (UseEndCheckbox.Checked)
      //            cfg.WriteLine("END_TIME          : " + StartDatetimepicker.Value.ToString("yyyy MM dd HH mm ss"));

      //         if (VariableGridCheckbox.Checked)
      //            cfg.WriteLine("VARIABLE_GRID     : 1");
      //         else
      //            cfg.WriteLine("VARIABLE_GRID     : 0");

      //         if (!string.IsNullOrWhiteSpace(GridFileTextbox.Text))
      //            cfg.WriteLine("GRID_FILENAME     : " + GridFileTextbox.Text);

      //         if (!string.IsNullOrWhiteSpace(TimeGroupTextbox.Text))
      //            cfg.WriteLine("TIME_GROUP        : " + TimeGroupTextbox.Text);

      //         if (!string.IsNullOrWhiteSpace(DecimationFactorTextbox.Text))
      //            cfg.WriteLine("DECIMATION_FACTOR : " + DecimationFactorTextbox.Text);

      //         foreach (TimeseriesBlock ts in TimeseriesCheckedlistbox.CheckedItems)
      //         {
      //            cfg.WriteLine("<BeginTimeSerie>");

      //            if (ExportTypeCombobox.SelectedIndex == 1) //by Mask
      //            {
      //               cfg.WriteLine("  NAME              : " + file.Path + ts.Name);
      //               if (!string.IsNullOrWhiteSpace(ts.MaskID))
      //                  cfg.WriteLine("  MASK_ID           : " + ts.MaskID);
      //               if (!string.IsNullOrWhiteSpace(ts.Layer))
      //                  cfg.WriteLine("  LAYER             : " + ts.Layer);
      //            }
      //            else
      //            {
      //               cfg.WriteLine("  NAME              : " + ts.Name);
      //               if (!string.IsNullOrWhiteSpace(ts.I))
      //                  cfg.WriteLine("  LOCALIZATION_I    : " + ts.I);
      //               if (!string.IsNullOrWhiteSpace(ts.J))
      //                  cfg.WriteLine("  LOCALIZATION_J    : " + ts.J);
      //               if (!string.IsNullOrWhiteSpace(ts.K))
      //                  cfg.WriteLine("  LOCALIZATION_J    : " + ts.K);
      //               if (!string.IsNullOrWhiteSpace(ts.Latitude))
      //                  cfg.WriteLine("  LATITUDE          : " + ts.Latitude);
      //               if (!string.IsNullOrWhiteSpace(ts.Longitude))
      //                  cfg.WriteLine("  LONGITUDE         : " + ts.Longitude);
      //               if (!string.IsNullOrWhiteSpace(ts.X))
      //                  cfg.WriteLine("  COORD_X           : " + ts.X);
      //               if (!string.IsNullOrWhiteSpace(ts.Y))
      //                  cfg.WriteLine("  COORD_Y           : " + ts.Y);
      //            }
      //            cfg.WriteLine("<EndTimeSerie>");
      //         }

      //         foreach (ParameterBlock ts in ParametersCheckedlistbox.CheckedItems)
      //         {
      //            cfg.WriteLine("<BeginParameter>");
      //            cfg.WriteLine("  PROPERTY          : " + ts.Name);
      //            cfg.WriteLine("  HDF_GROUP         : " + ts.Group);
      //            cfg.WriteLine("<EndParameter>");
      //         }
      //         cfg.Close();

      //         //run HDFExporter Tool
      //         app.Executable = PathToHDFExporterTextbox.Text;
      //         app.UseShell = false;
      //         app.WorkingDirectory = WorkingFolderTextbox.Text;
      //         app.Arguments = "-c " + config_name;
      //         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
      //         app.TextToCheck = "successfully terminated";
      //         app.Verbose = false;
      //         app.Wait = true;
      //         app.SearchTextOrder = SearchTextOrder.FROMEND;

      //         if (!app.Run())
      //         {
      //            MessageBox.Show("HDFExporter tool has failed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //            return;
      //         }

      //         if (!KeepConfigFileCheckbox.Checked)
      //         {
      //            try
      //            {
      //               System.IO.File.Delete(WorkingFolderTextbox.Text + config_name);
      //            }
      //            catch
      //            {
      //            }
      //         }
      //      }         
      //   }
      //}

      //private void ExportByFileList()
      //{
      //   string config_name = "";
      //   TextFile cfg = new TextFile();
      //   ExternalApp app = new ExternalApp();

      //   //Find a file name for the configuration file.
      //   bool Found = false;
      //   int n = 1;
      //   int tentatives = 0;
      //   while (Found)
      //   {
      //      config_name = ConfigInputFileNameTextbox.Text + n + ".cfg";
      //      if (System.IO.File.Exists(WorkingFolderTextbox.Text + config_name))
      //      {
      //         n++;
      //      }
      //      else
      //      {
      //         try
      //         {
      //            cfg.File.FullPath = WorkingFolderTextbox.Text + config_name;
      //            cfg.OpenNewToWrite();
      //            Found = true;
      //         }
      //         catch
      //         {
      //            n++;
      //            tentatives++;

      //            if (tentatives > 20)
      //            {
      //               MessageBox.Show("Was not possibe to create the configuration file to the HDFExporter tool.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //               return;
      //            }
      //         }
      //      }

      //   }

      //   //Create the Config Data File
      //   cfg.WriteLine("!File created using Mohid Toolbox");

      //   foreach (string file in HDFFileCheckedlistbox.CheckedItems)
      //   {
      //      cfg.WriteLine("<BeginHDF5File>");
      //      cfg.WriteLine("  NAME : " + file);
      //      cfg.WriteLine("<EndHDF5File>");
      //   }

      //   if (ExportTypeCombobox.SelectedIndex == 1) //by Mask
      //   {
      //      cfg.WriteLine("EXPORT_TYPE       : 2");
      //      cfg.WriteLine("MASK_GRID         : " + MaskFileTextbox.Text);
      //      cfg.WriteLine("AREA_FILL_VALUE   : " + AreaFillValueTextbox.Text);
      //      if (UsePointsCheckbox.Checked)
      //      {
      //         cfg.WriteLine("USE_POINTS        : 1");
      //         cfg.WriteLine("WATERPOINTS_NAME  : " + PointsNameCombobox.Text);
      //         cfg.WriteLine("WATERPOINTS_GROUP : " + PointsGroupTextbox.Text);
      //      }
      //      else
      //         cfg.WriteLine("USE_POINTS        : 0");
      //   }
      //   else //By coordinates
      //   {
      //      cfg.WriteLine("EXPORT_TYPE       : 1");
      //      cfg.WriteLine("WATERPOINTS_NAME  : " + PointsNameCombobox.Text);
      //      cfg.WriteLine("WATERPOINTS_GROUP : " + PointsGroupTextbox.Text);
      //   }

      //   if (CheckPropertyNameCheckbox.Checked)
      //      cfg.WriteLine("CHECK_PROPERTY    : 1");
      //   else
      //      cfg.WriteLine("CHECK_PROPERTY    : 0");

      //   if (UseStartCheckbox.Checked)
      //      cfg.WriteLine("START_TIME        : " + StartDatetimepicker.Value.ToString("yyyy MM dd HH mm ss"));
      //   if (UseEndCheckbox.Checked)
      //      cfg.WriteLine("END_TIME          : " + StartDatetimepicker.Value.ToString("yyyy MM dd HH mm ss"));

      //   if (VariableGridCheckbox.Checked)
      //      cfg.WriteLine("VARIABLE_GRID     : 1");
      //   else
      //      cfg.WriteLine("VARIABLE_GRID     : 0");

      //   if (!string.IsNullOrWhiteSpace(GridFileTextbox.Text))
      //      cfg.WriteLine("GRID_FILENAME     : " + GridFileTextbox.Text);

      //   if (!string.IsNullOrWhiteSpace(TimeGroupTextbox.Text))
      //      cfg.WriteLine("TIME_GROUP        : " + TimeGroupTextbox.Text);

      //   if (!string.IsNullOrWhiteSpace(DecimationFactorTextbox.Text))
      //      cfg.WriteLine("DECIMATION_FACTOR : " + DecimationFactorTextbox.Text);

      //   foreach (TimeseriesBlock ts in TimeseriesCheckedlistbox.CheckedItems)
      //   {
      //      cfg.WriteLine("<BeginTimeSerie>");

      //      if (ExportTypeCombobox.SelectedIndex == 1) //by Mask
      //      {
      //         cfg.WriteLine("  NAME              : " + (new FilePath(TSOutputFolderTextbox.Text)).Path + ts.Name);
      //         if (!string.IsNullOrWhiteSpace(ts.MaskID))
      //            cfg.WriteLine("  MASK_ID           : " + ts.MaskID);
      //         if (!string.IsNullOrWhiteSpace(ts.Layer))
      //            cfg.WriteLine("  LAYER             : " + ts.Layer);
      //      }
      //      else
      //      {
      //         cfg.WriteLine("  NAME              : " + ts.Name);
      //         if (!string.IsNullOrWhiteSpace(ts.I))
      //            cfg.WriteLine("  LOCALIZATION_I    : " + ts.I);
      //         if (!string.IsNullOrWhiteSpace(ts.J))
      //            cfg.WriteLine("  LOCALIZATION_J    : " + ts.J);
      //         if (!string.IsNullOrWhiteSpace(ts.K))
      //            cfg.WriteLine("  LOCALIZATION_J    : " + ts.K);
      //         if (!string.IsNullOrWhiteSpace(ts.Latitude))
      //            cfg.WriteLine("  LATITUDE          : " + ts.Latitude);
      //         if (!string.IsNullOrWhiteSpace(ts.Longitude))
      //            cfg.WriteLine("  LONGITUDE         : " + ts.Longitude);
      //         if (!string.IsNullOrWhiteSpace(ts.X))
      //            cfg.WriteLine("  COORD_X           : " + ts.X);
      //         if (!string.IsNullOrWhiteSpace(ts.Y))
      //            cfg.WriteLine("  COORD_Y           : " + ts.Y);
      //      }
      //      cfg.WriteLine("<EndTimeSerie>");
      //   }

      //   foreach (ParameterBlock ts in ParametersCheckedlistbox.CheckedItems)
      //   {
      //      cfg.WriteLine("<BeginParameter>");
      //      cfg.WriteLine("  PROPERTY          : " + ts.Name);
      //      cfg.WriteLine("  HDF_GROUP         : " + ts.Group);
      //      cfg.WriteLine("<EndParameter>");
      //   }
      //   cfg.Close();


      //   //run HDFExporter Tool
      //   app.Executable = PathToHDFExporterTextbox.Text;
      //   app.WorkingDirectory = WorkingFolderTextbox.Text;
      //   app.Arguments = "-c " + config_name;
      //   app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
      //   app.TextToCheck = "successfully terminated";
      //   app.Verbose = false;
      //   app.Wait = true;
      //   app.SearchTextOrder = SearchTextOrder.FROMEND;

      //   if (!app.Run())
      //   {
      //      MessageBox.Show("HDFExporter tool has failed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //      return;
      //   }

      //   if (!KeepConfigFileCheckbox.Checked)
      //   {
      //      try
      //      {
      //         System.IO.File.Delete(WorkingFolderTextbox.Text + config_name);
      //      }
      //      catch
      //      {
      //      }
      //   }         
      //}

      private void ExportButton_Click(object sender, EventArgs e)
      {
         Running r = new Running();

         ExportHDFToTSEngine engine = new ExportHDFToTSEngine();
         ExportHDFToTSOptions opts = new ExportHDFToTSOptions();

         opts.AreaFillValue = AreaFillValueTextbox.Text;
         opts.CheckPropertyName = CheckPropertyNameCheckbox.Checked;
         opts.ConfigFile = ConfigInputFileNameTextbox.Text;
         opts.DecimationFactor = DecimationFactorTextbox.Text;
         opts.End = EndDatetimepicker.Value;
         if (ExportTypeCombobox.SelectedIndex == 0)
            opts.FileSearchType = ExportHDFToTSFileSearchType.ByFileList;
         else
            opts.FileSearchType = ExportHDFToTSFileSearchType.ByFolderList;
         opts.GridFile = GridFileTextbox.Text;
         opts.KeepConfigFile = KeepConfigFileCheckbox.Checked;
         opts.MaskFile = MaskFileTextbox.Text;
         opts.PathToHDFExporter = PathToHDFExporterTextbox.Text;
         opts.PathToOutputTimeSeries = TSOutputFolderTextbox.Text;
         opts.PointsGroup = PointsGroupTextbox.Text;
         opts.PointsName = PointsNameCombobox.Text;
         opts.SearchSubFolders = SearchSubFoldersCheckbox.Checked;
         opts.Start = StartDatetimepicker.Value;
         opts.TimeGroup = TimeGroupTextbox.Text;
         opts.JoinTimeseries = JoinTimeseriesCheckbox.Checked;
         opts.KeepIntermediateTSFiles = KeepIntermediateTSCheckbox.Checked;
         if (ExportTypeCombobox.SelectedIndex == 0)
         {
            opts.Type = ExportHDFToTSType.ByCoordinate;
            opts.List = HDFFileCheckedlistbox.CheckedItems;
         }
         else
         {
            opts.Type = ExportHDFToTSType.ByMask;
            opts.List = FoldersCheckedlistbox.CheckedItems;
         }
         opts.UseEnd = UseEndCheckbox.Checked;
         opts.UseStart = UseStartCheckbox.Checked;
         opts.UsePoints = UsePointsCheckbox.Checked;
         opts.Variable = VariableGridCheckbox.Checked;
         opts.WorkingFolder = WorkingFolderTextbox.Text;
         opts.File = HDFMaskTextbox.Text;

         opts.TimeSeries = TimeseriesCheckedlistbox.CheckedItems;
         opts.Parameters = ParametersCheckedlistbox.CheckedItems;

         r.Text = "Exporting HDF to Timeseries";
         r.LabelText = "Processing...";
         r.ReportsProgress = true;
         r.SupportsCancellation = true;
         r.Worker = engine;
         r.Options = opts;

         r.Show();
      }

      private void LoadTemplateButton_Click(object sender, EventArgs e)
      {
         //Loads a template
      }

      private void SaveTemplateButton_Click(object sender, EventArgs e)
      {
         //Saves a template
      }

      private void FindMaskButton_Click(object sender, EventArgs e)
      {
         openFileDialog1.Filter = "*.dat|*.dat|All Files|*.*";
         openFileDialog1.AddExtension = false;
         openFileDialog1.CheckFileExists = false;
         openFileDialog1.CheckPathExists = false;
         openFileDialog1.Multiselect = false;
         openFileDialog1.RestoreDirectory = true;
         openFileDialog1.ShowHelp = true;
         openFileDialog1.ShowReadOnly = false;
         openFileDialog1.Title = "Open Mask Mohid GridData File";
         openFileDialog1.ValidateNames = false;

         if (!string.IsNullOrWhiteSpace(MaskFileTextbox.Text))
         {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(MaskFileTextbox.Text);
            openFileDialog1.FileName = Path.GetFileName(MaskFileTextbox.Text);
         }
         if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            MaskFileTextbox.Text = openFileDialog1.FileName;
         }
      }

      private void FindGridButton_Click(object sender, EventArgs e)
      {
         openFileDialog1.Filter = "*.dat|*.dat|All Files|*.*";
         openFileDialog1.AddExtension = false;
         openFileDialog1.CheckFileExists = false;
         openFileDialog1.CheckPathExists = false;
         openFileDialog1.Multiselect = false;
         openFileDialog1.RestoreDirectory = true;
         openFileDialog1.ShowHelp = true;
         openFileDialog1.ShowReadOnly = false;
         openFileDialog1.Title = "Open GridData File";
         openFileDialog1.ValidateNames = false;

         if (!string.IsNullOrWhiteSpace(GridFileTextbox.Text))
         {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(GridFileTextbox.Text);
            openFileDialog1.FileName = Path.GetFileName(GridFileTextbox.Text);
         }
         if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            GridFileTextbox.Text = openFileDialog1.FileName;
         }
      }

      private void AddNewFolderButton_Click(object sender, EventArgs e)
      {
         if (!string.IsNullOrWhiteSpace(FolderTextbox.Text))
         {
            if (FoldersCheckedlistbox.Items.Contains(FolderTextbox.Text))
            {
               MessageBox.Show("This Folder already exists in the list.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
               FoldersCheckedlistbox.Items.Add(FolderTextbox.Text, true);
         }
      }

      private void DeleteFoldersButton_Click(object sender, EventArgs e)
      {
         if (FoldersCheckedlistbox.SelectedIndex <= -1) return;

         if (MessageBox.Show("Are you sure you want to delete the selected items?", "ATTENTION", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
         {
            FoldersCheckedlistbox.Items.Remove(FoldersCheckedlistbox.SelectedItem);
         }
      }

      private void HDFSelectionCombobox_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (HDFSelectionCombobox.SelectedIndex == 0) //By Coordinates
         {
            label40.Enabled = false;
            HDFMaskTextbox.Enabled = false;
            //HDFFilesTabpage.Enabled = true;
            panel1.Enabled = true;
            TSOutputFolderTextbox.Enabled = true;
            FindTSOutputFolderButton.Enabled = true;
            label6.Enabled = true;
            JoinTimeseriesCheckbox.Enabled = false;
            KeepIntermediateTSCheckbox.Enabled = false;
         }
         else //By Mask
         {
            label40.Enabled = true;
            HDFMaskTextbox.Enabled = true;
            //HDFFilesTabpage.Enabled = false;
            panel1.Enabled = false;
            //TSOutputFolderTextbox.Enabled = false;
            //FindTSOutputFolderButton.Enabled = false;
            //label6.Enabled = false;
            JoinTimeseriesCheckbox.Enabled = true;
            JoinTimeseriesCheckbox_CheckedChanged(null, null);
            KeepIntermediateTSCheckbox.Enabled = true;
         }
      }

      private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
      {
         e.Cancel = !e.TabPage.Enabled;
      }

      private void OpenHDFFileButton_Click(object sender, EventArgs e)
      {
         openFileDialog1.Filter = "*.dat|*.hdf5|All Files|*.*";
         openFileDialog1.AddExtension = false;
         openFileDialog1.CheckFileExists = false;
         openFileDialog1.CheckPathExists = false;
         openFileDialog1.Multiselect = true;
         openFileDialog1.RestoreDirectory = true;
         openFileDialog1.ShowHelp = true;
         openFileDialog1.ShowReadOnly = false;
         openFileDialog1.Title = "Open GridData File";
         openFileDialog1.ValidateNames = false;

         if (!string.IsNullOrWhiteSpace(HDFFileTextbox.Text))
         {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(HDFFileTextbox.Text);
            openFileDialog1.FileName = Path.GetFileName(HDFFileTextbox.Text);
         }
         if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            foreach (string file in openFileDialog1.FileNames)
            {
               if (!HDFFileCheckedlistbox.Items.Contains(file))
                  HDFFileCheckedlistbox.Items.Add(file, true);
            }            
         }
      }

      private void AddHDFFileButton_Click(object sender, EventArgs e)
      {
         if (string.IsNullOrWhiteSpace(HDFFileTextbox.Text)) return;

         if (!HDFFileCheckedlistbox.Items.Contains(HDFFileTextbox.Text))
            HDFFileCheckedlistbox.Items.Add(HDFFileTextbox.Text, true);
      }

      private void DeleteHDFFileButton_Click(object sender, EventArgs e)
      {
         if (HDFFileCheckedlistbox.SelectedIndex <= -1) return;

         if (MessageBox.Show("Are you sure you want to delete the selected items?", "ATTENTION", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
         {
            HDFFileCheckedlistbox.Items.Remove(HDFFileCheckedlistbox.SelectedItem);
         }
      }

      private void FindFolderButton_Click(object sender, EventArgs e)
      {
         if (!string.IsNullOrWhiteSpace(FolderTextbox.Text))
            folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(FolderTextbox.Text);
            
         folderBrowserDialog1.ShowNewFolderButton = true;
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            FoldersCheckedlistbox.Items.Add(folderBrowserDialog1.SelectedPath, true);
         }
      }

      private void UseIndexesCheckbox_CheckedChanged(object sender, EventArgs e)
      {
         if (UseIndexesCheckbox.Checked)
         {
            IndexesGroupbox.Enabled = true;
            CoordinatesGroupbox.Enabled = false;
         }
         else
         {
            IndexesGroupbox.Enabled = false;
            CoordinatesGroupbox.Enabled = true;
         }
      }

      private void AddTSBlockButton_Click(object sender, EventArgs e)
      {
         if (string.IsNullOrWhiteSpace(NameTextbox.Text))
            return;

         if (!EditingTimeSeries)
         {
            foreach (TimeseriesBlock p in TimeseriesCheckedlistbox.Items)
               if (p.Name == NameTextbox.Text)
                  return;

            TimeseriesBlock newTSB = new TimeseriesBlock();
            newTSB.Name = NameTextbox.Text;
            newTSB.UseIndexes = UseIndexesCheckbox.Checked;

            newTSB.I = ITextbox.Text;
            newTSB.J = JTextbox.Text;
            newTSB.K = KTextbox.Text;

            newTSB.Latitude = LatitudeTextbox.Text;
            newTSB.Longitude = LongitudeTextbox.Text;

            newTSB.MaskID = MaskIDTextbox.Text;
            newTSB.Layer = LayerTextbox.Text;

            newTSB.X = XTextbox.Text;
            newTSB.Y = YTextbox.Text;

            TimeseriesCheckedlistbox.Items.Add(newTSB, true);           
         }
         else
         {
            EditingTSB.Name = NameTextbox.Text;
            EditingTSB.UseIndexes = UseIndexesCheckbox.Checked;
            EditingTSB.I = ITextbox.Text;
            EditingTSB.J = JTextbox.Text;
            EditingTSB.K = KTextbox.Text;
            EditingTSB.Latitude = LatitudeTextbox.Text;
            EditingTSB.Longitude = LongitudeTextbox.Text;
            EditingTSB.MaskID = MaskIDTextbox.Text;
            EditingTSB.Layer = LayerTextbox.Text;
            EditingTSB.X = XTextbox.Text;
            EditingTSB.Y = YTextbox.Text;
            TimeseriesCheckedlistbox.Items[EditingTSB.Index] = EditingTSB;
            NameTextbox.Enabled = true;
            EditingTimeSeries = false;
            CancelTSEditButton.Enabled = false;
            EditTSBlockButton.Enabled = true;
            DeleteTSButton.Enabled = true;
         }
      }

      private void EditTSBlockButton_Click(object sender, EventArgs e)
      {
         if (TimeseriesCheckedlistbox.SelectedIndex > -1) 
         {
            EditingTimeSeries = true;
            EditingTSB = (TimeseriesBlock)(TimeseriesCheckedlistbox.Items[TimeseriesCheckedlistbox.SelectedIndex]);
            EditingTSB.Index = TimeseriesCheckedlistbox.SelectedIndex;
            NameTextbox.Text = EditingTSB.Name;
            UseIndexesCheckbox.Checked = EditingTSB.UseIndexes;
            ITextbox.Text = EditingTSB.I;
            JTextbox.Text = EditingTSB.J;
            KTextbox.Text = EditingTSB.K;
            LatitudeTextbox.Text = EditingTSB.Latitude;
            LongitudeTextbox.Text = EditingTSB.Longitude;
            MaskIDTextbox.Text = EditingTSB.MaskID;
            LayerTextbox.Text = EditingTSB.Layer;
            XTextbox.Text = EditingTSB.X;
            YTextbox.Text = EditingTSB.Y;

            NameTextbox.Enabled = false;
            CancelTSEditButton.Enabled = true;
            EditTSBlockButton.Enabled = false;
            DeleteTSButton.Enabled = false;
         }
      }

      private void CancelTSEditButton_Click(object sender, EventArgs e)
      {
         NameTextbox.Enabled = true;
         EditingTimeSeries = false;
         CancelTSEditButton.Enabled = false;
         EditTSBlockButton.Enabled = true;
         DeleteTSButton.Enabled = true;
      }

      private void AddParameterBlockButton_Click(object sender, EventArgs e)
      {
         if (string.IsNullOrWhiteSpace(ParamNameTextbox.Text))
            return;

         if (!EditingParameter)
         {
            foreach (ParameterBlock p in ParametersCheckedlistbox.Items)
               if (p.Name == ParamNameTextbox.Text)
                  return;

            ParameterBlock newTSB = new ParameterBlock();
            newTSB.Name = ParamNameTextbox.Text;
            newTSB.Group = ParameterGroupEditbox.Text;
            ParametersCheckedlistbox.Items.Add(newTSB, true);
         }
         else
         {
            EditingPB.Name = ParamNameTextbox.Text;
            EditingPB.Group = ParameterGroupEditbox.Text;
            ParametersCheckedlistbox.Items[EditingPB.Index] = EditingPB;
            ParamNameTextbox.Enabled = true;
            EditingParameter = false;
            CancelParameterEditButton.Enabled = false;
            EditTSBlockButton.Enabled = true;
            DeleteTSButton.Enabled = true;
         }
      }

      private void ParamEditButton_Click(object sender, EventArgs e)
      {
         if (ParametersCheckedlistbox.SelectedIndex > -1)
         {
            EditingParameter = true;
            EditingPB = (ParameterBlock)(ParametersCheckedlistbox.Items[ParametersCheckedlistbox.SelectedIndex]);
            EditingPB.Index = ParametersCheckedlistbox.SelectedIndex;
            ParamNameTextbox.Text = EditingPB.Name;
            ParameterGroupEditbox.Text = EditingPB.Group;

            ParamNameTextbox.Enabled = false;
            CancelParameterEditButton.Enabled = true;
            ParamEditButton.Enabled = false;
            ParamDeleteButton.Enabled = false;
         }
      }

      private void CancelParameterEditButton_Click(object sender, EventArgs e)
      {
         ParamNameTextbox.Enabled = true;
         EditingParameter = false;
         CancelParameterEditButton.Enabled = false;
         ParamEditButton.Enabled = true;
         ParamDeleteButton.Enabled = true;
      }

      private void ParamDeleteButton_Click(object sender, EventArgs e)
      {
         if (ParametersCheckedlistbox.SelectedIndex <= -1) return;

         if (MessageBox.Show("Are you sure you want to delete the selected items?", "ATTENTION", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
         {
            ParametersCheckedlistbox.Items.Remove(ParametersCheckedlistbox.SelectedItem);
         }
      }

      private void DeleteTSButton_Click(object sender, EventArgs e)
      {
         if (TimeseriesCheckedlistbox.SelectedIndex <= -1) return;

         if (MessageBox.Show("Are you sure you want to delete the selected items?", "ATTENTION", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
         {
            TimeseriesCheckedlistbox.Items.Remove(TimeseriesCheckedlistbox.SelectedItem);
         }
      }

      private void CloseButton_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void UseEndCheckbox_CheckedChanged(object sender, EventArgs e)
      {
         if (UseEndCheckbox.Checked)
         {
            EndDatetimepicker.Enabled = true;
         }
         else
         {
            EndDatetimepicker.Enabled = false;
         }
      }

      private void UseStartCheckbox_CheckedChanged(object sender, EventArgs e)
      {
         if (UseStartCheckbox.Checked)
         {
            StartDatetimepicker.Enabled = true;
         }
         else
         {
            StartDatetimepicker.Enabled = false;
         }
      }

      private void FindHDFFileNameButton_Click(object sender, EventArgs e)
      {
         openFileDialog1.Filter = "*.dat|*.hdf5|All Files|*.*";
         openFileDialog1.AddExtension = false;
         openFileDialog1.CheckFileExists = false;
         openFileDialog1.CheckPathExists = false;
         openFileDialog1.Multiselect = false;
         openFileDialog1.RestoreDirectory = true;
         openFileDialog1.ShowHelp = true;
         openFileDialog1.ShowReadOnly = false;
         openFileDialog1.Title = "Open HDF5 Input File";
         openFileDialog1.ValidateNames = false;

         if (!string.IsNullOrWhiteSpace(HDFMaskTextbox.Text))
         {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(HDFMaskTextbox.Text);
            openFileDialog1.FileName = Path.GetFileName(HDFMaskTextbox.Text);
         }
         if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            HDFMaskTextbox.Text = openFileDialog1.SafeFileName;
         }
      }

      private void FindHDFExporterButton_Click(object sender, EventArgs e)
      {
         openFileDialog1.Filter = "*.dat|*.exe|All Files|*.*";
         openFileDialog1.AddExtension = false;
         openFileDialog1.CheckFileExists = false;
         openFileDialog1.CheckPathExists = false;
         openFileDialog1.Multiselect = false;
         openFileDialog1.RestoreDirectory = true;
         openFileDialog1.ShowHelp = true;
         openFileDialog1.ShowReadOnly = false;
         openFileDialog1.Title = "Open HDFExporter EXE File";
         openFileDialog1.ValidateNames = false;

         if (!string.IsNullOrWhiteSpace(PathToHDFExporterTextbox.Text))
         {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(PathToHDFExporterTextbox.Text);
            openFileDialog1.FileName = Path.GetFileName(PathToHDFExporterTextbox.Text);
         }
         if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            PathToHDFExporterTextbox.Text = openFileDialog1.FileName;
         }
      }

      private void FindWorkingFolderbutton_Click(object sender, EventArgs e)
      {
         if (!string.IsNullOrWhiteSpace(WorkingFolderTextbox.Text))
            folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(WorkingFolderTextbox.Text);

         folderBrowserDialog1.ShowNewFolderButton = true;
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            WorkingFolderTextbox.Text = folderBrowserDialog1.SelectedPath;            
         }
      }

      private void FindTSOutputFolderButton_Click(object sender, EventArgs e)
      {
         if (!string.IsNullOrWhiteSpace(TSOutputFolderTextbox.Text))
            folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(TSOutputFolderTextbox.Text);

         folderBrowserDialog1.ShowNewFolderButton = true;
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            TSOutputFolderTextbox.Text = folderBrowserDialog1.SelectedPath;
         }
      }

      private void JoinTimeseriesCheckbox_CheckedChanged(object sender, EventArgs e)
      {
         if (JoinTimeseriesCheckbox.Checked)
         {
            TSOutputFolderTextbox.Enabled = true;
            KeepIntermediateTSCheckbox.Enabled = true;
            label6.Enabled = true;
            FindTSOutputFolderButton.Enabled = true;
         }
         else
         {
            TSOutputFolderTextbox.Enabled = false;
            KeepIntermediateTSCheckbox.Enabled = false;
            label6.Enabled = false;            
            FindTSOutputFolderButton.Enabled = false;
         }
      }
   }
}
