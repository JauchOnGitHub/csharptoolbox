using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mohid.Log;
using Mohid.Configuration;
using Mohid.Files;

namespace Mohid
{
   namespace MyFarm
   {
      public class MM52TS
      {
         #region Data

         protected LogEngine log;
         protected bool acceptableFailure;
         protected Exception exception;
         protected ConfigNode logNode;
         
         

         #endregion Data

         #region Properties

         public FilePath WorkPath; 
         public FilePath WindsPath;
         public DateTime GatheringInitialDate;
         public DateTime GatheringFinalDate;
         public Exception Exception { get { return exception; } }

         #endregion Properties

         #region Setup

         public MM52TS()
         {
            acceptableFailure = false;
            log = new LogEngine("get.mm5.2.timeseries.log");
            logNode = new ConfigNode(DateTime.Now.ToString("yyyy-MM-dd.HHmmss"));
            WorkPath = new FilePath(@".\");
            WindsPath = new FilePath(@".\");
         }

         public ~MM52TS()
         {
         }

         #endregion Setup

         #region Engine

         public bool Run()
         {
            bool result = true;

            exception = null;
            LogThis("Start", DateTime.Now.ToString("yyyy-MM-dd HHmmss"));

            try
            {               
               GatherMM5Files();
               ExtractTimeSeries(InitialDate, FinalDate, TimeSeriesConfigFile);
               DeleteMM5Files();
               CleanUpUnecessaryFiles();
               SuccessfullEnd()();
            }
            catch (Exception ex)
            {
               exception = ex;
               result = false;
            }

            LogThis("End", DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
            return result;
         }

         protected void GatherMM5Files()
         {
            try
            {
               string[] OldWindFiles = Directory.GetFiles(WorkPath.Path, "*.hdf5");
               foreach (string OldFilePath in OldWindFiles)            
                   File.Delete(OldFilePath);
               LogThis("Clearing HDF5 files", "OK");
            }
            catch(Exception ex)
            {
               LogThis("Clearing HDF5 files", "ERROR");
               throw;
            }

            try
            {
               FileName fn = new FileName();
               string[] sep = { " " };

               string[] DomainAllWindFiles = Directory.GetFiles(WindsPath.Path, "D3_*.hdf5");            
               foreach (string FullFilePath in DomainAllWindFiles)
               {
                  fn.FullPath = FullFilePath;   
                  string FullFileName = fn.FullName;
                  string name = FullFileName.Substring(0, FullFileName.Length - 5);

                  string[] StrBuffer = name.Split(sep, StringSplitOptions.None);

                  //Filename must have 3 parts separated by underscore: domain_initialdate_finaldate.hdf5
                  if (StrBuffer.Length == 3)
                  {
                     int Year, Month, Day, Hour;
                     int.TryParse(StrBuffer[1].Substring(0, 4), out Year);
                     int.TryParse(StrBuffer[1].Substring(4, 2), out Month);
                     int.TryParse(StrBuffer[1].Substring(6, 2), out Day);
                     int.TryParse(StrBuffer[1].Substring(8, 2), out Hour);

                     DateTime FileInitialDate = new DateTime(Year, Month, Day, Hour, 0, 0);

                     int.TryParse(StrBuffer[2].Substring(0, 4), out Year);
                     int.TryParse(StrBuffer[2].Substring(4, 2), out Month);
                     int.TryParse(StrBuffer[2].Substring(6, 2), out Day);
                     int.TryParse(StrBuffer[2].Substring(8, 2), out Hour);

                     DateTime FileFinalDate = new DateTime(Year, Month, Day, Hour, 0, 0);
                     DateTime CurrentDate = FileInitialDate;
                     bool FileOK = false;                                       

                     while (CurrentDate <= FileFinalDate)
                     {
                        if (CurrentDate >= GatheringInitialDate && CurrentDate <= GatheringFinalDate) 
                        {
                            FileOK = true;
                            break;
                        }

                        CurrentDate = CurrentDate.AddHours(1);
                     }

                     if (FileOK)
                     {
                        ConfigNode fileNode = new ConfigNode();
                        logNode.ChildNodes.Add(fileNode);                        
                        FileTools.CopyFile(fn, new FileName(fn.FullName), CopyOptions.OVERWRIGHT);
                        fileNode.NodeData.Add(name, new KeywordData("OK"));
                     }
                    
                  }                    
                  else                   
                  {
                     LogThis("Irregular file name. File not used: " + FullFileName)
                  }            
               }
            }
            catch(Exception ex)
            {
            }
         }

         #endregion Engine

         #region Management

         protected void LogThis(string key, string message)
         {
            Console.WriteLine(Message);
            logNode.NodeData.Add(key, new KeywordData(Message));        
         }

         #endregion Management
      }
   }
}
