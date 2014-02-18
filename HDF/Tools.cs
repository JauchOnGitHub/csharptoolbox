using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Software;
using Mohid.Files;


namespace Mohid
{
   namespace HDF
   {
      public class HDFParameter
      {
         public string Group, Property;
         public string GroupV3, PropertyV3;

         public HDFParameter()
         {
            Group = "";
            Property = "";
            GroupV3 = "";
            PropertyV3 = "";
         }
         public HDFParameter(string group, string property)
         {
            Group = group;
            Property = property;
            GroupV3 = "";
            PropertyV3 = "";
         }
      }

      public class HDFExtractException : Exception
      {
         public HDFExtractException() : base() { }
         public HDFExtractException(string message) : base(message) { }
         public HDFExtractException(string message, Exception innerException) : base(message, innerException) { }
         public HDFExtractException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
      }

      public class HDFToolsBase
      {
         protected string fWorkingDirectory;
         protected string fAppPath;
         public string AppName;
         public string Arguments;
         public bool ThrowExceptionOnError;
         public CheckSuccessMethod CheckSuccessMethod;
         public SearchTextOrder TextOrder;
         public string TextToCheck;
         public bool Wait;
         public bool Verbose;
         public bool UseShell;

         public string WorkingDirectory
         {
            get
            {
               return fWorkingDirectory;
            }
            set
            {
               if (value != null)
               {
                  value = value.Trim();
                  if (!string.IsNullOrEmpty(value))
                  {
                     fWorkingDirectory = value;
                     if (!fWorkingDirectory.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                        fWorkingDirectory += System.IO.Path.DirectorySeparatorChar.ToString();
                  }
               }
            }
         }
         public string AppPath
         {
            get
            {
               return fAppPath;
            }
            set
            {
               if (value != null)
               {
                  value = value.Trim();
                  if (!string.IsNullOrEmpty(value))
                  {
                     fAppPath = value;
                     if (!fAppPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                        fAppPath += System.IO.Path.DirectorySeparatorChar.ToString();
                  }
               }
            }
         }

         public virtual void Reset()
         {
            fWorkingDirectory = "." + System.IO.Path.DirectorySeparatorChar;
            fAppPath = "." + System.IO.Path.DirectorySeparatorChar;
            AppName = "app.exe";
            ThrowExceptionOnError = false;
            CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            TextOrder = SearchTextOrder.FROMEND;
            TextToCheck = "successfull";
            Arguments = "";
            Wait = true;
            Verbose = false;
            UseShell = false;
         }

         public HDFToolsBase()
         {
            Reset();
         }

         public int ThrowException(int code)
         {
            if (ThrowExceptionOnError)
               throw new HDFExtractException("Error code: " + code.ToString());
            return code;
         }
         public int ThrowException(int code, string message)
         {
            if (ThrowExceptionOnError)
               throw new HDFExtractException(message);
            return code;
         }
         public bool ThrowException(string message)
         {
            if (ThrowExceptionOnError)
               throw new HDFExtractException(message);
            return false;
         }
         public bool ExecuteApp()
         {
            ExternalApp app;
            bool result;

            try
            {
               app = new ExternalApp();

               app.Executable = fAppPath + AppName;
               if (!string.IsNullOrWhiteSpace(Arguments))
                  app.Arguments = Arguments;
               app.WorkingDirectory = fWorkingDirectory;
               app.CheckSuccessMethod = CheckSuccessMethod;
               app.TextToCheck = TextToCheck;
               app.SearchTextOrder = TextOrder;
               app.Verbose = Verbose;
               app.UseShell = UseShell;
               app.Wait = true;
            }
            catch (Exception ex)
            {
               return ThrowException(ex.Message);
            }

            result = app.Run();
            if (!result)
            {
               //Console.WriteLine(app.Options.SaveDefaultOutput.ToString());
               //foreach(string line in ExternalApp.DefaultOutput)
               //   Console.WriteLine(line);
               return ThrowException("Application '" + app.Executable + "' run Failed.");
            }
            return true;
         }
      }

      public class HDFExtract : HDFToolsBase
      {
         public string fExtractorInputFileName;
         public string fGlobalInputFileName;
         public string fInput, fOutput;
         public DateTime Start, End;
         public bool ExtractWindow;
         public int WindowILB, WindowIUB, WindowJLB, WindowJUB;
         public bool ExtractSpecificLayers;
         public int LayerKLB, LayerKUB;
         public bool ExtractByInterval;
         public int DTInterval; //In seconds
         public bool ConvertV3ToV4;
         public List<HDFParameter> Parameters;

         public string ExtractorInputFileName
         {
            get
            {
               return fExtractorInputFileName;
            }
            set
            {
               fExtractorInputFileName = value.Trim();
            }
         }
         public string GlobalInputFileName
         {
            get
            {
               return fGlobalInputFileName;
            }
            set
            {
               fGlobalInputFileName = value.Trim();
            }
         }
         public string Input
         {
            get
            {
               return fInput;
            }
            set
            {
               fInput = value.Trim();
            }
         }
         public string Output
         {
            get
            {
               return fOutput;
            }
            set
            {
               fOutput = value.Trim();
            }
         }

         public HDFExtract()
         {
            ThrowExceptionOnError = false;
            AppName = "hdfextractor.exe";
            GlobalInputFileName = "nomfich.dat";
            ExtractorInputFileName = "hdfextractor.dat";
            Input = "";
            Output = "";
            Start = DateTime.Now;
            End = DateTime.Now;
            ExtractWindow = false;
            WindowILB = 0;
            WindowIUB = 0;
            WindowJLB = 0;
            WindowJUB = 0;
            ExtractSpecificLayers = false;
            LayerKLB = 0;
            LayerKUB = 0;
            ExtractByInterval = false;
            DTInterval = 0;
            ConvertV3ToV4 = false;
            Parameters = new List<HDFParameter>();
         }
         public int Extract()
         {
            if (Parameters.Count <= 0)
               return ThrowException(-1);
            if (string.IsNullOrEmpty(Input))
               return ThrowException(-2);
            if (string.IsNullOrEmpty(Output))
               return ThrowException(-3);
            if (string.IsNullOrEmpty(fAppPath))
               return ThrowException(-4);
            if (string.IsNullOrEmpty(fExtractorInputFileName))
               return ThrowException(-5);
            if (string.IsNullOrEmpty(fGlobalInputFileName))
               return ThrowException(-6);
            if (string.IsNullOrEmpty(AppName))
               return ThrowException(-7);

            if (string.IsNullOrEmpty(fWorkingDirectory))
               fWorkingDirectory = fAppPath;

            try
            {
               TextFile nomfich = new TextFile();
               TextFile datafile = new TextFile();

               nomfich.File = new FileName(fWorkingDirectory + fGlobalInputFileName);
               nomfich.OpenNewToWrite(System.IO.FileShare.None);
               nomfich.WriteLine("IN_MODEL : " + fExtractorInputFileName);
               nomfich.Close();

               datafile.File = new FileName(fWorkingDirectory + fExtractorInputFileName);
               datafile.OpenNewToWrite(System.IO.FileShare.None);
               datafile.WriteLine("FILENAME         : " + fInput);
               datafile.WriteLine("OUTPUTFILENAME   : " + fOutput);
               datafile.WriteLine("START_TIME       : " + Start.ToString("yyyy MM dd HH mm ss"));
               datafile.WriteLine("END_TIME         : " + End.ToString("yyyy MM dd HH mm ss"));
               if (ExtractWindow)
               {
                  datafile.WriteLine("XY_WINDOW_OUTPUT : 1");
                  datafile.WriteLine("XY_WINDOW_LIMITS : " + WindowILB.ToString() + " " + WindowJLB.ToString() + " " + WindowIUB.ToString() + " " + WindowJUB.ToString());
               }
               else
                  datafile.WriteLine("XY_WINDOW_OUTPUT : 0");
               if (ExtractSpecificLayers)
               {
                  datafile.WriteLine("LAYERS_OUTPUT    : 1");
                  datafile.WriteLine("LAYERS_MIN_MAX   : " + LayerKLB.ToString() + " " + LayerKUB.ToString());
               }
               else
                  datafile.WriteLine("LAYERS_OUTPUT    : 0");
               if (ExtractByInterval)
               {
                  datafile.WriteLine("INTERVAL         : 1");
                  datafile.WriteLine("DT_INTERVAL      : " + DTInterval.ToString());
               }
               else
                  datafile.WriteLine("INTERVAL         : 0");
               if (ConvertV3ToV4)
                  datafile.WriteLine("CONVERT_V3_TO_V4 : 1");
               else
                  datafile.WriteLine("CONVERT_V3_TO_V4 : 0");
               foreach (HDFParameter p in Parameters)
               {
                  datafile.WriteLine("<BeginParameter>");
                  if (ConvertV3ToV4)
                  {
                     datafile.WriteLine("HDF_GROUP_V3     :" + p.GroupV3);
                     datafile.WriteLine("PROPERTY_V3      :" + p.PropertyV3);
                  }
                  datafile.WriteLine("HDF_GROUP        : " + p.Group);
                  datafile.WriteLine("PROPERTY         : " + p.Property);
                  datafile.WriteLine("<EndParameter>");
               }
               datafile.Close();
            }
            catch (Exception ex)
            {
               ThrowException(-8, ex.Message);
            }

            TextToCheck = "Operation was successfull";
            if (!ExecuteApp())
               return -9;

            return 0;
         }
      }

      public class HDFGlue : HDFToolsBase
      {
         protected string fOutput;
         protected string fOutwatch;
         public string GlueInpuFile;
         public bool Is3DFile;
         public string BaseGroup;
         public string TimeGroup;
         protected List<string> filesToGlue;
         protected List<FileName> filesToGlue2;

         public List<string> FilesToGlue
         {
            get { return filesToGlue; }
            set { filesToGlue = value; filesToGlue2 = null; }         
         }
         public List<FileName> FilesToGlue2
         {
            get { return filesToGlue2; }
            set { filesToGlue2 = value; filesToGlue = null; }
         }

         public string Output
         {
            get
            {
               return fOutput;
            }
            set
            {
               fOutput = value.Trim();
            }
         }
         public string Outwatch
         {
            get
            {
               return fOutwatch;
            }
            set
            {
               fOutwatch = value.Trim();
            }
         }

         public override void Reset()
         {
            base.Reset();

            AppName = "ConvertToHdf5.exe";
            GlueInpuFile = "converttohdf5action.dat";
            Is3DFile = false;
            BaseGroup = "Results";
            TimeGroup = "Time";
            filesToGlue = null;
            filesToGlue2 = null;
         }

         public HDFGlue()
         {
            Reset();
            filesToGlue = new List<string>();
         }

         public HDFGlue(List<string> filesToGlue)
         {
            Reset();
            this.filesToGlue = filesToGlue;
         }

         public HDFGlue(List<FileName> filesToGlue)
         {
            Reset();
            this.filesToGlue2 = filesToGlue;
         }

         public int Glue()
         {
            if (filesToGlue != null && filesToGlue.Count <= 0)
               return ThrowException(-3, "No files to glue.");

            if (filesToGlue2 != null && filesToGlue2.Count <= 0)
               return ThrowException(-3, "No files to glue."); 

            try
            {
               TextFile datafile = new TextFile();

               if (Verbose)
                  Console.WriteLine("Creating input file: {0}", fWorkingDirectory + GlueInpuFile);

               datafile.File = new FileName(fWorkingDirectory + GlueInpuFile);               

               datafile.OpenNewToWrite(System.IO.FileShare.None);

               if (!String.IsNullOrEmpty(fOutwatch) && fOutwatch.Trim() != "")
                  datafile.WriteLine("OUTWATCH         : " + fOutwatch.Trim());
               datafile.WriteLine("<begin_file>");
               datafile.WriteLine("  ACTION           : GLUES HDF5 FILES");
               datafile.WriteLine("  OUTPUTFILENAME   : " + fOutput);
               if (Is3DFile)
                  datafile.WriteLine("  3D_FILE          : 1");
               else
                  datafile.WriteLine("  3D_FILE          : 0");
               datafile.WriteLine("  BASE_GROUP       : " + BaseGroup);
               datafile.WriteLine("  TIME_GROUP       : " + TimeGroup);
               datafile.WriteLine("  <<begin_list>>");
               if (filesToGlue != null)
               {
                  foreach (string file in filesToGlue)
                     datafile.WriteLine("      " + file);
               }
               else if (filesToGlue2 != null)
               {
                  foreach (FileName file in filesToGlue2)
                     datafile.WriteLine("      " + file.FullPath);
               }
               datafile.WriteLine("  <<end_list>>");
               datafile.WriteLine("<end_file>");
               datafile.Close();
            }
            catch (Exception ex)
            {
               return ThrowException(-1, ex.Message);
            }

            TextToCheck = "successfully terminated";
            if (!ExecuteApp())
               return ThrowException(-2);            

            return 0;
         }
      }
   }
}
