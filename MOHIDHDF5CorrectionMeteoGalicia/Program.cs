using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Configuration;
using Mohid.Files;
using Mohid.Software;

namespace MOHIDHDF5CorrectionMeteoGalicia
{
   class Program
   {
      static void Main(string[] args)
      {
         Config cfg = new Config("corr.cfg");
         cfg.Load();

         FilePath processor_path = cfg.Root["processor.path"].AsFilePath();
         FilePath output_path = cfg.Root["output.path"].AsFilePath();
         FilePath hdfs_path = cfg.Root["hdf.path"].AsFilePath();

         System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(hdfs_path.Path);
         System.IO.FileInfo[] aryFi = di.GetFiles("*.hdf5", System.IO.SearchOption.TopDirectoryOnly);

         Dictionary<string, string> info = new Dictionary<string,string>();

         ExternalApp app = new ExternalApp();
         app.Arguments = "--cfg task.cfg";
         app.CheckSuccessMethod = CheckSuccessMethod.DONOTCHECK;
         app.Executable = processor_path.Path + "MohidHDF5Processor.exe";
         app.UseShell = false;
         app.Verbose = false;
         app.Wait = true;
         app.WorkingDirectory = processor_path.Path;

         int count = 1;

         foreach (System.IO.FileInfo fi in aryFi)
         {
            Console.Write("{1}: Processing {0}...", fi.Name, count);

            info["<<input>>"] = fi.FullName;
            info["<<output>>"] = output_path.Path + fi.Name;

            TextFile.Replace(processor_path.Path + "task.template", processor_path.Path + "task.cfg", ref info);
            if (!app.Run())
            {
               Console.WriteLine("Failure when trying to correct file {0}", fi.Name);
               Console.WriteLine("[FAIL]");
            }
            else
               Console.WriteLine("[ OK ]");

            count++;
         }
      }
   }
}
