using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Files;
using Mohid.Software;
using Mohid.Core;

namespace Mohid
{
   namespace Simulation
   {
      public enum InputFileTemplateType
      {
         UNKNOWN,
         DATA,
         MODEL,
         NOMFICH
      }

      public enum MohidSimStatus
      {
         UNKNOWN,
         OK,
         PRE_PROCESSING_FAILED,
         AT_START_FAILED,
         AT_END_FAILED,
         RUN_FAILED,
         EXCEPTION
      }

      public class InputFileTemplate
      {
         protected FileName file;
         protected string output_file_name;
         protected InputFileTemplateType type;
         protected Dictionary<string, string> replaceList;

         public string OutputName
         {
            get 
            {
               if (string.IsNullOrWhiteSpace(output_file_name))
                  return Name;
               return output_file_name; 
            }
            set { output_file_name = value; }
         }

         public string Name
         {
            get { return file.Name; }
            set { file.Name = value; }
         }
         public string FullName
         {
            get { return file.FullName; }
            set { file.FullName = value; }
         }
         public string Path
         {
            get { return file.Path; }
            set { file.Path = value; }
         }
         public string FullPath
         {
            get { return file.FullPath; }
            set { file.FullPath = value; }
         }

         public InputFileTemplateType Type
         {
            get { return type; }
            set { type = value; }
         }

         protected void Init()
         {
            output_file_name = "";
            file = new FileName();
            type = InputFileTemplateType.UNKNOWN;
            replaceList = new Dictionary<string,string>();
         }

         public InputFileTemplate()
         {
            Init();
         }

         public InputFileTemplate(string fullpath, InputFileTemplateType type)
         {
            Init();
            file.FullPath = fullpath;
            this.type = type;
         }

         public string this[string key]
         {
            get
            {
               return replaceList[key];
            }
            set
            {
               replaceList[key] = value;
            }
         }

         public Dictionary<string, string> ReplaceList
         {
            get { return replaceList; }
         }
      }

      public class MohidSimulation
      {
         #region FIELDS / PROPERTIES

         protected DateTime start, 
                            end;

         protected double simLenght;

         protected MohidSimStatus status;
         protected string statusMessage;

         protected List<InputFileTemplate> templateFiles;

         public Run PreProcessing { get; set; }
         public Run OnBegin { get; set; }
         public Run OnEnd { get; set; }
         //public IRun PreProcessing { get; set; }
         //public IRun AtStart { get; set; }
         //public IRun AtEnd { get; set; }

         public MohidSimStatus Status { get { return status; } }
         public string StatusMessage { get { return statusMessage; } }

         public double SimLenght
         {
            get
            {               
               return simLenght;
            }
            set
            {
               if (value <= 0)
                  throw new Exception("Invalid simulation lenght (days): '" + value.ToString() + "'");
               simLenght = value;
               end = start.AddDays(simLenght);               
            }
         }
         public DateTime Start
         {
            get
            {
               return start;
            }
            set
            {
               start = value;
               end = start.AddDays(simLenght);
            }
         }
         public DateTime End
         {
            get
            {
               return end;
            }
            set
            {
               if (value < start)
                  throw new Exception("Simulation END (" + value.ToString() + ") must be greater than START (" + start.ToString() + ")");
               end = value;
               simLenght = (end - start).Days;
            }
         }
         public string StartTAG { get; set; }
         public string EndTAG { get; set; }
         public string SuccessString { get; set; }
         public bool CheckRun { get; set; }
         public bool Verbose { get; set; }
         public bool SaveOutput { get; set; }
         public bool SetupRunPeriod { get; set; }
         public bool CreateInputFiles { get; set; }
         public bool Wait { get; set; }
         public FilePath SimDirectory { get; set; }
         public FilePath DataDirectory { get; set; }
         public FilePath WorkingDirectory { get; set; }
         public FileName OutputFile { get; set; }
         public FileName Executable { get; set; }

         #endregion FIELDS / PROPERTIES

         #region TEMPLATES MANAGEMENT

         public void AddTemplate(InputFileTemplate file)
         {
            templateFiles.Add(file);
         }
         public InputFileTemplate AddTemplate()
         {
            InputFileTemplate newFile = new InputFileTemplate();
            AddTemplate(newFile);
            return newFile;
         }
         public InputFileTemplate AddTemplate(string path, InputFileTemplateType type)
         {
            InputFileTemplate newFile = new InputFileTemplate(path, type);
            AddTemplate(newFile);
            return newFile;
         }
         public InputFileTemplate GetTemplate(int index)
         {
            return templateFiles[index];
         }
         public void ClearTemplates() { templateFiles.Clear(); }
         public List<InputFileTemplate> TemplateFilesList { get { return templateFiles; } set { templateFiles = value; } }

         #endregion TEMPLATES MANAGEMENT
         
         #region CONSTRUCT

