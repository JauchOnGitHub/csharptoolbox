using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   public class TimeseriesBlock
   {
      public int Index;

      public string Name,
                    I, J, K,
                    MaskID, Layer,
                    Latitude, Longitude,
                    X, Y;
      public bool UseIndexes;

      public override string ToString()
      {
         return Name;
      }
   }

   public class ParameterBlock
   {
      public int Index;
      public string Name, Group;

      public override string ToString()
      {
         return Name;
      }
   }
}
