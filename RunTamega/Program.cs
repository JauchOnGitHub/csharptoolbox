using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mohid;
using Mohid.Core;
using Mohid.CommandArguments;

namespace RunTamega
{
   class Program
   {
      static void Main(string[] args)
      {
         Setup.StandartSetup();

         CmdArgs cmdArgs = new CmdArgs(args);
         
         MyWaterTamega mwt = new MyWaterTamega();
         mwt.Run(cmdArgs);
      }
   }
}