         protected virtual void InitOptions()
         {
            PreProcessing = null;
            OnBegin = null;
            OnEnd = null;
            //AtStart = null;
            //AtEnd = null;
            CreateInputFiles = true;
            SetupRunPeriod = true;
            simLenght = 14.0; //14 days
            start = DateTime.Today;
            end = start.AddDays(simLenght);
            StartTAG = "<<start>>";
            EndTAG = "<<end>>";
            CheckRun = true;
            SuccessString = "successfully terminated";
            SimDirectory = new FilePath();
            DataDirectory = new FilePath("data");
            SaveOutput = true;
            WorkingDirectory = new FilePath();
            OutputFile = new FileName("output.txt");
            Executable = new FileName(SimDirectory.Path + "mohid.exe");
            Verbose = false;
            templateFiles = new List<InputFileTemplate>();
         }

         protected virtual void Init()
         {
            InitOptions();
         }

         public MohidSimulation()
         {
            Init();
         }

         #endregion CONSTRUCT

         #region ENGINE

         public virtual bool Run()
         {
            try
            {
               if (CreateInputFiles)
               {
                  if (SetupRunPeriod)
                     SetupRunPeriodOnModelFile();

                  CreateInputDataFiles();
               }

               //if (PreProcessing != null && !PreProcessing.Run())
               if (PreProcessing != null && !PreProcessing())
               {
                  SetStatus(MohidSimStatus.PRE_PROCESSING_FAILED);
                  return false;
               }

               if (!RunMohid())
               {
                  SetStatus(MohidSimStatus.RUN_FAILED);
                  return false;
               }
               else
               {
                  SetStatus(MohidSimStatus.OK);
                  return true;
               }
            }
            catch (Exception ex)
            {
               SetStatus(MohidSimStatus.EXCEPTION, ex.Message);
               return false;
            }
         }

         public virtual bool Run(int iterations)
         {
            try
            {
               for (int i = 0; i < iterations; i++)
               {
                  //if (AtStart != null && !AtStart.Run())
                  if (OnBegin != null && !OnBegin())
                  {
                     SetStatus(MohidSimStatus.AT_START_FAILED, "");
                     return false;
                  }

                  if (!Run())
                     return false;

                  //if (AtEnd != null && !AtEnd.Run())
                  if (OnEnd != null && !OnEnd())
                  {
                     SetStatus(MohidSimStatus.AT_END_FAILED, "");
                     return false;
                  }
               }

               return true;
            }
            catch (Exception ex)
            {
               SetStatus(MohidSimStatus.EXCEPTION, ex.Message);
               return false;
            }
         }

         public virtual bool Run(DateTime toThis, bool doResidualTimeSim = true)
         {
            bool result = true;
            try
            {
               int iterations = 0;
               double totalDays;

               totalDays = (toThis - start).TotalDays;
               iterations = (int)(totalDays / simLenght);

               if (!Run(iterations))
                  return false;

               if (doResidualTimeSim)
               {
                  double origLenght = simLenght, 
                         residualTime;
                  residualTime = (int)((totalDays / simLenght) - (double)(iterations));

                  simLenght = residualTime;
                  if (!Run(1))
                     result = false;
                  simLenght = origLenght;
               }

               return result;
            }
            catch (Exception ex)
            {
               SetStatus(MohidSimStatus.EXCEPTION, ex.Message);
               return false;
            }
         }

         protected virtual void CreateInputDataFiles()
         {
            Dictionary<string, string> rl; 
            foreach(InputFileTemplate file in templateFiles)
            {              
               switch (file.Type)
               {
                  case Simulation.InputFileTemplateType.MODEL:
                  case Simulation.InputFileTemplateType.DATA:
                     rl = file.ReplaceList;
                     TextFile.Replace(file.FullPath, DataDirectory.Path + file.OutputName + ".dat", ref rl);
                     break;
                  default:
                     rl = file.ReplaceList;
                     TextFile.Replace(file.FullPath, WorkingDirectory.Path + file.OutputName + ".dat", ref rl);
                     break;
               }
            }
         }

         protected virtual void SetupRunPeriodOnModelFile()
         {
            foreach(InputFileTemplate file in templateFiles)
            {
               if (file.Type == InputFileTemplateType.MODEL)
               {
                  file[StartTAG] = start.ToString("yyyy M d H m s");
                  file[EndTAG] = end.ToString("yyyy M d H m s");

                  break;
               }
            }
         }

         protected virtual bool RunMohid()
         {
            ExternalApp app = new ExternalApp();
        
            app.Executable = Executable.FullPath;
            app.WorkingDirectory = WorkingDirectory.Path;
            app.Verbose = Verbose;
            if (CheckRun)
            {
               app.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
               app.TextToCheck = SuccessString;
               app.SearchTextOrder = SearchTextOrder.FROMEND;
            }
            else
            {
               app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
            }
            app.Wait = Wait;
   
            return app.Run();
         }

         #endregion ENGINE

         #region AUX

         protected void SetStatus(MohidSimStatus status, string message = "")
         {
            this.status = status;
            this.statusMessage = message;
         }

         #endregion AUX
      }
   }
}