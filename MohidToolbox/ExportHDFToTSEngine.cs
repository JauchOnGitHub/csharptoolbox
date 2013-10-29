using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Mohid;
using Mohid.Software;
using Mohid.Files;
using Mohid.MohidTimeSeries;

namespace Mohid
{
   public class ExportHDFToTSEngine : BackgroundWorker
   {
      public ExportHDFToTSEngine()
      {
         (this as BackgroundWorker).DoWork += new DoWorkEventHandler(DoWork);
      }

      private new void DoWork(object sender, DoWorkEventArgs e)
      {
         //ExternalApp app = new ExternalApp();
         ExportHDFToTSOptions opts = e.Argument as ExportHDFToTSOptions;

         if (opts.FileSearchType == ExportHDFToTSFileSearchType.ByFileList)
            ExportByFileList(opts, e);
         else
            ExportByFolderList(opts, e);

         if (this.CancellationPending)
            e.Cancel = true;
      }

      private void ExportByFileList(ExportHDFToTSOptions opts, DoWorkEventArgs e)
      {
         string config_name = "";
         TextFile cfg = new TextFile();
         ExternalApp app = new ExternalApp();

         //Find a file name for the configuration file.
         bool Found = false;
         int n = 1;
         int tentatives = 0;
         while (!Found)
         {
            config_name = opts.ConfigFile + n + ".cfg";
            if (System.IO.File.Exists(opts.WorkingFolder + config_name))
            {
               n++;
            }
            else
            {
               try
               {
                  cfg.File.FullPath = opts.WorkingFolder + config_name;
                  cfg.OpenNewToWrite();
                  Found = true;
               }
               catch
               {
                  n++;
                  tentatives++;

                  if (tentatives > 20)
                  {
                     MessageBox.Show("Was not possibe to create the configuration file to the HDFExporter tool.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     return;
                  }
               }
            }
         }

         //Create the Config Data File
         cfg.WriteLine("!File created using Mohid Toolbox");

         foreach (string file in opts.List)
         {
            cfg.WriteLine("<BeginHDF5File>");
            cfg.WriteLine("  NAME : " + file);
            cfg.WriteLine("<EndHDF5File>");
         }

         if (opts.Type == ExportHDFToTSType.ByMask) //by Mask
         {
            cfg.WriteLine("EXPORT_TYPE       : 2");
            cfg.WriteLine("MASK_GRID         : " + opts.MaskFile);
            cfg.WriteLine("AREA_FILL_VALUE   : " + opts.AreaFillValue);
            if (opts.UsePoints)
            {
               cfg.WriteLine("USE_POINTS        : 1");
               cfg.WriteLine("WATERPOINTS_NAME  : " + opts.PointsName);
               cfg.WriteLine("WATERPOINTS_GROUP : " + opts.PointsGroup);
            }
            else
               cfg.WriteLine("USE_POINTS        : 0");
         }
         else //By coordinates
         {
            cfg.WriteLine("EXPORT_TYPE       : 1");
            cfg.WriteLine("WATERPOINTS_NAME  : " + opts.PointsName);
            cfg.WriteLine("WATERPOINTS_GROUP : " + opts.PointsGroup);
         }

         if (opts.CheckPropertyName)
            cfg.WriteLine("CHECK_PROPERTY    : 1");
         else
            cfg.WriteLine("CHECK_PROPERTY    : 0");

         if (opts.UseStart)
            cfg.WriteLine("START_TIME        : " + opts.Start.ToString("yyyy MM dd HH mm ss"));
         if (opts.UseEnd)
            cfg.WriteLine("END_TIME          : " + opts.End.ToString("yyyy MM dd HH mm ss"));

         if (opts.Variable)
            cfg.WriteLine("VARIABLE_GRID     : 1");
         else
            cfg.WriteLine("VARIABLE_GRID     : 0");

         if (!string.IsNullOrWhiteSpace(opts.GridFile))
            cfg.WriteLine("GRID_FILENAME     : " + opts.GridFile);

         if (!string.IsNullOrWhiteSpace(opts.TimeGroup))
            cfg.WriteLine("TIME_GROUP        : " + opts.TimeGroup);

         if (!string.IsNullOrWhiteSpace(opts.DecimationFactor))
            cfg.WriteLine("DECIMATION_FACTOR : " + opts.DecimationFactor);

         foreach (TimeseriesBlock ts in opts.TimeSeries)
         {
            cfg.WriteLine("<BeginTimeSerie>");

            if (opts.Type == ExportHDFToTSType.ByMask) //by Mask
            {
               cfg.WriteLine("  NAME              : " + (new FilePath(opts.PathToOutputTimeSeries)).Path + ts.Name);
               if (!string.IsNullOrWhiteSpace(ts.MaskID))
                  cfg.WriteLine("  MASK_ID           : " + ts.MaskID);
               if (!string.IsNullOrWhiteSpace(ts.Layer))
                  cfg.WriteLine("  LAYER             : " + ts.Layer);
            }
            else
            {
               cfg.WriteLine("  NAME              : " + (new FilePath(opts.PathToOutputTimeSeries)).Path + ts.Name);
               if (!string.IsNullOrWhiteSpace(ts.I))
                  cfg.WriteLine("  LOCALIZATION_I    : " + ts.I);
               if (!string.IsNullOrWhiteSpace(ts.J))
                  cfg.WriteLine("  LOCALIZATION_J    : " + ts.J);
               if (!string.IsNullOrWhiteSpace(ts.K))
                  cfg.WriteLine("  LOCALIZATION_J    : " + ts.K);
               if (!string.IsNullOrWhiteSpace(ts.Latitude))
                  cfg.WriteLine("  LATITUDE          : " + ts.Latitude);
               if (!string.IsNullOrWhiteSpace(ts.Longitude))
                  cfg.WriteLine("  LONGITUDE         : " + ts.Longitude);
               if (!string.IsNullOrWhiteSpace(ts.X))
                  cfg.WriteLine("  COORD_X           : " + ts.X);
               if (!string.IsNullOrWhiteSpace(ts.Y))
                  cfg.WriteLine("  COORD_Y           : " + ts.Y);
            }
            cfg.WriteLine("<EndTimeSerie>");
         }

         foreach (ParameterBlock p in opts.Parameters)
         {
            cfg.WriteLine("<BeginParameter>");
            cfg.WriteLine("  PROPERTY          : " + p.Name);
            cfg.WriteLine("  HDF_GROUP         : " + p.Group);
            cfg.WriteLine("<EndParameter>");
         }
         cfg.Close();

         //run HDFExporter Tool
         app.Executable = opts.PathToHDFExporter;
         app.UseShell = false;
         app.WorkingDirectory = opts.WorkingFolder;
         app.Arguments = "-c " + config_name;
         app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
         app.TextToCheck = "successfully terminated";
         app.Verbose = false;
         app.Wait = false;
         app.SearchTextOrder = SearchTextOrder.FROMEND;

         Software.AppExitStatus es;

         if ((es = app.Run(this, e)) != AppExitStatus.Finished)
         {
            if (es != AppExitStatus.Canceled)
               MessageBox.Show("HDFExporter tool has failed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         if (!opts.KeepConfigFile)
         {
            try
            {
               System.IO.File.Delete(opts.WorkingFolder + config_name);
            }
            catch
            {
            }
         }
      }

      private void ExportByFolderList(ExportHDFToTSOptions opts, DoWorkEventArgs e)
      {
         string config_name = "";
         TextFile cfg = new TextFile();
         ExternalApp app = new ExternalApp();
         List<TimeSeries> outputTS = new List<TimeSeries>();
         bool first = true;

         System.IO.SearchOption so;
         FileName file = new FileName();
         FilePath path = new FilePath();
         List<Mohid.Files.FileInfo> files = new List<Mohid.Files.FileInfo>();

         if (opts.SearchSubFolders)
            so = System.IO.SearchOption.AllDirectories;
         else
            so = System.IO.SearchOption.TopDirectoryOnly;

         foreach (string folder in opts.List)
         {
            path.Path = folder;
            FileTools.FindFiles(ref files, path, Path.GetFileName(opts.File), false, null, so);
         }

         int totalFiles = files.Count;
         int filesProcessed = 0;

         foreach (Mohid.Files.FileInfo fi in files)
         {
            //Find a file name for the configuration file.
            if (this.CancellationPending)
            {
               return;
            }

            bool Found = false;
            int n = 1;
            int tentatives = 0;
            while (!Found)
            {
               config_name = opts.ConfigFile + n + ".cfg";
               if (System.IO.File.Exists(opts.WorkingFolder + config_name))
               {
                  n++;
               }
               else
               {
                  try
                  {
                     cfg.File.FullPath = opts.WorkingFolder + config_name;
                     cfg.OpenNewToWrite();
                     Found = true;
                  }
                  catch
                  {
                     n++;
                     tentatives++;

                     if (tentatives > 20)
                     {
                        MessageBox.Show("Was not possibe to create the configuration file to the HDFExporter tool.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                     }
                  }
               }
            }

            //Create the Config Data File
            cfg.WriteLine("!File created using Mohid Toolbox");

            file.FullPath = fi.FileName.FullPath;
            cfg.WriteLine("<BeginHDF5File>");
            cfg.WriteLine("  NAME : " + file.FullPath);
            cfg.WriteLine("<EndHDF5File>");

            if (opts.Type == ExportHDFToTSType.ByMask) //by Mask
            {
               cfg.WriteLine("EXPORT_TYPE       : 2");
               cfg.WriteLine("MASK_GRID         : " + opts.MaskFile);
               cfg.WriteLine("AREA_FILL_VALUE   : " + opts.AreaFillValue);
               if (opts.UsePoints)
               {
                  cfg.WriteLine("USE_POINTS        : 1");
                  cfg.WriteLine("WATERPOINTS_NAME  : " + opts.PointsName);
                  cfg.WriteLine("WATERPOINTS_GROUP : " + opts.PointsGroup);
               }
               else
                  cfg.WriteLine("USE_POINTS        : 0");
            }
            else //By coordinates
            {
               cfg.WriteLine("EXPORT_TYPE       : 1");
               cfg.WriteLine("WATERPOINTS_NAME  : " + opts.PointsName);
               cfg.WriteLine("WATERPOINTS_GROUP : " + opts.PointsGroup);
            }

            if (opts.CheckPropertyName)
               cfg.WriteLine("CHECK_PROPERTY    : 1");
            else
               cfg.WriteLine("CHECK_PROPERTY    : 0");

            if (opts.UseStart)
               cfg.WriteLine("START_TIME        : " + opts.Start.ToString("yyyy MM dd HH mm ss"));
            if (opts.UseEnd)
               cfg.WriteLine("END_TIME          : " + opts.End.ToString("yyyy MM dd HH mm ss"));

            if (opts.Variable)
               cfg.WriteLine("VARIABLE_GRID     : 1");
            else
               cfg.WriteLine("VARIABLE_GRID     : 0");

            if (!string.IsNullOrWhiteSpace(opts.GridFile))
               cfg.WriteLine("GRID_FILENAME     : " + opts.GridFile);

            if (!string.IsNullOrWhiteSpace(opts.TimeGroup))
               cfg.WriteLine("TIME_GROUP        : " + opts.TimeGroup);

            if (!string.IsNullOrWhiteSpace(opts.DecimationFactor))
               cfg.WriteLine("DECIMATION_FACTOR : " + opts.DecimationFactor);

            foreach (TimeseriesBlock ts in opts.TimeSeries)
            {
               cfg.WriteLine("<BeginTimeSerie>");

               if (opts.Type == ExportHDFToTSType.ByMask) //by Mask
               {
                  cfg.WriteLine("  NAME              : " + file.Path + ts.Name);
                  if (!string.IsNullOrWhiteSpace(ts.MaskID))
                     cfg.WriteLine("  MASK_ID           : " + ts.MaskID);
                  if (!string.IsNullOrWhiteSpace(ts.Layer))
                     cfg.WriteLine("  LAYER             : " + ts.Layer);
               }
               else
               {
                  cfg.WriteLine("  NAME              : " + file.Path + ts.Name);
                  if (!string.IsNullOrWhiteSpace(ts.I))
                     cfg.WriteLine("  LOCALIZATION_I    : " + ts.I);
                  if (!string.IsNullOrWhiteSpace(ts.J))
                     cfg.WriteLine("  LOCALIZATION_J    : " + ts.J);
                  if (!string.IsNullOrWhiteSpace(ts.K))
                     cfg.WriteLine("  LOCALIZATION_J    : " + ts.K);
                  if (!string.IsNullOrWhiteSpace(ts.Latitude))
                     cfg.WriteLine("  LATITUDE          : " + ts.Latitude);
                  if (!string.IsNullOrWhiteSpace(ts.Longitude))
                     cfg.WriteLine("  LONGITUDE         : " + ts.Longitude);
                  if (!string.IsNullOrWhiteSpace(ts.X))
                     cfg.WriteLine("  COORD_X           : " + ts.X);
                  if (!string.IsNullOrWhiteSpace(ts.Y))
                     cfg.WriteLine("  COORD_Y           : " + ts.Y);
               }
               cfg.WriteLine("<EndTimeSerie>");
            }

            foreach (ParameterBlock p in opts.Parameters)
            {
               cfg.WriteLine("<BeginParameter>");
               cfg.WriteLine("  PROPERTY          : " + p.Name);
               cfg.WriteLine("  HDF_GROUP         : " + p.Group);
               cfg.WriteLine("<EndParameter>");
            }
            cfg.Close();

            //run HDFExporter Tool
            app.Executable = opts.PathToHDFExporter;
            app.UseShell = false;
            app.WorkingDirectory = opts.WorkingFolder;
            app.Arguments = "-c " + config_name;
            app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            app.TextToCheck = "successfully terminated";
            app.Verbose = false;
            app.Wait = true;
            app.SearchTextOrder = SearchTextOrder.FROMEND;

            AppExitStatus es;

            if ((es = app.Run(this, e)) != AppExitStatus.Finished)
            {
               if (es != AppExitStatus.Canceled)
                  MessageBox.Show("HDFExporter tool has failed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return;
            }

            if (!opts.KeepConfigFile)
            {
               try
               {
                  System.IO.File.Delete(opts.WorkingFolder + config_name);
               }
               catch
               {
               }
            }

            if (opts.JoinTimeseries)
            {
               if (first)
               {
                  foreach (TimeseriesBlock tsb in opts.TimeSeries)
                  {
                     TimeSeries new_ts = new TimeSeries();
                     new_ts.Load(new FileName(file.Path + tsb.Name + ".ets"));
                     new_ts.Name = tsb.Name + ".ets";
                     outputTS.Add(new_ts);
                  }

                  first = false;
               }
               else
               {
                  int count = 0;
                  foreach (TimeseriesBlock tsb in opts.TimeSeries)
                  {
                     TimeSeries new_ts = new TimeSeries();
                     new_ts.Load(new FileName(file.Path + tsb.Name + ".ets"));
                     TimeSeries this_ts = outputTS[count];
                     this_ts.AddTimeSeries(new_ts);
                     new_ts = null;
                     if (!opts.KeepIntermediateTSFiles)
                     {
                        try
                        {
                           System.IO.File.Delete(file.Path + tsb.Name + ".ets");
                        }
                        catch
                        {
                        }
                     }
                     count++;
                  }
               }
            }

            if (this.WorkerReportsProgress)
            {
               filesProcessed++;
               int percentComplete = (int)((float)filesProcessed / (float)totalFiles * 100);
               this.ReportProgress(percentComplete);
            }
         }

         if (opts.JoinTimeseries)
         {
            foreach (TimeSeries ts in outputTS)
            {
               ts.Save(new FileName(new FilePath(opts.PathToOutputTimeSeries).Path + ts.Name + ".ets"));
            }
         }
      }
   }


   public enum ExportHDFToTSFileSearchType
   {
      ByFileList,
      ByFolderList
   }

   public enum ExportHDFToTSType
   {
      ByCoordinate,
      ByMask
   }

   public class ExportHDFToTSOptions
   {
      public ExportHDFToTSFileSearchType FileSearchType;
      public CheckedListBox.CheckedItemCollection List, //To Folders and HDF Files lists.
                                                  TimeSeries,
                                                  Parameters;
      public ExportHDFToTSType Type;

      public bool SearchSubFolders,
                  UsePoints,
                  CheckPropertyName,
                  UseStart,
                  UseEnd,
                  Variable,
                  KeepConfigFile,
                  JoinTimeseries,
                  KeepIntermediateTSFiles;

      public string File, //To HDF Mask file
                    ConfigFile,
                    MaskFile,
                    GridFile,
                    AreaFillValue,
                    WorkingFolder,
                    PointsName,
                    PointsGroup,
                    TimeGroup,
                    DecimationFactor,
                    PathToHDFExporter,
                    PathToOutputTimeSeries;

      public DateTime Start,
                      End;
   }

}
