using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Simulation;
using Mohid.Core;
using Mohid.Log;
using Mohid.Software;
using Mohid.HDF;

namespace GenerateMeteoDataFromMM5
{
   public partial class Form1 : Form
   {
      protected List<string> filesToGlue;

      public Form1()
      {
         InitializeComponent();
         filesToGlue = new List<string>();
      }

      private void button1_Click(object sender, EventArgs e)
      {
         if (!GenerateMeteoFiles())
         {
            MessageBox.Show("Error!");
         }
      }

      protected bool GenerateMeteoFiles()
      {
         DateTime simStartDate = dateTimePicker1.Value,
                  simEndDate = dateTimePicker2.Value,
                  simStart = simStartDate.AddSeconds(1),
                  startsim,
                  endsim;
         string dateStr;
         int counter;
         string monthly_file, mm5NewFilesFolder;
         FileName file = new FileName();
         bool found_file = false;
         System.IO.FileInfo fi;
         Dictionary<string, KeywordData> meteoFolderList;

         filesToGlue.Clear();

         //--------------------------------------------------------------------------------------------------------------
         //1. Loads the list of folders to look for the meteo files.
         //--------------------------------------------------------------------------------------------------------------
         ConfigNode foldersListNode = root.ChildNodes.Find(FindFoldersListBlocks);
         if (foldersListNode == null)
         {
            //If the list of folders were not provided, looks for the meteo.folder keyword.
            //If the keyword is also missing, uses a default folder, that in this case will be 'H'
            meteoFolderList = new Dictionary<string, KeywordData>();
            meteoFolderList.Add("1", root["meteo.folder", @"H:\"]);
         }
         else
         {
            meteoFolderList = foldersListNode.NodeData;
         }
         //--------------------------------------------------------------------------------------------------------------

         Console.WriteLine("Sim start date   : {0}", simStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim enddate      : {0}", simEndDate.ToString("yyyy/MM/dd HH:mm:ss"));
         Console.WriteLine("Sim lenght (days): {0}", mre.sim.SimLenght);
         //Console.WriteLine("Generating Meteo file from {0} to {1}", startsim.ToString("yyyy/MM/dd HH:mm:ss"), mre.sim.End.ToString("yyyy/MM/dd HH:mm:ss"));

         bool skipInterpolation = root["skip.interpolation", false].AsBool();

         //This will ensure that the first hour of simulation is included in the meteo file.
         startsim = simStart.AddHours(-5);
         endsim = startsim.AddHours(5);

         //--------------------------------------------------------------------------------------------------------------
         //2. Find the required files. The meteo file must contain data for the entire simulation period
         //--------------------------------------------------------------------------------------------------------------
         bool isFirstInstant;

         Console.WriteLine("Folder where meteo files are: {0}", root["meteo.folder", @"H:\"].AsString());
         Console.WriteLine("");
         Console.WriteLine("Looking for files...");
         Console.WriteLine("");

         int maxSkips = root["max.skips", 10].AsInt();
         int numberOfSkips = 0;
         List<string> requiredFiles = new List<string>();

         Console.WriteLine("The folowing folders will be used on searching meteo files:");
         counter = 0;
         foreach (KeywordData folder in meteoFolderList.Values)
         {
            counter++;
            Console.WriteLine("  {0}. {1}", counter, folder.AsFilePath().Path);
         }
         Console.WriteLine("");

         isFirstInstant = true;
         file.FullPath = "";
         while (startsim < mre.sim.End || !found_file)
         {
            dateStr = startsim.ToString("yyyyMMddHH") + "_" + endsim.ToString("yyyyMMddHH");
            Console.WriteLine("Looking for {0}", @"D3_" + dateStr + ".hdf5");
            foreach (KeywordData folder in meteoFolderList.Values)
            {
               found_file = false;
               file.FullPath = folder.AsFilePath().Path + @"D3_" + dateStr + ".hdf5";

               if (System.IO.File.Exists(file.FullPath))
               {
                  fi = new System.IO.FileInfo(file.FullPath);
                  if (fi.Length > 2000000)
                  {
                     Console.WriteLine("  Found at {0}", folder.AsFilePath().Path);
                     if (!skipInterpolation)
                     {
                        try
                        {
                           Console.Write("  Interpolating file....");
                           if (!InterpolateMeteoFile(file))
                           {
                              Console.WriteLine("[failed]");
                              continue;
                           }
                           else
                              Console.WriteLine("[ok]");
                        }
                        catch (Exception ex)
                        {
                           Console.WriteLine("[exception]");
                           Console.WriteLine("  Message returned: {0}", ex.Message);
                           continue;
                        }
                     }
                     else
                     {
                        filesToGlue.Add(file.FullPath);
                     }

                     found_file = true;
                     break;
                  }
               }
            }

            ////try to find the file (the period) in the "monthly" files in mohid land
            //if (!found_file && mm5NewFilesFolder != "")
            //{
            //   pathToFilesToGlue = root["mm5.months.folder"].AsFilePath().Path +
            //                       startsim.ToString("yyyy") + @"\";
            //   monthly_file = "MM5-D3-Portugal-" + startsim.ToString("yyyy-MM") + ".hdf5";

            //   if (System.IO.File.Exists(pathToFilesToGlue + monthly_file))
            //   {
            //      //find block with info for extractor
            //      ConfigNode extractorNode = root.ChildNodes.Find(FindHDFExtractorBlocks);
            //      if (extractorNode != null)
            //      {
            //         ExternalApp extractorTool = new ExternalApp();

            //         replace_list.Clear();
            //         replace_list["<<input.hdf>>"] = pathToFilesToGlue + monthly_file;
            //         replace_list["<<output.hdf>>"] = mm5NewFilesFolder + @"D3_" + dateStr + ".hdf5";
            //         replace_list["<<start>>"] = startsim.ToString("yyyy MM dd HH 00 00");
            //         replace_list["<<end>>"] = endsim.ToString("yyyy MM dd HH 00 00");

            //         string template = extractorNode["template", mre.sim.SimDirectory.Path + @"templates\tools\extractor.template"].AsString();
            //         string action = extractorNode["config.file.name", "extractor.cfg"].AsString();

            //         Console.WriteLine("Extracting template {0}.", action);

            //         TextFile.Replace(template, extractorNode["working.folder", @"..\tools\extraction\"].AsFilePath().Path + action, ref replace_list);

            //         extractorTool.WorkingDirectory = extractorNode["working.folder", @"..\tools\extraction\"].AsFilePath().Path;
            //         extractorTool.Executable = root["extractor.exe", @"..\tools\extraction\extractor.exe"].AsString();
            //         extractorTool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            //         extractorTool.TextToCheck = "successfully terminated";
            //         extractorTool.SearchTextOrder = SearchTextOrder.FROMEND;
            //         extractorTool.Wait = true;

            //         bool result = extractorTool.Run();
            //         if (!result)
            //         {
            //            Console.WriteLine("Extraction failed.");
            //         }
            //         else
            //         {
            //            requiredFiles.Add(mm5NewFilesFolder + file);
            //            found_file = true;
            //         }
            //      }
            //   }
            //}

            if (!found_file)
            {
               Console.WriteLine("  Not found!", file);

               if ((++numberOfSkips) > maxSkips)
               {
                  Console.WriteLine("Max number of skips reached during meteo creation file.");
                  return false;
               }

               if (isFirstInstant) //first file in the list
               {
                  Console.WriteLine("Going backward...");
                  endsim = startsim.AddHours(-1);
                  startsim = startsim.AddHours(-6);

                  continue;
               }
               else
               {
                  Console.WriteLine("Skipping for the next file...");
                  startsim = endsim.AddHours(1);
                  endsim = startsim.AddHours(5);
               }
            }
            else if (isFirstInstant && numberOfSkips > 0)
            {
               numberOfSkips = 0;
               counter = 0;

               startsim = simStart.AddHours(1);
               endsim = startsim.AddHours(5);
            }
            else if (startsim > mre.sim.End)
            {
               numberOfSkips = 0;
               break;
            }
            else
            {
               numberOfSkips = 0;
               startsim = endsim.AddHours(1);
               endsim = startsim.AddHours(5);
            }

            if (isFirstInstant && found_file)
               isFirstInstant = false;
         }
         //--------------------------------------------------------------------------------------------------------------

         //Now the interpolated files are glued
         HDFGlue glueTool = new HDFGlue(filesToGlue);
         glueTool.AppName = root["glue.exe.name", "glue.exe"].AsString();
         glueTool.AppPath = root["glue.exe.path", @"..\tools\glue\"].AsString();
         glueTool.WorkingDirectory = root["glue.working.folder", @"..\tools\glue\"].AsString();
         glueTool.Output = outputPath + outputName;
         glueTool.Is3DFile = false;
         glueTool.ThrowExceptionOnError = true;

         Console.WriteLine("Gluing files...");

         if (glueTool.Glue() != 0)
         {
            Console.WriteLine("Glue failed.");
            return false;
         }

         //Delete all the files in the temp directory
         try
         {
            System.IO.DirectoryInfo directory = new DirectoryInfo(@"..\tools\interpolation\temp\");
            foreach (System.IO.FileInfo fileToDelete in directory.GetFiles())
               fileToDelete.Delete();
         }
         catch (Exception ex)
         {
            Console.WriteLine("Was not possible to empty the temp directory due to an exception.");
            Console.WriteLine("Message returned was: {0}", ex.Message);
         }

         //Saves the created meteo file in the folder
         if (!FileTools.CopyFile(root["local.boundary.conditions"].AsFilePath(), generalBoundary, "meteo.hdf5", Files.CopyOptions.OVERWRIGHT))
         {
            Console.WriteLine("Was not possible to copy the meteo file to the meteo storage folder.");
            return false;
         }

         return true;
      }

      protected bool InterpolateMeteoFile(FileName file)
      {
         try
         {
            replace_list.Clear();
            replace_list["<<input.hdf>>"] = file.FullPath;
            replace_list["<<father.grid>>"] = root["father.grid", @"..\..\general.data\digital.terrain\mm5.dat"].AsString();
            replace_list["<<output.hdf>>"] = tempFolder + file.FullName;
            replace_list["<<model.grid>>"] = root["model.grid", @"..\..\general.data\digital.terrain\grid.dat"].AsString();

            string template = root["template", mre.sim.SimDirectory.Path + @"templates\tools\convert.to.HDF5.action.template"].AsString();
            string action = root["action.file", @"..\tools\interpolation\converttoHDF5action.dat"].AsString();

            TextFile.Replace(template, action, ref replace_list);

            tool.Arguments = "";
            tool.WorkingDirectory = root["interpolation.working.folder", @"..\tools\interpolation\"].AsString();
            tool.Executable = root["interpolation.exe", @"..\tools\interpolation\interpolation.exe"].AsString();
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "successfully terminated";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            bool result = tool.Run();
            if (!result)
            {
               return false;
            }

            filesToGlue.Add(pathToFilesToGlue + file.FullName);
         }
         catch (Exception ex)
         {
            Console.WriteLine("An exception has happened when interpolating file '{0}'", file.FullName);
            Console.WriteLine("The exception was: {0}.", ex.Message);
            return false;
         }
         return true;
      }
   }
}
