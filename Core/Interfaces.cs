using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Core
   {
      public delegate bool Run();

      public interface IRun
      {         
         bool Run();
      }

      public interface IMohidSim
      {
         bool Setup(object data);
         bool OnStart(object data);
         bool OnEnd(object data);
         bool OnPreProcessing(object data);
         bool OnSimStart(object data);
         bool OnSimEnd(object data);
         void OnException(object data);
         bool OnRunFail(object data);
         bool AfterInitialization(object data);
         Exception ExceptionRaised();
      }

      public class BCIMohidSim : IMohidSim
      {
         public virtual bool Setup(object data) { return true; }
         public virtual bool OnStart(object data) { return true; }
         public virtual bool OnEnd(object data) { return true; }
         public virtual bool OnPreProcessing(object data) { return true; }
         public virtual bool OnSimStart(object data) { return true; }
         public virtual bool OnSimEnd(object data) { return true; }
         public virtual void OnException(object data) { }
         public virtual bool OnRunFail(object data) { return false; }
         public virtual bool AfterInitialization(object data) { return true; }
         public virtual Exception ExceptionRaised() { return null; }
      }
   }
}
