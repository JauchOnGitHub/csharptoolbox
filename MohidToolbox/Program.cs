using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mohid.Script;
using Mohid.Files;
using Mohid.CommandArguments;
using Mohid;
using System.Runtime.InteropServices;

namespace MohidToolbox
{
   static class Program
   {
      [DllImport("kernel32.dll")]
      static extern bool AttachConsole(int dwProcessId);
      private const int ATTACH_PARENT_PROCESS = -1;

      [STAThread]

      static void Main()
      {
         CmdArgs cmdArgs = new CmdArgs(Environment.GetCommandLineArgs());

         if (cmdArgs.Options.Count <= 0 && cmdArgs.Parameters.Count <= 0 && cmdArgs.Arguments.Count <= 1)
         {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return;
         }

         AttachConsole(ATTACH_PARENT_PROCESS);

         if (cmdArgs.HasParameter("sc"))
         {
            ScriptCompiler script = new ScriptCompiler();

            if (!script.Run(new FileName(cmdArgs.Parameter("s")), cmdArgs))
               Console.WriteLine("The run failed.");
            else
               Console.WriteLine("The run was ok.");
         }
         else if (cmdArgs.HasParameter("sv"))
         {
            Console.WriteLine("Run VB.NET script is not implemented yet.");
         }
         else if (cmdArgs.HasParameter("l"))
         {
            Console.WriteLine("Run from DLL is not implemented yet.");
         }
         else
         {
            Console.WriteLine("");
            Console.WriteLine("To launch visual interface: MohidToolBox");
            Console.WriteLine("To use command line:        MohidToolbox [[--sc][--sv] scriptfilename] [--l dllfilename]");
            Console.WriteLine("");
            Console.WriteLine("       --sc : Used to indicate a CSharp script file name");
            Console.WriteLine("       --sv : Used to indicate a VB.NET script file name (not implemented)");
            Console.WriteLine("       --l  : Used to indicate a DLL script file name (not implemented)");
            Console.WriteLine("");
            Console.WriteLine("If no options are present, the visual interface will be launched.");
            Console.WriteLine("ATTENTION: Only ONE of the above options can be used at a time");
            Console.WriteLine("");
            Console.WriteLine("Press a key...");
         }

         return;
      }
   }
}
