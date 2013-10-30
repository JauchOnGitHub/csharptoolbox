using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Globalization;

using Mohid.Core;
using Mohid.CommandArguments;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;
using Mohid.Files;
using Mohid.Databases;
using System.Threading;

namespace MohidARBVSDownloader
{
   class Program
   {
      class NodeData
      {
         public string DBColumn;
         public string TSHeader;
         public string NodeID;
         public string DefaultValue;
         public List<MeteoAgriNode> Data;

         public NodeData()
         {
         }
      }

      class DataToRead
      {
         public string TimeSeriesFileName;
         public string TableName;
         public string DBDateColumn;
         public List<NodeData> NodesList;
         public DateTime Start;         

         public DataToRead()
         {
            NodesList = new List<NodeData>();
         }
      }

      static void Main(string[] args)
      {
         Database db = null;
         string query;
         OdbcDataReader dbReader;
         int interval_seconds_guess;
         string dateFormat;
         string server = "http://www.meteoagri.com/";

         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

         try
         {
            db = new Database();
            CmdArgs cmdArgs = new CmdArgs(args);
            List<DataToRead> list = new List<DataToRead>();
            int defaultDaysToDownload;

            string user, pass;

            bool save_ts = cmdArgs.HasOption("save_ts");
            bool save_db = cmdArgs.HasOption("save_db");

            if (!cmdArgs.HasParameter("user"))
               throw new Exception("Must provide the user. Uses '--user user_name'.");

            if (!cmdArgs.HasParameter("pass"))
               throw new Exception("Must provide the pass. Uses '--pass password'.");

            user = cmdArgs.Parameters["user"];
            pass = cmdArgs.Parameters["pass"];

            if (cmdArgs.HasParameter("cfg"))
            {
               Config conf = new Config(cmdArgs.Parameters["cfg"]);
               conf.Load();

               ConfigNode dbConf = conf.Root.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "database.config"; });
               if (dbConf == null)
                  throw new Exception("No 'database.config' block was found.");

               if (db.Connect(dbConf["db.conn.string"].AsString()) != Result.TRUE)
                  throw new Exception("Connection to database was not possible. The message returned was: " + db.RaisedException.Message);

               server = conf.Root["server", "http://www.meteoagri.com/"].AsString();
               defaultDaysToDownload = conf.Root["days.to.download", 14].AsInt();
               dateFormat = conf.Root["date.format", "#yyyy-MM-dd HH:mm:ss#"].AsString();
               interval_seconds_guess = conf.Root["interval.guess", 900].AsInt();

               List<ConfigNode> nodeList = conf.Root.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "data.to.read"; });
               if (nodeList == null)
                  throw new Exception("No blocks 'nodes.to.read' were found.");

