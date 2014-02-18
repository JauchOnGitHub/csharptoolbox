using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;
using Mohid.Simulation;
using Mohid.Core;
using Mohid.Log;

namespace Mohid
{
   namespace Simulation
   {
      public class SimUserException : Exception
      {
         public SimUserException() : base() { }
         public SimUserException(string message) : base(message) { }
         public SimUserException(string message, Exception innerException) : base(message, innerException) { }
         public SimUserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
      }

      public class SimConfigException : Exception
      {
         public SimConfigException() : base() { }
         public SimConfigException(string message) : base(message) { }
         public SimConfigException(string message, Exception innerException) : base(message, innerException) { }
         public SimConfigException(SerializationInfo info, StreamingContext context) : base(info, context) { }
      }

      public class MohidRunEngineData
      {
         public Config cfg;
         public IMohidSim userInterface;
         public List<InputFileTemplate> templatesStart;
         public List<InputFileTemplate> templatesContinuation;
         public FilePath resFolder;
         public FilePath storeFolder;
         public MohidSimulation sim;
         public DateTime endOfSimulation;
         public bool useEndOfSimulation;
         public LogEngine log;
         public bool changeTemplates;
         public DateTime runStart;
         public DateTime runEnd;
         public bool RestartFailedRun;
         public int simID;
         public int maxIterations;
         public string dateFormat;
         public CultureInfo provider;
         public FilePath oldFolder;
         public bool LastOperationResult;
         public CmdArgs args;
         public FileName logFileName;

         public MohidRunEngineData()
         {
            sim = new MohidSimulation();
            log = new LogEngine();
            templatesStart = new List<InputFileTemplate>();
            templatesContinuation = new List<InputFileTemplate>();
            dateFormat = "yyyy/MM/dd HH:mm:ss";
            provider = CultureInfo.InvariantCulture;
            LastOperationResult = true;
         }

      }

      public class MohidRunEngine
      {
         #region DATA         

         protected MohidRunEngineData data;
         protected DateTime simEnd;
         protected bool useEndDate;

         public bool Verbose { get; set; }

         #endregion DATA

         #region INITIALIZATION

         protected void Init()
         {
            data = new MohidRunEngineData();
         }

         public MohidRunEngine()
         {
            Init();
         }

         #endregion INITIALIZATION

         #region SCRIPT CONTROL

         protected IMohidSim LoadUserInterface(CmdArgs args)
         {
            try
            {
               FileName interfaceName;
               data.sim.PreProcessing = OnPreProcessing;

               if (args.HasParameter("script"))
                  interfaceName = new FileName(args.Parameters["script"]);
               else
                  return null;

               if (interfaceName.Extension.ToLower() == "dll") //it's a library
               {
                  if (!args.HasParameter("class"))
                     return null;

                  string class_name = args.Parameter("class");

                  Assembly ass = Assembly.LoadFrom(interfaceName.FullPath);
                  data.userInterface = (IMohidSim)Activator.CreateInstance(ass.GetType("Mohid." + class_name));
                  return data.userInterface;
               }
               else //it's a script
               {
                  ScriptCompiler sc = new ScriptCompiler();
                  Assembly ass = sc.Compile(interfaceName);
                  data.userInterface = (IMohidSim)sc.FindScriptInterface("IMohidSim", ass);
                  return data.userInterface;
               }
            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.LoadUserInterface", ex);
            }
         }

         #endregion SCRIPT CONTROL

         #region ENGINE

