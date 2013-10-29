using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.IO;

using Mohid.MohidTimeSeries;

namespace MohidARBVSDownloader
{
   public class MeteoAgriData
   {
      public string Type,
                    D,
                    S,
                    T,
                    Value;
   }

   public class MeteoAgriNode
   {
      public string NodeId { get; set; }
      public List<MeteoAgriData> Data;    

      public MeteoAgriNode()
      {
         Data = new List<MeteoAgriData>();
         NodeId = "";
      }

      public MeteoAgriNode(string nodeId)
      {
         Data = new List<MeteoAgriData>();
         NodeId = nodeId;
      }
   }

   public class MeteoAgriEngine
   {
      public string Server { get; set; }
      public int TimeOut { get; set; }
      public string Version { get; set; }
      public string Mode { get; set; }
      public bool Verbose { get; set; }
      public string ExceptionMessage { get { return exceptionMessage; } }
      public string DateFormat { get; set; }
      public string Cache { get; set; }

      protected string exceptionMessage;
      protected string sessionId;
      protected WebClient wc;

      public MeteoAgriEngine(string server = "http://www.meteoagri.com/")
      {
         Server = server;
         TimeOut = 3600;
         Version = "1.2";
         Mode = "t";
         Verbose = false;
         DateFormat = "iso8601";
         Cache = "y";

         sessionId = "";
         wc = new WebClient();
      }

      public bool Login(string user, string pass)
      {
         try
         {
            string address = string.Format(Server + "addUPI?function=login&user={0}&passwd={1}&timeout={2}&mode={3}&version={4}",
                                           Uri.EscapeDataString(user),
                                           Uri.EscapeDataString(pass),
                                           Uri.EscapeDataString(TimeOut.ToString()),
                                           Uri.EscapeDataString(Mode),
                                           Uri.EscapeDataString(Version));

            string result = wc.DownloadString(address);            

            XPathDocument doc = new XPathDocument(new StringReader(result));
            XPathNavigator nav = doc.CreateNavigator();

            nav.MoveToRoot();
            nav.MoveToFirstChild();

            if (nav.LocalName != "response") throw new Exception("Expecting 'response' but found '" + nav.LocalName);
            
            nav.MoveToFirstChild();
            if (nav.LocalName != "result") throw new Exception("Expecting 'result' but found '" + nav.LocalName);

            nav.MoveToFirstChild();
            if (nav.LocalName != "string") throw new Exception("Expecting 'string' but found '" + nav.LocalName);

            sessionId = nav.Value;

            return true;
         }
         catch (Exception ex)
         {
            exceptionMessage = ex.Message;

            if (Verbose)
            {
               Console.WriteLine("");
               Console.WriteLine("An exception has rised during Login. The message returned was:");
               Console.WriteLine(ex.Message);
               Console.WriteLine("");
            }
            return false;
         }
      }

      public bool Logout()
      {
         try
         {
            string address = string.Format(Server + "addUPI?function=logout&session-id={0}&mode={1}",
                                           Uri.EscapeDataString(sessionId),
                                           Uri.EscapeDataString(Mode));

            string result = wc.DownloadString(address);

            XPathDocument doc = new XPathDocument(new StringReader(result));
            XPathNavigator nav = doc.CreateNavigator();

            nav.MoveToRoot();
            nav.MoveToFirstChild();

            if (nav.LocalName != "response") throw new Exception("Expecting 'response' but found '" + nav.LocalName);

            nav.MoveToFirstChild();
            if (nav.LocalName != "result") throw new Exception("Expecting 'result' but found '" + nav.LocalName);

            if (!nav.IsEmptyElement) throw new Exception("Expecting an empty 'result' element but found '" + nav.Value);


            return true;
         }
         catch (Exception ex)
         {
            exceptionMessage = ex.Message;

            if (Verbose)
            {
               Console.WriteLine("");
               Console.WriteLine("An exception has rised during Logout. The message returned was:");
               Console.WriteLine(ex.Message);
               Console.WriteLine("");
            }
            return false;
         }
      }

