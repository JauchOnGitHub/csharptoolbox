using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid;
using Mohid.Configuration;
using Mohid.CommandArguments;

namespace Mohid
{
   namespace Script
   {
      public interface IMohidScript
      {
         bool Run(CmdArgs args);
      }

      public interface IMohidTask
      {
         bool Run(ConfigNode cfg);
         Exception LastException { get; }
         bool Reset();
      }
   }
}
