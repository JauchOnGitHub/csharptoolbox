using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mohid
{
   namespace Workspace
   {
      public interface mwPlugin
      {
         public bool Initialize(mwAPI api);   
      }
   }
}