      public void GetData(string parameterId, DateTime start, int slots, TimeSeries ts, int col_index, string dateFormat)
      {
         try
         {
            string address = string.Format(Server + "addUPI?function=getdata&session-id={0}&id={1}&df={2}&date={3}&slots={4}&cache={5}&mode={6}",
                                           Uri.EscapeDataString(sessionId),
                                           Uri.EscapeDataString(parameterId),
                                           Uri.EscapeDataString(DateFormat),
                                           Uri.EscapeDataString(start.ToString("yyyyMMddTHH:mm:ss")),                                           
                                           Uri.EscapeDataString(slots.ToString()),
                                           Uri.EscapeDataString(Cache),
                                           Uri.EscapeDataString(Mode));

            string result = wc.DownloadString(address);

            XPathDocument doc = new XPathDocument(new StringReader(result));
            XPathNavigator error, nav = doc.CreateNavigator();

            nav.MoveToRoot();
            nav.MoveToFirstChild();
            if (nav.LocalName != "response") throw new Exception("Expecting 'response' but found '" + nav.LocalName);
            
            XPathNodeIterator nodeRows, ni = nav.SelectChildren("node", "");
            if (ni.Count < 1)
            {
               string addUPIError = "";
               ni = nav.SelectChildren("error", "");
               if (ni.Count > 0)
               {
                  ni.MoveNext();
                  error = ni.Current;
                  if (error.HasAttributes)
                  {
                     if (error.MoveToAttribute("msg", ""))
                        addUPIError = error.Value;
                  }
               }

               throw new Exception(addUPIError);
            }

            //List<MeteoAgriNode> nodesList = new List<MeteoAgriNode>();
            MeteoAgriData newData = new MeteoAgriData();
            DateTime instant = DateTime.Now;
            string instant_str;
            bool start_defined = false;

            XPathNavigator row, node;
            while (ni.MoveNext())
            {
               node = ni.Current;
               nodeRows = node.SelectChildren("v", "");

               string nodeId = "";
               if (node.HasAttributes)
               {
                  nodeId = node.GetAttribute("id", "");
               }

               if (nodeRows.Count < 1)
               {
                  throw new Exception("No values were found for 'parameter id' = " + parameterId + " [nodeId='" + nodeId + "']");
               }

               //MeteoAgriNode newNode = new MeteoAgriNode(nodeId);
               //nodesList.Add(newNode);                              

               while (nodeRows.MoveNext())
               {
                  //MeteoAgriData newData = new MeteoAgriData(); 
                 
                  row = nodeRows.Current;
                  newData.Value = row.Value;
                  if (row.HasAttributes)
                  {
                     newData.Type = row.GetAttribute("type", "");
                     newData.D = row.GetAttribute("d", "");
                     newData.S = row.GetAttribute("s", "");
                     newData.T = row.GetAttribute("t", "");
                  }

                  int instant_index = -1;

                  if (!start_defined)
                  {
                     instant = DateTime.ParseExact(newData.T, "yyyyMMddTHH:mm:ss", null);
                     instant_str = start.ToString(dateFormat);
                     start_defined = true;

                     if (ts.StartInstant > instant)
                     {
                        ts.StartInstant = instant;
                        instant_index = ts.AddInstant(instant);
                     }
                     else if (ts.StartInstant != instant)
                     {
                        instant_index = ts.Index(instant, Mohid.Core.SearchType.FIND_EXACTLY);
                        if (instant_index == -1)
                           instant_index = ts.AddInstant(instant);
                     }
                     else
                     {
                        instant_index = 0;
                     }
                     
                  }
                  else
                  {
                     if (!start_defined)
                        throw new Exception("The start instant was not found in the imported data.");

                     instant = instant.AddSeconds(double.Parse(newData.T));
                     instant_str = instant.ToString(dateFormat);

                     instant_index = ts.Index(instant, Mohid.Core.SearchType.FIND_EXACTLY);
                     if (instant_index == -1)
                        instant_index = ts.AddInstant(instant);
                  }

                  ts[col_index, instant_index] = newData.Value;
                  //newNode.Data.Add(newData);
               }               
            }
            
            //return nodesList;
         }
         catch (Exception ex)
         {
            exceptionMessage = ex.Message;

            if (Verbose)
            {
               Console.WriteLine("");
               Console.WriteLine("An exception has rised during data recovery. The message returned was:");
               Console.WriteLine(ex.Message);
               Console.WriteLine("");
            }
         }
      }

   }
}
