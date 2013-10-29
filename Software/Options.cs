using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Files;

namespace Mohid
{
   namespace Software
   {
      public class AppOptions
      {
         public FilePath workingDirectory;
         public FileName exeFile;

         public bool wait,
                     saveDefaultOutput,
                     saveErrorOutput,
                     useShell;

         public string textToCheck,
                       arguments;

         public CheckSuccessMethod checkSuccessMethod;
         public SearchTextOrder searchTextOrder;

         public int successExitCode;

         public bool verbose;
         public bool showProcessCompletedMessage;

         public bool ShowProcessCompletedMessage
         {
            get { return showProcessCompletedMessage; }
            set { showProcessCompletedMessage = value; }
         }

         public string WorkingDirectory
         {
            get { return workingDirectory.Path; }
            set { workingDirectory.Path = value; }
         }
         public string Executable
         {
            get { return exeFile.FullPath; }
            set { exeFile.FullPath = value; }
         }
         public bool Wait
         {
            get { return wait; }
            set { wait = value; }
         }
         public bool SaveDefaultOutput
         {
            get { return saveDefaultOutput; }
            set { saveDefaultOutput = value; }
         }
         public bool SaveErrorOutput
         {
            get { return saveErrorOutput; }
            set { saveErrorOutput = value; }
         }
         public bool Verbose
         {
            get { return verbose; }
            set { verbose = value; }
         }

         public string TextToCheck
         {
            get { return textToCheck; }
            set
            {
               textToCheck = value;
               if (textToCheck == null || textToCheck == "")
                  checkSuccessMethod = Software.CheckSuccessMethod.DONOTCHECK;
            }
         }
         public string Arguments
         {
            get { return arguments; }
            set { arguments = value; }
         }
         public CheckSuccessMethod CheckSuccessMethod
         {
            get { return checkSuccessMethod; }
            set { checkSuccessMethod = value; }
         }
         public SearchTextOrder SearchTextOrder
         {
            get { return searchTextOrder; }
            set { searchTextOrder = value; }
         }
         public int SuccessExitCode
         {
            get { return successExitCode; }
            set { successExitCode = value; }
         }
         public bool UseShell
         {
            get { return useShell; }
            set { useShell = value; }
         }

         public AppOptions()
         {
            Reset();
         }

         public void Reset()
         {
            workingDirectory = new FilePath();
            exeFile = new FileName();
            wait = true;
            checkSuccessMethod = Software.CheckSuccessMethod.DONOTCHECK;
            textToCheck = "";
            searchTextOrder = Software.SearchTextOrder.FROMEND;
            saveDefaultOutput = false;
            saveErrorOutput = false;
            useShell = false;
            arguments = "";
            verbose = false;
            successExitCode = 0;
            showProcessCompletedMessage = false;
         }
      }
   }
}
