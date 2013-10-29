using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

using Mohid.Core;
using Mohid.CommandArguments;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;
using Mohid.Files;
using Mohid.Databases;

namespace MohidARBVSDownloader
{
   class Program
   {
      class NodeData
      {
         public string DBColumn;
         public string NodeID;
         public List<MeteoAgriNode> Data;

         public NodeData()
         {
         }
      }

      class DataToRead
      {
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
         string dateFormat;
         string server = "http://www.meteoagri.com/";

         try
         {
            db = new Database();
            CmdArgs cmdArgs = new CmdArgs(args);
            List<DataToRead> list = new List<DataToRead>();
            int defaultDaysToDownload;

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
               dateFormat = conf.Root["date.format", "#yyyy-MM-dd#"].AsString();

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

                  item.TableName = node["table.name"].AsString();
                  item.DBDateColumn = node["db.date.column"].AsString();

                  List<ConfigNode> cfList = node.ChildNodes.FindAll(delegate(ConfigNode n) { return n.Name == "node"; });
                  if (cfList == null)
                     throw new Exception("No blocks 'node' were found on block number " + blockNumber.ToString());

                  foreach (ConfigNode cn in cfList)
                  {
                     nd = new NodeData();
                     nd.DBColumn = cn["column"].AsString();
                     nd.NodeID = cn["node.id"].AsString();
                     item.NodesList.Add(nd);
                  }

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

                  blockNumber++;
               }
            }
            else
               throw new Exception("No valid configuration file provided. use 'cfg' parameter.");

            MeteoAgriEngine maEng = new MeteoAgriEngine(server);
            maEng.TimeOut = 360;

            Console.Write("Connecting to MeteoAgri...");
            if (maEng.Login("arbvs", "arbvs"))
            {
               Console.WriteLine("[OK]");

               int count = 1;
               int nItems = 0;
               Console.WriteLine("DATA READ/WRITE in progress.");
               foreach (DataToRead dr in list)
               {
                  //First, find how many "slots" will be downloaded

                  int slots = (DateTime.Now - dr.Start).Days;
                  if (slots <= 0)
                  {
                     count++;
                     continue;
                     //throw new Exception("Start date for 'data.to.read' number " + count.ToString() + " (" + dr.Start.ToString("yyyy-MM-dd") + ") is same than today's date");
                  }

                  nItems = 0;
                  foreach (NodeData nd in dr.NodesList)
                  {
                     Console.Write("Reading from ARBVS server the node {0}, id = {1}...", nd.DBColumn, nd.NodeID);
                     nd.Data = maEng.GetData(nd.NodeID, dr.Start, slots);

                     if (nd.Data == null)
                     {
                        Console.WriteLine("[NO DATA]");
                        //throw new Exception(maEng.ExceptionMessage);                        
                     }
                     else if (nd.Data.Count < 1)
                     {
                        Console.WriteLine("[NO DATA]");
                        //throw new Exception("ARBVS server returned an empty node");
                     }
                     else
                     {
                        if (nItems < 1)
                           nItems = nd.Data[0].Data.Count;
                        else if (nItems != nd.Data[0].Data.Count)
                        {
                           Console.WriteLine("[ERROR]");
                           throw new Exception("ARBVS server returned a different number of items than the previous node");
                        }

                        Console.WriteLine("[OK]");

                        //Console.WriteLine("");
                        //Console.WriteLine("The data returned was:");
                        //foreach (MeteoAgriNode node in nodes)
                        //{
                        //   Console.WriteLine("NODE : {0}", node.NodeId);
                        //   foreach (MeteoAgriData data in node.Data)
                        //      Console.WriteLine(string.Format("    type:'{0}' d:'{1}' s:'{2}' t:'{3}' value:'{4}'",
                        //                                      data.Type, data.D, data.S, data.T, data.Value));
                        //   Console.WriteLine("------------");
                        //}
                     }
                  }

                  string fields, data;
                  if (nItems > 0)
                  {
                     Console.Write("Writing info on database...");
                     for (int i = 0; i < nItems; i++)
                     {
                        fields = "Instant";
                        data = dr.Start.AddDays(i).ToString(dateFormat);
                        foreach (NodeData nd in dr.NodesList)
                        {
                           fields += ", " + nd.DBColumn;
                           data += ", " + nd.Data[0].Data[i].Value;
                        }

                        query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", dr.TableName, fields, data);
                        //if (db.ExecuteCommand(query) != 1)
                        db.ExecuteCommand(query);
                     }
                     Console.WriteLine("[OK][{0} items inserted]", nItems);
                  }

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
