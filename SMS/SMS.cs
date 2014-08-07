using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Text.RegularExpressions;

using Mohid.Core;

namespace Mohid
{
   namespace SMS
   {
      public struct CommandSettings
      {         
         public int TimeToWait { get; set; }
         public bool IncludeLineTerminator { get; set; }
      }

      public class SMSEngine
      {
         #region INTERNAL DATA
         protected bool ownerOfPort;
         protected SerialPort port;
         protected int fTimeOut;
         protected string exceptionMessage;
         #endregion INTERNAL DATA

         #region PROPERTIES / PUBLIC DATA
         public bool Debug { get; set; }
         public bool Verbose { get; set; }
         public string ExceptionMessage { get { return exceptionMessage; } }
         public CommandSettings CmdSettings;
         public string Message { get; set; }
         public int IntervalBetweenMessages { get; set; }         
         public int TimeOut
         {
            get { return fTimeOut; }
            set
            {
               fTimeOut = value;
               Port.WriteTimeout = fTimeOut;
               Port.ReadTimeout = fTimeOut;
            }
         }
         public string PinCode { get; set; }
         public string SMSCenter { get; set; }
         public int WaitTime { get; set; }
         public int Baudrate { get; set; }
         public SerialPort Port 
         {
            get { return port; }
            set
            {
               ClosePort();
               ownerOfPort = false;
               port = value;
            }
         }
         #endregion PROPERTIES / PUBLIC DATA

         #region CLASS INITIALIZATON
         protected void Initialize(SerialPort port)
         {
            fTimeOut = 10000;
            WaitTime = 200; //milliseconds
            PinCode = "";
            SMSCenter = "";
            IntervalBetweenMessages = 10000; //milliseconds
            Message = "";
            CmdSettings.IncludeLineTerminator = true;
            CmdSettings.TimeToWait = 200;
            this.port = port;
            Debug = false;
            Verbose = false;
         }
         public SMSEngine()
         {
            ownerOfPort = true;
            Initialize(new SerialPort());
            ResetPortSettings();
         }
         public SMSEngine(SerialPort port)
         {
            ownerOfPort = false;
            Initialize(port);
            ClosePort();
         }
         public void ResetPortSettings()
         {
            ClosePort();

            Port.WriteTimeout = fTimeOut;
            Port.ReadTimeout = fTimeOut;
            Port.PortName = "COM1";
            Port.BaudRate = Baudrate;
            Port.Parity = Parity.None;
            Port.DataBits = 8;
            Port.StopBits = StopBits.One;
            Port.Handshake = Handshake.RequestToSend;
            Port.DtrEnable = true;
            Port.RtsEnable = true;
            Port.NewLine = Environment.NewLine;
         }
         #endregion CLASS INITIALIZATON

         #region ENGINE
         public static SerialPort GetNewPort()
         {
            SerialPort new_port = new SerialPort();

            new_port.WriteTimeout = 10000;
            new_port.ReadTimeout = 10000;
            new_port.PortName = "COM1";
            new_port.BaudRate = 9600;
            new_port.Parity = Parity.None;
            new_port.DataBits = 8;
            new_port.StopBits = StopBits.One;
            new_port.Handshake = Handshake.RequestToSend;
            new_port.DtrEnable = true;
            new_port.RtsEnable = true;
            new_port.NewLine = Environment.NewLine;

            return new_port;
         }
         public Result SendSMS(string cellNumber, string message)
         {
            try
            {
               Result r;
               List<string> results = new List<string>();

               if (Debug)
                  Console.WriteLine("Starting modem.");
               if ((r = Start()) != Result.TRUE)
               {
                  if (Debug)
                     Console.WriteLine("Modem start failed");

                  return r;
               }

               cellNumber = cellNumber.Trim();
               message = message.Trim();

               if (message.Length > 140)
                  message = message.Substring(0, 140).Trim();

               if (cellNumber.Substring(0, 4) != "+351")
                  cellNumber = "+351" + cellNumber;
        
               if (port.IsOpen)
               {
                  if ((r = SendCommand("AT+CMGS=" + cellNumber + "\r", 200, true, true)) != Result.OK) 
                     return r;

                  if ((r = SendCommand(message + (char)26, 10000, false, true)) != Result.OK)
                     return r;

                  return r;
               }

               throw new Exception("Tried to send SMS message but COM port is not open.");
            }
            catch(Exception ex)
            {
               exceptionMessage = ex.Message;
               if (Debug)
                  Console.WriteLine("SMS.SendMessage Exception: {0}", ex.Message);
               return Result.EXCEPTION;
            }
         }
         protected Result Start()
         {
            try
            {
               if (!port.IsOpen)
               {
                  port.Open();
                  if (!port.IsOpen)
                     throw new Exception("Unable to open port " + port.PortName);
                  if (InitializeModem() != Result.OK)
                     return Result.FALSE;
               }

               return Result.TRUE;
            }
            catch (Exception ex)
            {
               exceptionMessage = "Could not open COM port. Message returned: '" + ex.Message + "'.";
               if (Debug)
                  Console.WriteLine("SMS.Start Exception: {0}", ex.Message);
               return Result.EXCEPTION;
            }
         }

