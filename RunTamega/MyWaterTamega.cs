//DLLNAME: MohidToolboxCore
//DLLNAME: System

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Simulation;
using Mohid.Core;
using Mohid.Log;

namespace Mohid
{
   public class MyWaterTamega
   {
      protected List<InputFileTemplate> first;
      protected List<InputFileTemplate> next;      
      protected FilePath resFolder;
      protected FilePath storeFolder;
      protected MohidSimulation sim;
      protected DateTime endOfSimulation;
      protected LogEngine log; 
      protected int lastSimNumber;
      protected int iterations;
      protected double left;
      protected bool changeTemplates;
      protected DateTime runStart;
      protected DateTime runEnd;
      protected bool RestartFailedRun;
      protected int simID;
      protected int maxIterations;
      protected string dateFormat;
      protected CultureInfo provider;
      protected FilePath oldFolder;
      
      public void Init()
      {
         sim = new MohidSimulation();
         log = new LogEngine();
         first = new List<InputFileTemplate>();
         next = new List<InputFileTemplate>();
         dateFormat = "yyyy/MM/dd HH:mm:ss";
         provider = CultureInfo.InvariantCulture;
      }

      public bool Run(CmdArgs args)
      {
         Init();

         bool res = false;
         if (LoadConfig(args))
            res = RunSimulation();

         return res;
      }

      protected bool FindFirstTemplateInfoBlocks(ConfigNode toMatch)
      {
         if (toMatch.Name == "start.template.file")
            return true;
         return false;
      }
      protected bool FindNextTemplateInfoBlocks(ConfigNode toMatch)
      {
         if (toMatch.Name == "continuation.template.file")
            return true;
         return false;
      }
      //protected bool FindTemplatesSpecialSetup(ConfigNode toMatch)
      //{
      //   if (toMatch.Name == "template.special.setup")
      //      return true;
      //   return false;
      //}


      protected bool LoadConfig(CmdArgs args)
      {
         Config cfg = new Config();         

         string configFile;
         if (args.HasParameter("cfg"))
            configFile = args.Parameter("cfg");
         else
            configFile = "sim.cfg";

         if (args.HasParameter("max.iter"))
            maxIterations = int.Parse(args.Parameter("max.iter"));
         else
            maxIterations = -1;

         cfg.ConfigFile.FullPath = configFile;
         if (!cfg.Load())
            return false;
         
         ConfigNode root = cfg.Root;

         try
         {
            sim.SimDirectory = root["sim.folder", "sim"].AsFilePath();
            if (!log.Load(root["log.file", sim.SimDirectory.Path + "sim.log"].AsFileName()))
               return false;  

            RestartFailedRun = root["restart.failed.run", true].AsBool();
            if (log.Count > 0)
            {
               ConfigNode lastEntry = log[log.Count - 1];
               if (!lastEntry["run.status"].AsBool())
               {
                  if (RestartFailedRun)
                  {
                     sim.Start = lastEntry["sim.start"].AsDateTime(dateFormat);
                     sim.SimLenght = lastEntry["sim.lenght"].AsDouble();
                     simID = lastEntry["sim.id"].AsInt();
                  }
                  else
                     return false;
               }
               else
               {
                  sim.Start = lastEntry["sim.end"].AsDateTime(dateFormat);
                  sim.SimLenght = root["sim.lenght", 14].AsDouble();
                  simID = lastEntry["sim.id"].AsInt() + 1;
               }
            }
            else
            {
               sim.Start = root["sim.start"].AsDateTime(dateFormat);
               sim.SimLenght = root["sim.lenght", 14].AsDouble();
               simID = 1;
            }
            
            sim.WorkingDirectory = root["working.folder", "."].AsFilePath();
            sim.CheckRun = root["check.run", true].AsBool();
            sim.Verbose = root["verbose", true].AsBool();
            sim.Wait = root["wait", true].AsBool();
            sim.SuccessString = root["check.this", "successfully terminated"].AsString();
            sim.DataDirectory = root["data.folder", sim.SimDirectory.Path + "data"].AsFilePath();
            sim.SetupRunPeriod = root["setup.run.period", false].AsBool();
            if (sim.SetupRunPeriod)
            {
               sim.EndTAG = root["sim.end.tag", "<<end>>"].AsString();
               sim.StartTAG = root["sim.start.tag", "<<start>>"].AsString();
            }
            sim.SetupRunPeriod = root["wait", false].AsBool(); 
            sim.SaveOutput = true;
            sim.OutputFile = new FileName(sim.SimDirectory.Path + "res" + System.IO.Path.DirectorySeparatorChar + root["output.file", "result.txt"].AsString());
            sim.Executable = new FileName(sim.SimDirectory.Path + "exe" + System.IO.Path.DirectorySeparatorChar + root["mohid.executable", "mohid.exe"].AsString());
            sim.CreateInputFiles = root["use.templates", false].AsBool();
            endOfSimulation = root["sim.end"].AsDateTime(dateFormat);
            resFolder = root["results.folder", sim.SimDirectory.Path + "res"].AsFilePath();
            storeFolder = root["store.folder", sim.SimDirectory.Path + "store"].AsFilePath();
            oldFolder = root["old.folder", sim.SimDirectory.Path + "old"].AsFilePath();

            if (sim.SetupRunPeriod && !sim.CreateInputFiles)
               return false;

            if (sim.CreateInputFiles)
            {
               InputFileTemplate newTemplate;
               List<ConfigNode> itfList = root.ChildNodes.FindAll(FindFirstTemplateInfoBlocks);
               foreach (ConfigNode ticn in itfList)
               {
                  newTemplate = new InputFileTemplate(ticn["file"].AsFileName().FullPath,
                                                      (InputFileTemplateType)Enum.Parse(typeof(InputFileTemplateType), ticn["type", "data"].AsString(), true));
                  first.Add(newTemplate);
               }

               changeTemplates = root["change.templates", true].AsBool();
               if (changeTemplates)
               {
                  itfList = root.ChildNodes.FindAll(FindNextTemplateInfoBlocks);
                  foreach (ConfigNode ticn in itfList)
                  {
                     newTemplate = new InputFileTemplate(ticn["file"].AsFileName().FullPath,
                                                         (InputFileTemplateType)Enum.Parse(typeof(InputFileTemplateType), ticn["type", "data"].AsString(), true));
                     next.Add(newTemplate);
                  }
               }

               //itfList = root.ChildNodes.FindAll(FindTemplatesSpecialSetup);
               //if (itfList.Count > 0)
               //{
               //   templatesSpecialSetup = true;
               //}
               //else
               //   templatesSpecialSetup = false;
            }            
         }
         catch 
         {
            return false;
         }

         return true;
      }

