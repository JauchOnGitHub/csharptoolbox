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

namespace Mohid
{
   public class MyWaterTamega : BCIMohidSim
   {
      public override bool OnSimStart(object data)
      {
         MohidRunEngineData mre = (MohidRunEngineData)data;

         //Atmosphere (it's specific for Tamega simulations)
         DateTime hdfStartDate = new DateTime(2003, 10, 1, 0, 0, 0);

         int hdfIdToUse = (int)((mre.sim.Start - hdfStartDate).TotalDays / 14.0) + 1;

         //finds the template file that has the Atmosphere file
         foreach (InputFileTemplate itf in mre.sim.TemplateFilesList)
         {
            if (itf.Name.ToLower() == "atmosphere")
               itf.ReplaceList["<<meteo_rain>>"] = "precipitation_" + hdfIdToUse.ToString();
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
