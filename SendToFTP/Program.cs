using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


using Mohid.Files;
using Mohid.HDF;

namespace SendToFTP
{
   class Program
   {

      static void Copy(string sourceFileName, string destinyFileName, string sourcePath, string destinyPath)
      {
         FileName source = new FileName(), destiny = new FileName();

         source.Path = sourcePath;
         source.FullName = sourceFileName;
         destiny.Path = destinyPath;
         destiny.FullName = destinyFileName;
         FileTools.CopyFile(source, destiny, CopyOptions.OVERWRIGHT); 
      }

      static void Main(string[] args)
      {


         DirectoryInfo dir = new DirectoryInfo(@"L:\Portugal\Douro\Tamega\MyWater\Simulations\MohidLand\Ref.Evtp.3m.4\store");
         foreach (System.IO.DirectoryInfo g in dir.GetDirectories())
         {
            string folder = g.FullName.Substring(g.FullName.LastIndexOf(System.IO.Path.DirectorySeparatorChar));

            FileTools.CreateFolder(folder, new FilePath(@"E:\Aplica\Projects\MyWater\Work\ToFTP\"));
            Copy("atmosphere.hdf5", "atmosphere.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
            Copy("basin.hdf5", "basin.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
            Copy("basin.evtp.hdf5", "basinevtp.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
            Copy("basin.refevtp.hdf5", "basin.refevtp.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
            Copy("drainage.network.hdf5", "drainagenetwork.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
            Copy("porous.media.hdf5", "porousmedia.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
            Copy("runoff.hdf5", "runoff.hdf5", g.FullName, @"E:\Aplica\Projects\MyWater\Work\ToFTP\" + folder);
         }
      }
   }
}
