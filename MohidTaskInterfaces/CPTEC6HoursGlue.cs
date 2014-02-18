using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Mohid;
using Mohid.Script;
using Mohid.Configuration;
using Mohid.HDF;

namespace MohidTaskInterfaces
{
   public class CPTEC6HoursGlue : IMohidTask
   {
      protected Exception fLastException;
      protected int fHoursToAdd;

      //User input
      protected DateTime fStartDate;
      protected DateTime fEndDate;
      protected string fCPTECFilesPath;
      protected string fOutputPath;
      protected int fIntervalToUse; //0: 0-12, 1: 12-24, 2: 24-36, 3: 36-48, 4: 48-60, 5: 60-72
      protected string fCPTECFileNameTag;
      protected string fOutputTag;
      protected string fGlueExe;
      protected string fGlueExePath;
      protected string fGlueWorkingFolder;

      public CPTEC6HoursGlue()
      {
         Reset();
      }

      public bool Run(ConfigNode cfg)
      {
         LoadCfg(cfg);
         if (!CheckUserInput())
            return false;

         return GlueFiles();
      }

      public void LoadCfg(ConfigNode cfg)
      {
         fStartDate = cfg["start.date"].AsDateTime("yyyy M d H m s");
         fEndDate = cfg["end.date"].AsDateTime("yyyy M d H m s");
         fCPTECFilesPath = cfg["path.to.cptec", fCPTECFilesPath].AsString();
         fOutputPath = cfg["output.path", fOutputPath].AsString();
         fIntervalToUse = cfg["interval.to.use", fIntervalToUse].AsInt();
         fOutputTag = cfg["output.tag", fOutputTag].AsString();
         fGlueExe = cfg["glue.exe", fGlueExe].AsString();
         fGlueExePath = cfg["glue.exe.path", fGlueExePath].AsString();
         fGlueWorkingFolder = cfg["glue.exe.working.folder", fGlueWorkingFolder].AsString();

         SelectHoursToAdd(fIntervalToUse);
      }

      public bool CheckUserInput()
      {
         if (fStartDate.Hour != 0 || fStartDate.Hour != 12)
            return false;
         if (fStartDate.Minute != 0)
            return false;
         if (fStartDate.Minute != 0)
            return false;

         if (fEndDate < fStartDate)
            return false;

         if (fEndDate.Hour != 0 || fEndDate.Hour != 12)
            return false;
         if (fEndDate.Minute != 0)
            return false;
         if (fEndDate.Second != 0)
            return false;

         if (!Directory.Exists(fCPTECFilesPath))
            return false;
         if (!fCPTECFilesPath.EndsWith("\\"))
            fCPTECFilesPath += "\\";

         if (!Directory.Exists(fOutputPath))
            return false;
         if (!fOutputPath.EndsWith("\\"))
            fOutputPath += "\\";
         
         if (fIntervalToUse < 0 || fIntervalToUse > 5)
            return false;

         if (string.IsNullOrWhiteSpace(fCPTECFileNameTag))
            return false;

         if (string.IsNullOrWhiteSpace(fOutputTag))
            return false;

         if (!Directory.Exists(fGlueExePath))
            return false;
         if (!fGlueExePath.EndsWith("\\"))
            fOutputPath += "\\";

         if (!Directory.Exists(fGlueWorkingFolder))
            return false;
         if (!fGlueWorkingFolder.EndsWith("\\"))
            fOutputPath += "\\";

         if (File.Exists(fGlueExePath + fGlueExe))
            return false;

         return true;
      }

      public void SelectHoursToAdd(int intervalToUse)
      {
         switch (intervalToUse)
         {
            case 0: //1-12h
               fHoursToAdd = 1;
               break;
            case 1: //13-24h
               fHoursToAdd = 13;
               break;
            case 2: //25-36h
               fHoursToAdd = 25;
               break;
            case 3: //37-48h
               fHoursToAdd = 37;
               break;
            case 4: //49-60h
               fHoursToAdd = 49;
               break;
            case 5: //61-72h
               fHoursToAdd = 61;
               break;
            default:
               throw new Exception("Invalid 'Interval To Use' code");
         }
      }

      public bool GlueFiles()
      {
         DateTime date;
         int i;
         List<string> filesToGlue = new List<string>(6);
         string file;
         DateTime start = DateTime.Now, end = DateTime.Now;

         for (date = fStartDate; date <= fEndDate; date.AddHours(12))
         {
            filesToGlue.Clear();
            for (i = 0; i < 6; i++)
            {
               file = fCPTECFilesPath + fCPTECFileNameTag + date.ToString("yyyyMMddHH") + "+" + date.AddHours(fHoursToAdd + i);
               filesToGlue.Add(file);
               if (i == 0)
                  start = date.AddHours(fHoursToAdd + i);
               if (i == 5)
                  end = date.AddHours(fHoursToAdd + i);
            }

            if (CheckList(filesToGlue))
               Glue(start, end, filesToGlue);

            filesToGlue.Clear();
            for (i = 6; i < 12; i++)
            {
               file = fCPTECFilesPath + fCPTECFileNameTag + date.ToString("yyyyMMddHH") + "+" + date.AddHours(fHoursToAdd + i);
               filesToGlue.Add(file);
               if (i == 6)
                  start = date.AddHours(fHoursToAdd + i);
               if (i == 11)
                  end = date.AddHours(fHoursToAdd + i);
            }
            if (CheckList(filesToGlue))
               Glue(start, end, filesToGlue);
         }

         return true;
      }

      protected bool CheckList(List<string> list)
      {
         foreach (string file in list)
         {
            if (!File.Exists(file))
               return false;
         }

         return true;
      }

      protected bool Glue(DateTime start, DateTime end, List<string> list)
      {
         try
         {
            HDFGlue tool_glue = new HDFGlue();

            tool_glue.Reset();

            tool_glue.FilesToGlue = list;

            tool_glue.AppName = fGlueExe;
            tool_glue.AppPath = fGlueExePath;
            tool_glue.WorkingDirectory = fGlueWorkingFolder;
            tool_glue.Output = fOutputPath + fOutputTag + start.ToString("yyyyMMddHH") + "_" + end.ToString("yyyyMMddHH") + ".hdf5";
            tool_glue.Is3DFile = false;
            tool_glue.ThrowExceptionOnError = true;

            tool_glue.Glue();
         }
         catch (Exception ex)
         {
            fLastException = ex;
            return false;
         }

         return true;
      }

      public Exception LastException
      {
         get { return fLastException; }
      }

      public bool Reset()
      {
         fLastException = null;
         fStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
         fCPTECFilesPath = @".\data\";
         fOutputPath = @".\output\";
         fIntervalToUse = 0;
         fCPTECFileNameTag = "";
         fOutputTag = "cptec_";
         fGlueExe = "glue.exe";
         fGlueExePath = @".\glue\";
         fGlueWorkingFolder = fGlueExePath;

         return true;
      }
   }
}
