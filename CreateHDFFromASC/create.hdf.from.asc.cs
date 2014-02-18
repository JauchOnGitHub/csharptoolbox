//DLLNAME: JauchToolboxCore
//DLLNAME: System

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid;
using Mohid.Core;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.Software;
using Mohid.Files;

namespace JauchTools
{
   public class CreateHDFFromASCToolException : Exception
   {
      public CreateHDFFromASCToolException() 
         : base () 
      {
      }

      public CreateHDFFromASCToolException(string message)
         : base(message)
      {
      }

      public CreateHDFFromASCToolException(string message, Exception innerException)
         : base(message, innerException)
      {
      }      
   }

   public class CreateHDFFromASC : IMohidTask
   {
      protected FilePath fWorkingDir; //General working folder
      protected Exception fLastException;

      public CreateHDFFromASC()
      {
         Reset();
      }

      public bool Run(ConfigNode cfg)
      {
         fLastException = null;

         Console.WriteLine("Starting CreateHDFFromASC...");

         fWorkingDir = cfg["working.dir", AppDomain.CurrentDomain.BaseDirectory].AsFilePath();

         try
         {
            if (cfg["from.asc.to.xyz", false].AsBool())
               ConvertToXYZ(cfg);

            if (cfg["from.xyz.to.mgd", false].AsBool())
               ConvertToMGD(cfg);

            if (cfg["from.mgd.to.hdf", false].AsBool())
               ConvertToHDF(cfg);

            if (cfg["glue.hdfs", false].AsBool())
               GlueHDFs(cfg);

            Console.WriteLine("CreateHDFFromASC finished successfully...");
            return true;
         }
         catch (Exception ex)
         {
            fLastException = ex;
            return false;
         }
      }

      protected void ConvertToXYZ(ConfigNode cfg)
      {
         Console.WriteLine("Converting ASC to XYZ...");

         List<FileInfo> list_of_files = new List<FileInfo>();
         Dictionary<string, string> replace_list = new Dictionary<string,string>();
         FileName template = cfg["conv.to.xyz.template"].AsFileName();
         string working_dir = cfg["xyz.working.dir", fWorkingDir.Path].AsFilePath().Path; 
         string config = template.FullName.Replace("template", "dat");
         string output;
         ExternalApp tool = new ExternalApp();

         try
         {
            FileTools.FindFiles(ref list_of_files, cfg["asc.path"].AsFilePath(), "*." + cfg["asc.extension", "asc"].AsString(), true, "", System.IO.SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in list_of_files)
            {
               Console.WriteLine("    -> {0}", file.FileName.FullName);
               output = cfg["xyz.path"].AsFilePath().Path + file.FileName.Name + ".xyz";

               if (!cfg["rewrite.xyz", false].AsBool())
               {
                  if (System.IO.File.Exists(output))
                     continue;
               }

               replace_list["{{input.file}}"] = file.FileName.FullPath;
               replace_list["{{output.file}}"] = output;

               TextFile.Replace(template.FullPath, working_dir + config, ref replace_list, false);



               tool.Arguments = "";
               tool.WorkingDirectory = working_dir;
               tool.Executable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg["conv.to.xyz.exe"].AsString());
               tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
               tool.TextToCheck = "Conversion terminated successfully.";
               tool.SearchTextOrder = SearchTextOrder.FROMEND;
               tool.Wait = true;

               bool result = tool.Run();
               if (!result)
               {
                  if (tool.Exception != null)
                  {
                     Console.WriteLine("ConvertToXYZ failed: {0}", tool.Exception.Message);
                     throw new CreateHDFFromASCToolException(tool.Exception.Message, tool.Exception);
                  }
                  else
                  {
                     Console.WriteLine("ConvertToXYZ failed.");
                     throw new CreateHDFFromASCToolException("ConvertToXYZ failed.");
                  }
               }
            }
         }
         catch (CreateHDFFromASCToolException)
         {
            throw;
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            throw;
         }


      }

      protected void ConvertToMGD(ConfigNode cfg)
      {
         Console.WriteLine("Converting XYZ to MGD...");

         List<FileInfo> list_of_files = new List<FileInfo>();
         Dictionary<string, string> replace_list = new Dictionary<string, string>();
         FileName template = cfg["conv.to.mgd.template"].AsFileName();
         string working_dir = cfg["mgd.working.dir", fWorkingDir.Path].AsFilePath().Path; 
         string config = template.FullName.Replace("template", "dat");
         string output;

         FileTools.FindFiles(ref list_of_files, cfg["xyz.path"].AsFilePath(), "*.xyz", true, "", System.IO.SearchOption.TopDirectoryOnly);
         foreach (FileInfo file in list_of_files)
         {
            Console.WriteLine("    -> {0}", file.FileName.FullName);
            output = cfg["mgd.path"].AsFilePath().Path + file.FileName.Name + ".dat"; ;
            if (!cfg["rewrite.mgd", false].AsBool())
            {
               if (System.IO.File.Exists(output))
                  continue;
            }

            replace_list["{{input.file}}"] = file.FileName.FullPath;
            replace_list["{{output.file}}"] = output;
            replace_list["{{no.data.point}}"] = cfg["no.data.point", "-99"].AsString();

            TextFile.Replace(template.FullPath, working_dir + config, ref replace_list, false);

            ExternalApp tool = new ExternalApp();

            tool.Arguments = "";
            tool.WorkingDirectory = working_dir;
            tool.Executable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg["conv.to.mgd.exe"].AsString());
            tool.CheckSuccessMethod = CheckSuccessMethod.DEFAULTOUTPUT;
            tool.TextToCheck = "successfully terminated";
            tool.SearchTextOrder = SearchTextOrder.FROMEND;
            tool.Wait = true;

            bool result = tool.Run();
            if (!result)
            {
               if (tool.Exception != null)
               {
                  Console.WriteLine("ConvertToMGD failed: {0}", tool.Exception.Message);
                  throw tool.Exception;
               }
               else
               {
                  Console.WriteLine("ConvertToMGD failed without explanation.");
                  throw new Exception("ConvertToMGD failed.");
               }
            }
         }
      }

      protected void ConvertToHDF(ConfigNode cfg)
      {
      }

      protected void GlueHDFs(ConfigNode cfg)
      {
      }

      public Exception LastException
      {
         get { return fLastException; }
      }

      public bool Reset()
      {
         fLastException = null;
         return true;
      }
   }
}
