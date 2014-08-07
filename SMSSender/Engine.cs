using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Data.Odbc;
using Mohid;
using Mohid.Files;
using Mohid.SMS;
using Mohid.Databases;
using Mohid.Configuration;

namespace SMSSender
{
   public class EngineData
   {
      public string Pin,                    
                    SMSCenter,
                    DBPath,
                    DBName,
                    ConnectionString;
      public int Tries,
                 TimeOut,
                 WaitTime,
                 IntervalBetweenMessages;

      public System.IO.Ports.SerialPort Port;        
   }

   public class Engine
   {
      protected Config config;
      protected bool config_loaded;
      protected ConfigNode config_root;
      protected EngineData data;
      protected SMSEngine sms_engine;
      protected Database database;
      protected StringBuilder errors;

      public bool Debug { get; set; }
      public bool Verbose { get; set; }

      public bool HasErrors
      {
         get { if (errors.Length > 0) return true; return false; }
      }
      public string Errors
      {
         get { return errors.ToString(); }
      }

      public Engine()
      {
         config_loaded = false;
         config_root = null;
         data = new EngineData();
         sms_engine = null;
         database = null;
         errors = new StringBuilder();
         Debug = false;
      }

      public bool LoadConfig(FileName config_file)
      {

         try
         {
            config_loaded = false;

            config = new Config(config_file.FullPath);
            config.Load();

            config_root = config.Root;

            //Save here the data that will be used across the class
            data.Pin = config_root["pin_code", ""].AsString();
            data.SMSCenter = config_root["sms_center"].AsString();
            data.TimeOut = config_root["timeout", 10].AsInt() * 1000;
            data.WaitTime = config_root["wait", 10].AsInt() * 1000;
            data.Tries = config_root["tentatives", 2].AsInt();            

            data.Port = SMSEngine.GetNewPort();
            ConfigNode port = config_root.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "port"; });
            if (port == null)
               throw new Exception("Block \"port\" not found");

