using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.Threading;
using System.ComponentModel;

using Mohid.Files;

namespace Mohid
{
   namespace Software
   {
      public enum AppExitStatus
      {
         Finished,
         Failed,
         Canceled,
         Exception,
         Unknown
      }

      public enum CheckSuccessMethod
      {
         UNKNOWN,
         DONOTCHECK,
         DEFAULTOUTPUT,
         ERROROUTPUT,
         EXITCODE
      }

      public enum SearchTextOrder
      {
         UNKNOWN,
         FROMBEGIN,
         FROMEND
      }

      public class ExternalApp
      {
         protected AppOptions opts;
         protected Exception exception;

         protected int successExitCode;

         public AppOptions Options
         {
            get { return opts; }
            set
            {
               opts = value;
            }
         }

         public string WorkingDirectory
         {
            get { return opts.workingDirectory.Path; }
            set { opts.workingDirectory.Path = value; }         
         }
         public string Executable
         {
            get { return opts.exeFile.FullPath; }
            set { opts.exeFile.FullPath = value; }
         }
         public bool Wait
         {
            get { return opts.wait; }
            set { opts.wait = value; }
         }
         public bool SaveDefaultOutput
         {
            get { return opts.saveDefaultOutput; }
            set { opts.saveDefaultOutput = value; }
         }
         public bool SaveErrorOutput
         {
            get { return opts.saveErrorOutput; }
            set { opts.saveErrorOutput = value; }
         }
         public bool Verbose 
         {
            get { return opts.verbose; }
            set { opts.verbose = value; }
         }

         public string TextToCheck
         {
            get { return opts.textToCheck; }
            set
            {
               opts.textToCheck = value;
               if (opts.textToCheck == null || opts.textToCheck == "")
                  opts.checkSuccessMethod = Software.CheckSuccessMethod.DONOTCHECK;
            }
         }
         public string Arguments
         {
            get { return opts.arguments; }
            set { opts.arguments = value; }
         }
         public CheckSuccessMethod CheckSuccessMethod
         {
            get { return opts.checkSuccessMethod; }
            set { opts.checkSuccessMethod = value; }
         }         
         public SearchTextOrder SearchTextOrder
         {
            get { return opts.searchTextOrder; }
            set { opts.searchTextOrder = value; }
         }
         public int SuccessExitCode
         {
            get { return successExitCode; }
            set { successExitCode = value; }
         }
         public bool UseShell
         {
            get { return opts.useShell; }
            set { opts.useShell = value; }
         }

         public Exception Exception
         {
            get { return exception; }
         }

         public virtual void Reset()
         {
            exception = null;

            if (opts != null)
               opts.Reset();
            else
               opts = new Software.AppOptions();

            ResetOutput();

            successExitCode = 0;
         }

         public static List<string> DefaultOutput = new List<string>();
         public static List<string> ErrorOutput = new List<string>();

         public ExternalApp()
         {
            opts = null;
            Reset();
         }

         public void ResetOutput()
         {
            DefaultOutput.Clear();
            ErrorOutput.Clear();
         }      

         public bool Run()
         {
            try
            {
               bool saveDef = opts.saveDefaultOutput,
                    saveErr = opts.saveErrorOutput;

               ResetOutput();

               Process objProcess = new Process();               

               if (!opts.wait)
               {
                 saveDef = false;
                 saveErr = false;
               }

               if (opts.checkSuccessMethod == Software.CheckSuccessMethod.DEFAULTOUTPUT)
                  saveDef = true;

               if (opts.checkSuccessMethod == Software.CheckSuccessMethod.ERROROUTPUT)
                  saveErr = true;

               if (string.IsNullOrEmpty(opts.workingDirectory.Path))
                  opts.workingDirectory.Path = opts.exeFile.Path;
               
               objProcess.StartInfo.RedirectStandardOutput = saveDef;
               objProcess.StartInfo.RedirectStandardError = saveErr;
               objProcess.StartInfo.FileName = opts.exeFile.FullPath;
               objProcess.StartInfo.Arguments = opts.arguments;
               objProcess.StartInfo.UseShellExecute = opts.useShell;
               objProcess.StartInfo.WorkingDirectory = opts.workingDirectory.Path;

               if (saveDef) 
                  objProcess.OutputDataReceived += new DataReceivedEventHandler(NewOutputData);

               if (saveErr) 
                  objProcess.ErrorDataReceived += new DataReceivedEventHandler(NewErrorData);

               objProcess.Start();               

               if (saveDef) objProcess.BeginOutputReadLine();
               if (saveErr) objProcess.BeginErrorReadLine();

               if (opts.wait)
                  objProcess.WaitForExit();

               int exitCode = objProcess.ExitCode;

               bool successfull = false;
               switch (opts.checkSuccessMethod)
               {
                  case Software.CheckSuccessMethod.EXITCODE:
                     if (exitCode == successExitCode)
                        successfull = true;
                     break;
                  case Software.CheckSuccessMethod.DEFAULTOUTPUT:
                     if (saveDef)
                        successfull = FindText(ref DefaultOutput);          
                     break;
                  case Software.CheckSuccessMethod.ERROROUTPUT:
                     if (saveErr)
                        successfull = FindText(ref ErrorOutput);
                     break;       
                  default:
                     successfull = true;
                     break;
               }

               exception = null;
               return successfull;
            }
            catch(Exception ex)
            {
               exception = ex;
               return false;
            }
         }

