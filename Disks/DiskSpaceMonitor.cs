using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
using Mohid.Log;
using Mohid.Files;

namespace Mohid
{
   namespace Disks
   {
      struct DSMLogData
      {
         public DateTime Date;
         public long FreeSpace;
         public double LostSpace;
         public int WarningType;
      }

      public class DiskSpaceMonitor
      {
         public string Disk; //Disk to monitorize
         public double MinimumSpace; //Minimum space to start the warnings
         public int AlertDays; //Number of days before end of disk space to start the warnings
         public FileName LogFile;
         public string DateFormat;
         public int ReadingsToAverage;

         protected LogEngine log;

         protected void Init()
         {
            Disk = @"C:\";
            MinimumSpace = 30.0; //30 GB
            AlertDays = 15;
            LogFile.FullName = @".\sm.log";
            log = new LogEngine();
            DateFormat = "dd-MM-yyyy";
            ReadingsToAverage = 7;
         }

         public DiskSpaceMonitor()
         {
            Init();
         }

         protected void CheckSpace()
         {
            log.Load();
            string [] sep = {";"};

            if (log.Count > 0)
            {
               DateTime now = DateTime.Today;
               DSMLogData last_data = LoadLogData(log.Parse(sep, log.LastIndex));
               DSMLogData data;

               double days = (double)((now - last_data.Date).Days);

               DriveInfo d = null;
               foreach (DriveInfo di in DriveInfo.GetDrives())
               {
                  if (di.IsReady && di.Name == Disk)
                  {
                     d = di;
                     break;
                  }
               }

               if (d == null)
                  throw new Exception("Drive '" + Disk + "' is not accessible");

               double lostSpace = ((double)(d.AvailableFreeSpace - last_data.FreeSpace)) / days;

               int count = Math.Min(ReadingsToAverage - 1, log.LastIndex);
               double sum = lostSpace;
               for (int i = count; i >= 0; i--)
               {
                  data = LoadLogData(log.Parse(sep, i));
                  sum += data.LostSpace;
               }

               sum /= (count + 2);

               long daysRemaining = (long)((double)d.AvailableFreeSpace / sum);

            }
            else
            {
               //For the case of first time check.
            }
         }

         private DSMLogData LoadLogData(string[] data)
         {
            DSMLogData dsmld;

            dsmld.Date = DateTime.Today;
            dsmld.FreeSpace = 0;
            dsmld.LostSpace = 0;
            dsmld.WarningType = 0;

            foreach (string item in data)
            {
               switch (item.ToLower())
               {
                  case "date":
                     dsmld.Date = DateTime.ParseExact(item, DateFormat, null);
                     break;
                  case "freespace":
                     dsmld.FreeSpace = long.Parse(item);
                     break;
                  case "lostspace":
                     dsmld.LostSpace = double.Parse(item);
                     break;
                  case "warningtype":
                     dsmld.WarningType = int.Parse(item);
                     break;
                  default:
                     break;
               }
            }

            return dsmld;
         }
      }
   }
}
*/