               DataToRead item;
               NodeData nd;
               int blockNumber = 1;
               foreach (ConfigNode node in nodeList)
               {
                  item = new DataToRead();
                  list.Add(item);

                  item.TimeSeriesFileName = node["time.series.file.name", ""].AsString();
                  item.TableName    = node["table.name"].AsString(); 
                  item.DBDateColumn = node["db.date.column"].AsString();

                  List<ConfigNode> cfList = node.ChildNodes.FindAll(delegate(ConfigNode n) { return n.Name == "node"; });
                  if (cfList == null)
                     throw new Exception("No blocks 'node' were found on block number " + blockNumber.ToString());

                  foreach (ConfigNode cn in cfList)
                  {
                     nd = new NodeData();
                     nd.DefaultValue = cn["default.value", ""].AsString();
                     nd.DBColumn = cn["column"].AsString();
                     nd.TSHeader = cn["ts.header", nd.DBColumn].AsString();
                     nd.NodeID   = cn["node.id"].AsString();
                     item.NodesList.Add(nd);
                  }

                  if (save_db)
                  {
                     query = string.Format("SELECT {0} FROM {1} ORDER BY {0} DESC",
                                           item.DBDateColumn, item.TableName);

                     dbReader = db.ExecuteQuerySingleRow(query);

                     item.Start = DateTime.Now.AddDays(-defaultDaysToDownload);

                     if (dbReader.HasRows)
                     {
                        while (dbReader.Read())
                           item.Start = dbReader.GetDateTime(dbReader.GetOrdinal(item.DBDateColumn)).AddDays(1);

                        if (!dbReader.IsClosed)
                           dbReader.Close();
                     }
                  }
                  else
                  {
                     item.Start = DateTime.Now.AddDays(-defaultDaysToDownload);
                  }
                  blockNumber++;
               }
            }
            else
               throw new Exception("No valid configuration file provided. use 'cfg' parameter.");

            MeteoAgriEngine maEng = new MeteoAgriEngine(server);
            maEng.TimeOut = 360;

            Console.Write("Connecting to MeteoAgri...");
            if (maEng.Login(user, pass))
            {
               Console.WriteLine("[OK]");

               int count = 1;
               int nItems = 0;
               Console.WriteLine("DATA READ/WRITE in progress.");
               foreach (DataToRead dr in list)
               {
                  TimeSeries ts = new TimeSeries();
                  ts.StartInstant = dr.Start;
                  ts.TimeUnits = TimeUnits.SECONDS;
                  
                  foreach (NodeData nd in dr.NodesList)
                  {
                     Column col = new Column(typeof(string), 0, nd.TSHeader);
                     col.DefaultValue = "0";
                     ts.AddColumn(col);
                  }

                  //First, find how many "slots" will be downloaded

                  int slots = (int)(DateTime.Now - dr.Start).TotalSeconds / interval_seconds_guess;
                  if (slots <= 0)
                  {
                     count++;
                     continue;
                     //throw new Exception("Start date for 'data.to.read' number " + count.ToString() + " (" + dr.Start.ToString("yyyy-MM-dd") + ") is same than today's date");
                  }

                  nItems = 0;
                  int column = -1;
                  foreach (NodeData nd in dr.NodesList)
                  {
                     column++;
                     Console.WriteLine("Reading from ARBVS server the node {0}, id = {1}...", nd.DBColumn, nd.NodeID);
                     maEng.GetData(nd.NodeID, dr.Start, slots, ts, column, dateFormat);

                     //if (nd.Data == null)
                     //{
                     //   Console.WriteLine("[NO DATA]");
                     //   //throw new Exception(maEng.ExceptionMessage);                        
                     //}
                     //else if (nd.Data.Count < 1)
                     //{
                     //   Console.WriteLine("[NO DATA]");
                     //   //throw new Exception("ARBVS server returned an empty node");
                     //}
                     //else
                     //{
                     //   if (nItems < 1)
                     //      nItems = nd.Data[0].Data.Count;
                     //   else if (nItems != nd.Data[0].Data.Count)
                     //   {
                     //      Console.WriteLine("[ERROR]");
                     //      throw new Exception("ARBVS server returned a different number of items than the previous node");
                     //   }

                     //   Console.WriteLine("[OK]");

                     //   //Console.WriteLine("");
                     //   //Console.WriteLine("The data returned was:");
                     //   //foreach (MeteoAgriNode node in nodes)
                     //   //{
                     //   //   Console.WriteLine("NODE : {0}", node.NodeId);
                     //   //   foreach (MeteoAgriData data in node.Data)
                     //   //      Console.WriteLine(string.Format("    type:'{0}' d:'{1}' s:'{2}' t:'{3}' value:'{4}'",
                     //   //                                      data.Type, data.D, data.S, data.T, data.Value));
                     //   //   Console.WriteLine("------------");
                     //   //}
                     //}
                  }

                  string fields, data;
                  //if (nItems > 0)
                  //{
                     //DateTime start = DateTime.Now;
                     //bool start_defined = false;
                  if (save_db)
                  {
                     Console.Write("Writing info on database...");
                     for (int i = 0; i < ts.NumberOfInstants; i++)
                     {
                        fields = "Instant";
                        data = ts.InstantAsDateTime(i).ToString(dateFormat);

                        //if (dr.NodesList[0].Data[0].Data[i].D == "-1")
                        //{
                        //   start = DateTime.ParseExact(dr.NodesList[0].Data[0].Data[i].T, "yyyyMMddTHH:mm:ss", null);
                        //   data = start.ToString(dateFormat);
                        //   start_defined = true;
                        //}
                        //else
                        //{
                        //   if (!start_defined)
                        //      throw new Exception("The start instant was not found in the imported data.");
                        //   start = start.AddSeconds(double.Parse(dr.NodesList[0].Data[0].Data[i].T));
                        //   data = start.ToString(dateFormat);
                        //}

                        int column_index = -1;
                        foreach (NodeData nd in dr.NodesList)
                        {
                           column_index++;
                           fields += ", " + nd.DBColumn;
                           data += ", " + (ts[column_index, i] as string);
                        }

                        query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", dr.TableName, fields, data);
                        //if (db.ExecuteCommand(query) != 1)
                        db.ExecuteCommand(query);
                     }
                     Console.WriteLine("[OK][{0} items inserted]", ts.NumberOfInstants.ToString());
                     //}
                  }

                  if (save_ts)
                     ts.Save(new FileName(dr.TimeSeriesFileName));

                  count++;
               }

               Console.Write("Logging out...");
               if (maEng.Logout())
                  Console.WriteLine("[OK]");
               else
               {
                  Console.WriteLine("[ERROR]");
                  Console.WriteLine("The message returned was:");
                  Console.WriteLine(maEng.ExceptionMessage);
               }

            }
            else
            {
               Console.WriteLine("[ERROR]");
               Console.WriteLine("The message returned was:");
               Console.WriteLine(maEng.ExceptionMessage);
            }

            db.Disconnect();
         }
         catch (Exception ex)
         {
            if (db != null) db.Disconnect();
            Console.WriteLine("");
            Console.WriteLine("An exception has happened. The message returned was:");
            Console.WriteLine(ex.Message);

         } 

         //Console.WriteLine("");
         //Console.WriteLine("Press any key...");
         //Console.ReadKey();
      }
   }
}