      protected bool PreProcessing()
      {
         return true;
      }

      protected bool OnBegin()
      {
         //Atmosphere (it's specific for Tamega simulations)
         DateTime hdfStartDate = new DateTime(2003, 10, 1, 0, 0, 0);

         //To this work, the sim start date for sim 1 must coincide with hdf start date or be an exact 14 days multiple
         //if (simID == 1 & sim.Start != hdfStartDate)
         //   return false;

         int hdfIdToUse = (int)((sim.Start - hdfStartDate).TotalDays / 14.0) + 1;

         //finds the template file that has the Atmosphere file
         foreach (InputFileTemplate itf in sim.TemplateFilesList)
         {
            if (itf.Name.ToLower() == "atmosphere")
               itf.ReplaceList["<<meteo_rain>>"] = "precipitation_" + hdfIdToUse.ToString();
         }

         return true;
      }

      protected bool OnEnd()
      {
         FilePath store = FileTools.CreateFolder(sim.Start.ToString("yyyyMMdd.HHmmss") + "-" + sim.End.ToString("yyyyMMdd.HHmmss"), storeFolder);
         if (!FileTools.CopyFile(resFolder, oldFolder, "*.fin*", CopyOptions.OVERWRIGHT))
            return false;
         return FileTools.CopyFile(resFolder, store, "*.*", Files.CopyOptions.OVERWRIGHT); 
        
      }

      protected bool RunSimulation()
      {
         sim.PreProcessing = PreProcessing;
         //sim.OnBegin = OnBegin;
         //sim.OnEnd = OnEnd;         

         bool res;

         sim.TemplateFilesList = first;

         while (maxIterations != 0)
         {
            if (sim.Start < endOfSimulation)
            {
               if (endOfSimulation < sim.End)
                  sim.End = endOfSimulation;

               if (simID > 1 && changeTemplates)
                  sim.TemplateFilesList = next;

               if (!OnBegin())
               {
                  SaveRunLog(false);
                  return false;
               }

               runStart = DateTime.Now;
               res = sim.Run();
               runEnd = DateTime.Now;

               if (res)
                  if (!OnEnd())
                  {
                     SaveRunLog(false);
                     Console.WriteLine("File copy was not successfull.");
                     return false;
                  }

               SaveRunLog(res);
               if (!res)
                  return false;

               sim.Start = sim.End;

               //if (changeTemplates)
               //   ChangeTemplates();

               simID++;
            }
            else
               break;

            maxIterations--;
         }

         return true;
      }

      protected void SaveRunLog(bool success)
      {
         ConfigNode data = new ConfigNode("log.entry");
         data["run.start"] = new KeywordData(runStart.ToString(dateFormat));
         data["run.end"] = new KeywordData(runEnd.ToString(dateFormat));
         data["sim.start"] = new KeywordData(sim.Start.ToString(dateFormat));
         data["sim.end"] = new KeywordData(sim.End.ToString(dateFormat));
         data["sim.lenght"] = new KeywordData(sim.SimLenght.ToString());
         data["run.status"] = new KeywordData(success.ToString());
         data["sim.id"] = new KeywordData(simID.ToString());

         log.AddEntry(data);
         log.Save();
      }

      protected void ChangeTemplates()
      {
         changeTemplates = false;
         sim.TemplateFilesList = next;
      }

   }
}