         protected Result InitializeModem()
         {
            Result r;

            try
            {
               if (Debug)
                  Console.WriteLine("Initializing modem.");
               if (Debug)
                  Console.WriteLine("Sending AT command."); 
               if ((r = SendCommand("AT")) != Result.OK) return r;
               if (Debug)
                  Console.WriteLine("Sending ATE0 command.");   
               if ((r = SendCommand("ATE0")) != Result.OK) return r;
               if (Debug)
                  Console.WriteLine("Sending ATV1 command."); 
               if ((r = SendCommand("ATV1")) != Result.OK) return r;               
               if ((r = CheckPinOk()) != Result.TRUE)
               {
                  if (string.IsNullOrWhiteSpace(PinCode))
                     throw new Exception("SIM card needs PIN but one was not provided.");
                  if ((r = SendCommand("AT+CPIN=" + PinCode)) != Result.OK) return r;
                  if ((r = CheckPinOk()) != Result.TRUE)
                     throw new Exception("Wrong PIN.");
               }
               if (Debug)
                  Console.WriteLine("Sending AT+CSCA command.");   
               if ((r = SendCommand("AT+CSCA=" + SMSCenter)) != Result.OK) return r;
               if (Debug)
                  Console.WriteLine("Sending AT+CMGF=1 command.");              
               if ((r = SendCommand("AT+CMGF=1")) != Result.OK) return r;               
            }
            catch (Exception ex)
            {
               exceptionMessage = ex.Message;
               if (Debug)
                  Console.WriteLine("SMS.InitializeModem Exception: {0}", ex.Message);
               return Result.EXCEPTION;
            }

            return Result.OK;
         }
         protected Result CheckPinOk()
         {
            Result r;
            List<string> results = new List<string>();
            if (Debug)
               Console.WriteLine("Sending at+cpin? command.");   
            if ((r = SendCommand("at+cpin?", CmdSettings.TimeToWait, CmdSettings.IncludeLineTerminator, true, results)) != Result.OK) return r;                    
        
            foreach (string res in results)
               if (res.Contains("+CPIN: SIM PIN")) return Result.FALSE;

            return Result.TRUE;
         }
         public Result SendCommand(string command, bool checkResult = true)
         {
            return SendCommand(command, CmdSettings.TimeToWait, CmdSettings.IncludeLineTerminator, checkResult);
         }
         public Result SendCommand(string command, int timeToWait, bool includeLineTerminator, bool checkResult = true, List<string> returned = null)
         {
            if (!string.IsNullOrWhiteSpace(command))
            {               
               try
               {
                  if (includeLineTerminator)
                     Port.WriteLine(command);
                  else Port.Write(command);
               }
               catch (Exception ex)
               {
                  exceptionMessage = ex.Message;
                  if (Debug)
                     Console.WriteLine("SMS.SendCommand Exception: {0}", ex.Message);
                  return Result.EXCEPTION;
               }
            }
            else
               return Result.ERROR;

            if (timeToWait > 0)
               Thread.Sleep(timeToWait);
            if (checkResult)
               return CheckResult(returned);

            return Result.OK;
         }
         public Result CheckResult(List<string> returned = null)
         {
            try
            {
               string result = "";

               for (; ; )
               {
                  result += Port.ReadExisting();                  

                  if (string.IsNullOrWhiteSpace(result))
                  {
                     result = "";
                     continue;
                  }

                  if (result.Contains("\r\n> "))
                  {
                     if (returned != null)
                        returned.Add(">");
                     return Result.OK;
                  }

                  if (result.EndsWith("\r\n"))
                  {
                     if (returned != null)
                        returned.Add(result);

                     if (result.ToUpper().Contains("ERR")) return Result.ERROR;
                     if (result.ToUpper().Contains("OK")) return Result.OK;
                  }
               }
            }
            catch (Exception ex)
            {
               exceptionMessage = ex.Message;
               if (Debug)
                  Console.WriteLine("SMS.CheckResult Exception: {0}", ex.Message);
               return Result.EXCEPTION;
            }
         }
         #endregion ENGINE

         #region FINALIZATION
         public void ClosePort()
         {
            if (ownerOfPort && Port != null && Port.IsOpen)
               Port.Close();
         }       
         ~SMSEngine()
         {
            ClosePort();
         }
         #endregion FINALIZATION
      }
   }
}