            data.Port.WriteTimeout = port["write_timeout", 10].AsInt() * 1000;
            data.Port.ReadTimeout = port["read_timeout", 10].AsInt() * 1000;
            data.Port.PortName = port["port_name"].AsString();
            data.Port.BaudRate = port["baudrate", 9600].AsInt();
            data.Port.DataBits = port["databits", 8].AsInt();
            data.Port.Parity = (Parity) Enum.Parse(typeof(Parity), port["parity", "None"].AsString(), true);
            data.Port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), port["stopbits", "One"].AsString(), true);
            data.Port.Handshake = (Handshake)Enum.Parse(typeof(Handshake), port["handshake", "RequestToSend"].AsString(), true);
            data.Port.DtrEnable = port["dtr_enabled", true].AsBool();
            data.Port.RtsEnable = port["rts_enabled", true].AsBool();
            data.Port.NewLine = Environment.NewLine;

            ConfigNode db = config_root.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "database"; });
            if (db == null)
               throw new Exception("Block \"database\" not found");

            if (db.NodeData.ContainsKey("connection_string"))
            {
               data.DBName = "";
               data.DBPath = "";
               data.ConnectionString = db["connection_string"].AsString();
            }
            else
            {
               data.DBName = db["name"].AsString();
               data.DBPath = db["path"].AsFilePath().Path;
               data.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                       data.DBPath + data.DBName +
                                       ";Persist Security Info=False";
            }

            config_loaded = true;
            return true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.LoadConfig Exception: {0}", ex.Message);

            if (config_root != null)
               config_root = null;

            return false;
         }
      }

      public bool SendMessages(DateTime date_time_to_send)
      {
         bool result;

         try
         {
            if (config_loaded != true)
               throw new Exception("No config loaded");

            if (!SetupModem())
               throw new Exception("Modem setup failure");

            if (!OpenDatabase())
               throw new Exception("Open database failure");

            if (!SendMessagesFromDatabase(date_time_to_send))
               throw new Exception("One or more messages failed");            

            result = true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.SendMessages Exception: {0}", ex.Message);      
            result = false;
         }

         if (data.Port.IsOpen)
            data.Port.Close();

         CloseDatabase();

         return result;
      }

      protected bool SetupModem()
      {
         try
         {
            sms_engine = new SMSEngine(data.Port);
            sms_engine.Debug = Debug;
            
            sms_engine.SMSCenter = data.SMSCenter;
            sms_engine.TimeOut = data.TimeOut;            
            sms_engine.PinCode = data.Pin;
            sms_engine.WaitTime = data.WaitTime;
            sms_engine.IntervalBetweenMessages = data.IntervalBetweenMessages;            

            return true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.SetupModem Exception: {0}", ex.Message);
            return false;
         }
      }

      protected bool SendMessagesFromDatabase(DateTime date_time_to_send)
      {
         try
         {
            int try_counter = 1;
            Mohid.Core.Result r;

            OdbcDataReader toSend = FindSMSToSend(date_time_to_send);

            if (toSend == null)
               return false;

            while (toSend.Read())
            {              
               if (!MessageWasAlreadySent(toSend, date_time_to_send))
               {
                  r = Mohid.Core.Result.UNKNOWN;
                  try_counter = 1;

                  while (try_counter <= data.Tries)
                  {                     
                     r = sms_engine.SendSMS("+351" + toSend["MobilePhone"].ToString(), toSend["SMS_Text"].ToString());
                     if (r == Mohid.Core.Result.OK)
                        break;
                     try_counter++;
                  }

                  if (r != Mohid.Core.Result.OK)
                  {
                     errors.AppendLine(DateTime.Now + ": SMS sending failed.");
                     errors.AppendLine("Mobile Phone: " + toSend["MobilePhone"].ToString());
                     errors.AppendLine("SMS Day     : " + toSend["SMS_Day"].ToString());
                     errors.AppendLine("SMS Hour    : " + toSend["SMS_Hour"].ToString());
                     errors.AppendLine("SMS Text    : " + toSend["SMS_Text"].ToString());
                     errors.AppendLine("----------------------------------");
                  }
                  else
                  {
                     if (!LogMessageOnDatabase(toSend))
                        throw new Exception("Was not possible to LOG the message in the database");
                  }
               }
            }
            return true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.SendMessagesFromDatabase Exception: {0}", ex.Message);
            return false;
         }
      }

      protected bool LogMessageOnDatabase(OdbcDataReader toSend)
      {
         try
         {
            string query = "INSERT INTO SMS_Log (MobilePhone, SMS_Day, SMS_Hour, SMS_Text, DateTimeSent, ID_Group, SMS_Sent) VALUES ('" +
                           toSend["MobilePhone"].ToString() + "', '" + toSend["SMS_Day"].ToString() + "', '" + ((DateTime)toSend.GetDateTime(toSend.GetOrdinal("SMS_Hour"))).ToString("hh:mm:ss") + "', '" +
                           toSend["SMS_Text"].ToString() + "', '" + DateTime.Now + "', '" + toSend["ID_Group"].ToString() + "', '" +
                           DateTime.Now + "')";

            int r = database.ExecuteCommand(query);
            
            if (r < 1) 
               return false;
            
            return true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.LogMessageOnDatabase Exception: {0}", ex.Message);

            return false;
         }

      }

      protected OdbcDataReader FindSMSToSend(DateTime date_time_to_send)
      {
         try
         {
            string query = "SELECT Farmer.Ativo, Farmer.MobilePhone, FarmerSMSInfo.SMS_Day, FarmerSMSInfo.SMS_Hour, SMS.SMS_Text, SMS.ID_Group " +
                           "FROM ((Farmer INNER JOIN FarmerGroups ON Farmer.ID_Farmer=FarmerGroups.ID_Farmer) INNER JOIN FarmerSMSInfo " +
                           "ON Farmer.ID_Farmer=FarmerSMSInfo.Farmer_ID) INNER JOIN SMS " +
                           "ON FarmerGroups.ID = SMS.ID_Group " +
                           "WHERE Farmer.Ativo = 1 AND FarmerSMSInfo.SMS_Day = " + ((int)(date_time_to_send.DayOfWeek) + 1) + " AND " +
                           "SMS.SMS_Day = FarmerSMSInfo.SMS_Day AND " +
                           "FarmerSMSInfo.SMS_Hour <= #" + date_time_to_send.Hour + ":" + date_time_to_send.Minute + ":" + date_time_to_send.Second + "#";

            OdbcDataReader reader = database.ExecuteQuery(query);
            return reader;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.FindSMSToSend Exception: {0}", ex.Message);

            return null;
         }
      }

      protected bool MessageWasAlreadySent(OdbcDataReader toSend, DateTime date_time_to_send)
      {
         try
         {
            string query = "SMS_Log.ID_Group = " + toSend["ID_Group"].ToString() + " AND " +
                           "SMS_Log.SMS_Day = '" + toSend["SMS_Day"].ToString() + "' AND " +
                           "SMS_Log.SMS_Sent >= #" + date_time_to_send.Date.Year + "-" + date_time_to_send.Date.Month + "-" + date_time_to_send.Date.Day + " 00:00:00# AND " +
                           "SMS_Log.SMS_Sent <= #" + date_time_to_send.Date.Year + "-" + date_time_to_send.Date.Month + "-" + date_time_to_send.Date.Day + " 23:59:59#";

            int count = database.Count("SMS_Log", query);

            if (count == -1)
               throw new Exception("Error when trying to check if message was already sent.");

            if (count > 0)
               return true;
            else
               return false;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.MessageWasAlreadySent Exception: {0}", ex.Message);

            Console.WriteLine("Engine.MessageWasAlreadySent Warning:");
            Console.WriteLine("   A message could not be verifyed due an error.");
            return true;
         }
      }

      protected bool OpenDatabase()
      {
         try
         {
            database = new Database();
            string connection_str = data.ConnectionString;

            Mohid.Core.Result r = database.Connect(connection_str);
            if (r != Mohid.Core.Result.TRUE)
            {
               if (r == Mohid.Core.Result.EXCEPTION)
                  throw database.RaisedException;

               return false;
            }

            return true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.OpenDatabase Exception: {0}", ex.Message);
            
            return false;
         }
      }

      protected bool CloseDatabase()
      {
         try
         {
            database.Disconnect();
            return true;
         }
         catch (Exception ex)
         {
            if (Debug)
               Console.WriteLine("Engine.OpenDatabase Exception: {0}", ex.Message);

            return false;
         }
      }
   }
}
