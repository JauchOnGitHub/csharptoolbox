using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Core
   {
      public enum ExceptionType
      {
         UNKNOWN,
         WARNING,
         ERROR
      }

      public class GeneralException : Exception
      {
         protected ExceptionType type;

         public ExceptionType Type { get { return type; } }

         public GeneralException(ExceptionType type = ExceptionType.UNKNOWN)
            : base()
         {
            this.type = type;
         }

         public GeneralException(string message, ExceptionType type = ExceptionType.UNKNOWN)
            : base(message)
         {
            this.type = type;
         }

         public GeneralException(string message, Exception innerException, ExceptionType type = ExceptionType.UNKNOWN)
            : base(message, innerException)
         {
            this.type = type;
         }

         public GeneralException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context, ExceptionType type = ExceptionType.UNKNOWN)
            : base(info, context)
         {
            this.type = type;
         }
      }
   }
}
