using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RenameMohidResultsFiles
{
   class Program
   {
      /*
       * Arguments:
       * First : Path to folder where the files must be renamed
       * Second: Search pattern to choose files (ex.: *.hdf5)
       * Third : AllDirectories or TopDirectoryOnly
       * Fourth: _1 or _2
      */
      static void Main(string[] args)
      {
         bool show_help = true;

         if (args.Length == 4)
         {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(args[0]);
            string new_name;

            foreach (System.IO.FileInfo fileToRename in directory.GetFiles(args[1], (System.IO.SearchOption)Enum.Parse(typeof(System.IO.SearchOption), (string)args[2], true)))
            {
               Console.Write("Cheking file {0}", fileToRename.Name);
               if (System.IO.Path.GetFileNameWithoutExtension(fileToRename.Name).EndsWith(args[3]))
               {
                  new_name = fileToRename.FullName.Replace(args[3], "");
                  System.IO.File.Move(fileToRename.FullName, new_name);
                  Console.WriteLine("[ OK ]");
               }
               else
                  Console.WriteLine("[ SKIPPED ]");
            }
            show_help = false;
         }
         
         if (show_help)
         {
            Console.WriteLine("Usage: RenameMohidResultsFiles [path] [search_pattern] [recursion] [ends_with]");
            Console.WriteLine("       [path]           : Path to the folder where the files to rename are.");
            Console.WriteLine("       [search_pattern] : Pattern of files to rename. Ex.: *.hdf5");
            Console.WriteLine("       [recursion]      : AllDirectories to include sub-folders or TopDirectoryOnly");
            Console.WriteLine("       [ends_with]      : Ending of the name that must \"disappear\"");
            Console.WriteLine("                             Ex.: _1");
            Console.WriteLine("                             RunOff_1.hdf5 => RunOff.hdf5");
         }
      }
   }
}
