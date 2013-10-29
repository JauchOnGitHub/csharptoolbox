//DLLNAME: MohidToolboxCore
//DLLNAME: System

using System;
using System.Collections.Generic;
using System.Text;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Simulation;
using Mohid.Core;
using Mohid.Log;
using Mohid.Software;

namespace Mohid
{
   public class MyWaterTamegaEVTPComparison : BCIMohidSim
   {
      protected FilePath boundaryconditions;
      protected FilePath mm5Path;

      public MyWaterTamegaEVTPComparison()
      {
         mm5Path = new FilePath();
      }

      public override bool OnSimStart(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         if (mre.simID == 1)
         {
            foreach(InputFileTemplate itf in mre.templatesStart)
            if (itf.Name.Contains("atmosphere"))
            {
               itf.ReplaceList["<<folder>>"] = mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss");
            }

         }
         else
         {
            foreach(InputFileTemplate itf in mre.templatesContinuation)
            if (itf.Name.Contains("atmosphere"))
            {
               itf.ReplaceList["<<folder>>"] = mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss");
            }
         }	  

         return true;
      }

      public override bool OnSimEnd(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         FilePath store = FileTools.CreateFolder(mre.sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + mre.sim.End.ToString("yyyyMMdd.HHmmss"), mre.storeFolder);
         if (!FileTools.CopyFile(mre.resFolder, mre.oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;

         return FileTools.CopyFile(mre.resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT);
      }
   }
}