         public bool Run(CmdArgs args, IMohidSim test = null)
         {
            bool sim_run = true;
            data.args = args;
            
            try
            {
               if (test != null)
               {
                  data.userInterface = test;
               }
               else
               {
                  if (Verbose) Console.Write("Loading User Interface...");
                  LoadUserInterface(args);
                  if (Verbose) Console.WriteLine("[OK]");
               }

               OnStart();

               if (Verbose) Console.Write("Loading Configuration...");
               LoadConfig(args);
               if (Verbose) Console.WriteLine("[OK]");

               if (Verbose) Console.Write("Loading Log File...");
               LoadLogFile();
               if (Verbose) Console.WriteLine("[OK]");
            }
            catch(Exception ex)
            {
               if (Verbose)
               {
                  Console.WriteLine("[FAILED]");
                  Console.WriteLine("");
                  Console.WriteLine("An EXCEPTION was raised. The message returned was:");
                  Console.WriteLine(ex.Message);
                  Console.WriteLine("");
               }

               throw new Exception("MohidRunEngine.Run", ex);
            }

            try
            {
               AfterInit();

               if (data.useEndOfSimulation)
               {
                  useEndDate = true;
                  simEnd = data.endOfSimulation;
                  if (Verbose) Console.WriteLine("Using End from config file: {0}", simEnd.ToString("dd/MM/yyyy HH:mm:ss"));
               }
               else
               {
                  useEndDate = false;
                  //simEnd = data.sim.Start.AddDays(data.sim.SimLenght);
                  //if (Verbose) Console.WriteLine("Using End from run: {0}", simEnd);
               }

               int count = 0;
               string end;

               if (data.maxIterations < 1 && !data.useEndOfSimulation)
                  end = " of infinite simulations";
               else if (data.maxIterations > 0 && data.useEndOfSimulation)
                  end = " of " + data.maxIterations.ToString() + ", ending on " + data.endOfSimulation.ToString("yyyy-MM-dd");
               else if (data.maxIterations > 0)
                  end = " of " + data.maxIterations.ToString();
               else
                  end = ". Ending " + data.endOfSimulation.ToString("yyyy-MM-dd");

               if (Verbose && data.maxIterations > 0) Console.WriteLine("Number of max iterations expected: {0}", data.maxIterations);
               while (data.maxIterations != 0 && (!useEndDate || data.sim.Start < simEnd))
               {
                  try
                  {
                     SetupRun();

                     OnSimStart();

                     count++;
                     if (Verbose)
                     {
                        Console.WriteLine("");
                        Console.WriteLine("Simulation {0}{1}", count, end);
                        Console.WriteLine("Start: {0} End: {1}", data.sim.Start.ToString("yyyy-MM-dd HH:mm:ss"), data.sim.End.ToString("yyyy-MM-dd HH:mm:ss"));
                        Console.WriteLine("");
                     }

                     if (!(sim_run = Run()) && !OnRunFail())
                     {
                        AddNewEntryToLog(sim_run);
                        break;
                     }
                     
                     OnSimEnd();
                     AddNewEntryToLog(sim_run);

                     NextSimSetup();
                  }
                  catch (Exception ex)
                  {
                     Console.WriteLine("");
                     Console.WriteLine("An EXCEPTION was raised. The message returned was:");
                     Console.WriteLine(ex.Message);
                     Console.WriteLine("");

                     OnException();
                     throw;
                  }
               }

               OnEnd();

               if (Verbose)
               {
                  Console.WriteLine("");
                  Console.WriteLine("Finished.");
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine("Exception: {0}", ex.Message);
               Console.WriteLine("Saving LOG...");
               data.log.Save();

               throw new Exception("MohidRunEngine.Run", ex);
            }

            Console.WriteLine("Saving LOG...");
            data.log.Save(); 
            
            return sim_run;
         }

         protected void LoadLogFile()
         {
            try
            {
               if (!data.log.Load(data.logFileName))
                  throw new Exception("Was not possible to load the simulation LOG file: " + data.logFileName.FullPath); 
               
               if (data.log.Count > 0)
               {
                  ConfigNode lastEntry = data.log[data.log.Count - 1];
                  if (!lastEntry["run.status"].AsBool())
                  {
                     if (data.RestartFailedRun)
                     {
                        data.sim.Start = lastEntry["sim.start"].AsDateTime(data.dateFormat);
                        data.sim.SimLenght = lastEntry["sim.lenght"].AsDouble();
                        data.simID = lastEntry["sim.id"].AsInt();
                     }
                     else
                        throw new Exception("It is not possible to run a new simulation because the last simulation has FAILED."); ;
                  }
                  else
                  {
                     data.sim.Start = lastEntry["sim.end"].AsDateTime(data.dateFormat);
                     data.simID = lastEntry["sim.id"].AsInt() + 1;
                  }
               }
            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.LoadLogFile", ex);
            }
         }

         protected void NextSimSetup()
         {
            try
            {
               data.sim.Start = data.sim.End;
               data.simID++;
               data.maxIterations--;
            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.NextSimSetup", ex);
            }
         }

         protected void SetupRun()
         {
            try
            {
               if (useEndDate && simEnd < data.sim.End)
                  data.sim.End = simEnd;

               if (data.changeTemplates && data.simID > 1)
                  data.sim.TemplateFilesList = data.templatesContinuation;

            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.SetupRun", ex);
            }
         }

         protected bool Run()
         {
            try
            {
               data.runStart = DateTime.Now;
               data.LastOperationResult = data.sim.Run();
               data.runEnd = DateTime.Now;
               return data.LastOperationResult;
            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.Run", ex);
            }
         }

         protected void AddNewEntryToLog(bool success)
         {
            try
            {
               ConfigNode nodeData = new ConfigNode("log.entry");

               nodeData["run.start"] = new KeywordData(data.runStart.ToString(data.dateFormat));
               nodeData["run.end"] = new KeywordData(data.runEnd.ToString(data.dateFormat));
               nodeData["sim.start"] = new KeywordData(data.sim.Start.ToString(data.dateFormat));
               nodeData["sim.end"] = new KeywordData(data.sim.End.ToString(data.dateFormat));
               nodeData["sim.lenght"] = new KeywordData(data.sim.SimLenght.ToString());
               nodeData["run.status"] = new KeywordData(success.ToString());
               nodeData["sim.id"] = new KeywordData(data.simID.ToString());

               data.log.AddEntry(nodeData);
            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.AddNewEntryToLog", ex);
            }
         }

         #region USER DEFINED FUNCTIONS

         protected bool OnPreProcessing()
         {
            if (data.userInterface != null)
               if (!data.userInterface.OnPreProcessing(data))
               {
                  Exception ex = data.userInterface.ExceptionRaised();
                  if (ex != null)
                     throw ex;
                  throw new Exception("MohidRunEngine.OnPreProcessing Unknown error");
               }

            return true;
         }
         protected void OnStart()
         {
            if (data.userInterface != null)
               if (!data.userInterface.OnStart(data))
               {
                  Exception ex = data.userInterface.ExceptionRaised();
                  if (ex != null)
                     throw ex;
                  throw new Exception("MohidRunEngine.OnStart Unknown error");
               }
         }
         protected void AfterInit()
         {
            if (data.userInterface != null)
               if (!data.userInterface.AfterInitialization(data))
               {
                  Exception ex = data.userInterface.ExceptionRaised();
                  if (ex != null)
                     throw ex;
                  throw new Exception("MohidRunEngine.AfterInit Unknown error");
               }
         }
         protected void OnSimStart()
         {
            if (data.userInterface != null)
               if (!data.userInterface.OnSimStart(data))
               {
                  Exception ex = data.userInterface.ExceptionRaised();
                  if (ex != null)
                     throw ex;
                  throw new Exception("MohidRunEngine.OnSimStart Unknown error");
               }
         }
         protected void OnSimEnd()
         {
            if (data.userInterface != null)
               if (!data.userInterface.OnSimEnd(data))
               {
                  Exception ex = data.userInterface.ExceptionRaised();
                  if (ex != null)
                     throw ex;
                  throw new Exception("MohidRunEngine.OnSimEnd Unknown error");
               }
         }
         protected void OnEnd()
         {
            if (data.userInterface != null)
               if (!data.userInterface.OnEnd(data))
               {
                  Exception ex = data.userInterface.ExceptionRaised();
                  if (ex != null)
                     throw ex;
                  throw new Exception("MohidRunEngine.OnEnd Unknown error");
               }
         }
         protected void OnException()
         {
            if (data.userInterface != null)
               data.userInterface.OnException(data);
         }
         protected bool OnRunFail()
         {
            if (data.userInterface != null)
               return data.userInterface.OnRunFail(data);
            return false;
         }

         #endregion USER DEFINED FUNCTIONS

         #endregion ENGINE

         #region CONFIG

         protected void LoadConfig(CmdArgs args)
         {
            try
            {
               data.cfg = new Config();
               Config cfg = data.cfg;

               string configFile;
               if (args.HasParameter("simcfg"))
                  configFile = args.Parameter("simcfg");
               else
                  configFile = "sim.cfg";

               if (args.HasParameter("max.iter"))
                  data.maxIterations = int.Parse(args.Parameter("max.iter"));
               else
                  data.maxIterations = -1;

               cfg.ConfigFile.FullPath = configFile;
               if (!cfg.Load())
                  throw new Exception("Was not possible to load the configuration file '" + configFile + "'. " + cfg.ExceptionMessage);

               ConfigNode root = cfg.Root;

               data.sim.SimDirectory = root["sim.folder", "sim"].AsFilePath();
               data.logFileName = root["log.file", data.sim.SimDirectory.Path + "sim.log"].AsFileName();

               data.RestartFailedRun = root["restart.failed.run", true].AsBool();

               data.sim.Start = root["sim.start"].AsDateTime(data.dateFormat);
               data.sim.SimLenght = root["sim.lenght", 14].AsDouble();
               data.simID = 1;

               data.sim.CheckRun = root["check.run", true].AsBool();
               data.sim.Verbose = root["verbose", true].AsBool();
               data.sim.Wait = root["wait", true].AsBool();
               data.sim.SuccessString = root["check.this", "successfully terminated"].AsString();

               data.sim.SetupRunPeriod = root["setup.run.period", false].AsBool();

               if (data.sim.SetupRunPeriod)
               {
                  data.sim.EndTAG = root["sim.end.tag", "<<end>>"].AsString();
                  data.sim.StartTAG = root["sim.start.tag", "<<start>>"].AsString();
               }

               data.sim.DataDirectory = root["data.folder", data.sim.SimDirectory.Path + "data"].AsFilePath();
               data.sim.WorkingDirectory = root["working.folder", data.sim.SimDirectory.Path + "exe"].AsFilePath();
               data.resFolder = root["results.folder", data.sim.SimDirectory.Path + "res"].AsFilePath();
               data.storeFolder = root["store.folder", data.sim.SimDirectory.Path + "store"].AsFilePath();
               data.oldFolder = root["old.folder", data.sim.SimDirectory.Path + "old"].AsFilePath();

               data.sim.SaveOutput = root["save.output", true].AsBool();
               if (data.sim.SaveOutput)
               {
                  data.sim.OutputFile = new FileName(data.resFolder.Path + root["output.file", "result.txt"].AsString());
               }

               data.sim.Executable = root["mohid.executable", "mohid.exe"].AsFileName();
               data.sim.CreateInputFiles = root["use.templates", false].AsBool();
               if (root.NodeData.ContainsKey("sim.end"))
               {
                  data.useEndOfSimulation = true;
                  data.endOfSimulation = root["sim.end"].AsDateTime(data.dateFormat);
               }
               else
                  data.useEndOfSimulation = false;

               if (!data.useEndOfSimulation && data.maxIterations < 1 && !root["infinite.run", false].AsBool())
                  throw new Exception("'sim.end' keyword and 'max.iter' parameter are missing and 'infinit.run' keyword is missing or set to False.");

               if (data.sim.SetupRunPeriod && !data.sim.CreateInputFiles)
                  throw new Exception("If 'setup.run.period' is set to True, 'use.templates' also must be set to True.");

               if (data.sim.CreateInputFiles)
               {
                  InputFileTemplate newTemplate;
                  List<ConfigNode> itfList = root.ChildNodes.FindAll(FindFirstTemplateInfoBlocks);
                  foreach (ConfigNode ticn in itfList)
                  {
                     newTemplate = new InputFileTemplate(ticn["file"].AsFileName().FullPath,
                                                         (InputFileTemplateType)Enum.Parse(typeof(InputFileTemplateType), ticn["type", "data"].AsString(), true));
                     data.templatesStart.Add(newTemplate);
                  }
                  data.sim.TemplateFilesList = data.templatesStart;

                  data.changeTemplates = root["change.templates", true].AsBool();
                  if (data.changeTemplates)
                  {
                     itfList = root.ChildNodes.FindAll(FindNextTemplateInfoBlocks);
                     foreach (ConfigNode ticn in itfList)
                     {
                        newTemplate = new InputFileTemplate(ticn["file"].AsFileName().FullPath,
                                                            (InputFileTemplateType)Enum.Parse(typeof(InputFileTemplateType), ticn["type", "data"].AsString(), true));
                        data.templatesContinuation.Add(newTemplate);
                     }
                  }
               }
            }
            catch (Exception ex)
            {
               throw new Exception("MohidRunEngine.LoadConfig", ex);
            }
         }

         protected bool FindFirstTemplateInfoBlocks(ConfigNode toMatch)
         {
            if (toMatch.Name == "cold.start.template.file")
               return true;
            return false;
         }
         protected bool FindNextTemplateInfoBlocks(ConfigNode toMatch)
         {
            if (toMatch.Name == "hot.start.template.file")
               return true;
            return false;
         }

         #endregion CONFIG
      }
   }
}

