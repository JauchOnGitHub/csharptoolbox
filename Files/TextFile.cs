using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Files
   {
      public class TextFile
      {
         public FileName File;

         protected bool fileIsOpenToRead,
                        fileIsOpenToWrite;

         protected FileStream fileStream;

         protected StreamReader reader;
         protected StreamWriter writer;

         protected virtual void Init()
         {
            File = new FileName(@"Text.txt");
            fileIsOpenToRead = false;
            fileIsOpenToWrite = false;
            fileStream = null;
            reader = null;
            writer = null;
         }

         public TextFile()
         {
            Init();
         }

         public TextFile(string fullpath)
         {
            Init();
            File.FullPath = fullpath;
         }

         public TextFile(FileName file)
         {
            Init();
            File = file;
         }

         ~TextFile()
         {
            Close();
         }

         public void Open(FileMode mode, FileAccess access, FileShare share)
         {
            fileStream = new FileStream(File.FullPath, mode, access, share);

            switch (access)
            {
               case FileAccess.Read:
                  reader = new StreamReader(fileStream);
                  fileIsOpenToRead = true;
                  fileIsOpenToWrite = false;
                  break;
               case FileAccess.Write:
                  writer = new StreamWriter(fileStream);
                  fileIsOpenToRead = false;
                  fileIsOpenToWrite = true;
                  break;
               default:
                  throw new Exception("File can be open for reading or writing, but not both at same time");
            }

         }

         public void OpenToRead(FileShare share = FileShare.None)
         {
            Open(FileMode.Open, FileAccess.Read, share);
         }

         public void OpenToWrite(FileShare share = FileShare.None)
         {
            Open(FileMode.Open, FileAccess.Write, share);
         }

         public void OpenToAppend(FileShare share = FileShare.None)
         {
            Open(FileMode.Append, FileAccess.Write, share);
         }

         public void OpenNewToWrite(FileShare share = FileShare.None)
         {
            Open(FileMode.Create, FileAccess.Write, share);
         }

         public void Close()
         {
            if (fileStream != null)
            {
               if (writer != null && fileStream.CanWrite)
               {
                  writer.Flush();
               }

               fileStream.Close();
               fileStream = null;
            }

            fileIsOpenToRead = false;
            fileIsOpenToWrite = false;
         }

         public bool Write(string text)
         {
            if (!fileIsOpenToWrite) return false;
            writer.Write(text);
            return true;
         }

         public bool Write(StringBuilder text)
         {
            if (!fileIsOpenToWrite) return false;
            writer.Write(text.ToString());
            return true;
         }

         public bool WriteLine(string line)
         {
            if (!fileIsOpenToWrite) return false;
            writer.WriteLine(line);
            return true;
         }

         public bool WriteLines(List<string> lines)
         {
            if (!fileIsOpenToWrite) return false;
            foreach (string line in lines)
               writer.WriteLine(line);
            return true;
         }

         public string Read(int n_chars = 1)
         {
            if (!fileIsOpenToRead) return null;

            string text = "";
            int c;
            int i;

            for (i = 1; i <= n_chars; i++)
            {
               c = reader.Read();
               if (c == -1) break;
               text = text + c.ToString();
            }

            return text;
         }

         public string ReadLine()
         {
            if (!fileIsOpenToRead) return null;
            return reader.ReadLine();
         }

         public List<string> ReadLines()
         {
            if (!fileIsOpenToRead) return null;

            List<string> lines = new List<String>();
            string line;

            for (; ; )
            {
               line = reader.ReadLine();
               if (line == null) break;
               lines.Add(line);
            }

            return lines;
         }

         public string ReadLinesToString()
         {
            if (!fileIsOpenToRead) return null;

            StringBuilder lines = new StringBuilder();
            string line;

            for (; ; )
            {
               line = reader.ReadLine();
               if (line == null) break;
               lines.AppendLine(line);
            }

            return lines.ToString();
         }

         public static int Replace(string old_file, string new_file, ref Dictionary<string, string> replace_list, bool warning_on_exception = true)
         {
            try
            {
               TextFile input = null;
               TextFile output = null;
               int NumberOfChanges = 0;
            
               input = new TextFile(old_file);
               output = new TextFile(new_file);

               input.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
               output.Open(FileMode.Create, FileAccess.Write, FileShare.None);

               string line = "";
               string new_line = "";
             
               line = input.ReadLine();

               while (line != null)
               {
                  foreach (KeyValuePair<string, string> keyPair in replace_list)
                  {
                     new_line = line.Replace(keyPair.Key, replace_list[keyPair.Key]);
                     if (new_line != line) NumberOfChanges++;
                     line = new_line;
                  }

                  output.WriteLine(line);
                  line = input.ReadLine();
               }

               input.Close();
               output.Close();

               return NumberOfChanges;
            }
            catch (Exception ex)
            {
               if (warning_on_exception)
               {
                  Console.WriteLine();
                  while (ex != null)
                  {
                     Console.WriteLine("=> {0}", ex.Message);
                     ex = ex.InnerException;
                  }
                  Console.WriteLine();
               }

               return -1;
            }
         }
      }
   }
}