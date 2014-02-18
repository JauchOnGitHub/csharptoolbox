using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace Mohid
{
   public class Setup
   {
      public static void StandardSetup()
      {
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      }
   }
}