         public AppExitStatus Run(BackgroundWorker worker, DoWorkEventArgs e)
         {
            try
            {
               bool saveDef = opts.saveDefaultOutput,
                    saveErr = opts.saveErrorOutput;

               ResetOutput();

               Process objProcess = new Process();

               if (!opts.wait)
               {
                  saveDef = false;
                  saveErr = false;
               }

               if (opts.checkSuccessMethod == Software.CheckSuccessMethod.DEFAULTOUTPUT)
                  saveDef = true;

               if (opts.checkSuccessMethod == Software.CheckSuccessMethod.ERROROUTPUT)
                  saveErr = true;

               if (string.IsNullOrEmpty(opts.workingDirectory.Path))
                  opts.workingDirectory.Path = opts.exeFile.Path;

               objProcess.StartInfo.CreateNoWindow = true;
               objProcess.StartInfo.RedirectStandardOutput = saveDef;
               objProcess.StartInfo.RedirectStandardError = saveErr;
               objProcess.StartInfo.FileName = opts.exeFile.FullName;
               objProcess.StartInfo.Arguments = opts.arguments;
               objProcess.StartInfo.UseShellExecute = opts.useShell;
               objProcess.StartInfo.WorkingDirectory = opts.workingDirectory.Path;

               if (saveDef)
                  objProcess.OutputDataReceived += new DataReceivedEventHandler(NewOutputData);

               if (saveErr)
                  objProcess.ErrorDataReceived += new DataReceivedEventHandler(NewErrorData);

               objProcess.Start();

               if (saveDef) objProcess.BeginOutputReadLine();
               if (saveErr) objProcess.BeginErrorReadLine();

               if (opts.wait)
                  objProcess.WaitForExit();
               else
               {
                  while (!objProcess.HasExited)
                  {
                     if (worker.CancellationPending)
                     {
                        objProcess.Kill();
                        return AppExitStatus.Canceled;
                     }
                     Thread.Sleep(200);
                  }
               }

               int exitCode = objProcess.ExitCode;

               AppExitStatus successfull = AppExitStatus.Unknown;
               switch (opts.checkSuccessMethod)
               {
                  case Software.CheckSuccessMethod.EXITCODE:
                     if (exitCode == successExitCode)
                        successfull = AppExitStatus.Finished;
                     break;
                  case Software.CheckSuccessMethod.DEFAULTOUTPUT:
                     if (saveDef)
                     {
                        if (FindText(ref DefaultOutput))
                           successfull = AppExitStatus.Finished;
                        else
                           successfull = AppExitStatus.Failed;
                     }
                     break;
                  case Software.CheckSuccessMethod.ERROROUTPUT:
                     if (saveErr)
                     {
                        if (FindText(ref ErrorOutput))
                           successfull = AppExitStatus.Finished;
                        else
                           successfull = AppExitStatus.Failed;
                     }
                     break;
                  default:
                     successfull = AppExitStatus.Finished;
                     break;
               }

               exception = null;
               return successfull;
            }
            catch (Exception ex)
            {
               exception = ex;
               return AppExitStatus.Exception;
            }
         }

         protected void NewOutputData(object sendingProcess, DataReceivedEventArgs outLine)
         {
            DefaultOutput.Add(outLine.Data);
            if (opts.verbose)
               Console.WriteLine(outLine.Data);
         }

         protected void NewErrorData(object sendingProcess, DataReceivedEventArgs outLine)
         {
            ErrorOutput.Add(outLine.Data);
            if (opts.verbose)
               Console.WriteLine(outLine.Data);
         }

         protected bool FindText(ref List<string> text)
         {
            int i;

            if (opts.searchTextOrder == Software.SearchTextOrder.FROMBEGIN)
            {
              for (i = 0 ; i < text.Count; i++)
                 if (!string.IsNullOrWhiteSpace(text[i]) && text[i].Contains(opts.textToCheck))
                   return true;
            }   
            else
            {
              for (i = text.Count - 1; i >= 0; i--)
                 if (!string.IsNullOrWhiteSpace(text[i]) && text[i].Contains(opts.textToCheck)) 
                  return true;
            }

            return false;
         }


      }
   }
}