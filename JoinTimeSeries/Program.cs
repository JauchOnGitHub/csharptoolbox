using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Core;
using Mohid.CommandArguments;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;
using Mohid.Files;

namespace JoinTimeSeries
{
   class Program
   {
      static void Main(string[] args)
      {
         string stepMessage = "";
         FileName outputFileName;

         try
         {
            stepMessage = "command line arguments loading.";
            CmdArgs cmdArgs = new CmdArgs(args);

            if (cmdArgs.HasParameter("cfg"))
            {
               stepMessage = "configuration file loading.";
               Config conf = new Config(cmdArgs.Parameters["cfg"]);
               conf.Load();          

               stepMessage = "configuration file parsing.";
               ConfigNode nodeList = conf.Root.ChildNodes.Find(delegate(ConfigNode node){ return node.Name == "timeseries.to.join"; });
               if (nodeList == null)               
                  throw new Exception("Block 'timeseries.to.join' is missing.");   
               outputFileName = conf.Root["output.file.name", "output.tsr"].AsFileName();
               
               TimeUnits timeUnits = (TimeUnits) Enum.Parse(typeof(TimeUnits), conf.Root["time.units", "seconds"].AsString(), true);

               List<FileName> list = new List<FileName>();               
               foreach (KeyValuePair<string, KeywordData> item in nodeList.NodeData)               
                  list.Add(item.Value.AsFileName());

               if (list.Count <= 1)
                  throw new Exception("Block 'timeseries.to.join' must contain at least 2 entries");

               stepMessage = "loading timeseries.";
               List<TimeSeries> timeSeries = new List<TimeSeries>();

               foreach(FileName ts in list)
               {
                  TimeSeries newTS = new TimeSeries();
                  newTS.Load(ts);
                  timeSeries.Add(newTS);
               }

               DateTime start = timeSeries[0].StartInstant;
               for (int i = 1; i < timeSeries.Count; i++)
               {
                  if (timeSeries[i].StartInstant < start)
                     start = timeSeries[i].StartInstant;
               }

               stepMessage = "creating output timeseries.";
               TimeSeries outTS = new TimeSeries();
               outTS.StartInstant = start;
               outTS.TimeUnits = timeUnits;

               foreach (Column col in timeSeries[0].Columns)
               {
                  Column newCol = new Column(col.ColumnType);
                  newCol.Header = col.Header;
                  outTS.AddColumn(newCol);
               }

               foreach (TimeSeries toJoin in timeSeries)
                  outTS.AddTimeSeries(toJoin);

               stepMessage = "saving output timeseries.";
               outTS.Save(outputFileName);

               Console.WriteLine("Process complete with success.");
            }
            else
            {
               Console.WriteLine("Parameter --cfg is missing.");
               Console.WriteLine("Execution aborted.");
               return;
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("An exception was raised while {0}", stepMessage);
            Console.WriteLine("Exception message: {0}", ex.Message);
         }
      }
   }
}
