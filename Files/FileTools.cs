using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Mohid
{
   namespace Files
   {
      public enum CopyOptions
      {
         UNKNOW,
         OVERWRIGHT,
         IGNORE
      }

      public class FileInfo
      {
         public FileName FileName { get; set; }
         public string FileExtension { get; set; }     
         public DateTime DateOnFileName { get; set; }
         public string DateOnFileMask { get; set; }

         protected virtual void Init()
         {
            FileName = new FileName();
            FileExtension = "";
            DateOnFileName = DateTime.Today;
            DateOnFileMask = ""; // "yyyy_MM_dd_HH_mm_ss";
         }

         public FileInfo()
         {
            Init();
         }

         public FileInfo(FileName fileName, string dateOnFileMask = null)
         {
            Init();

            if (string.IsNullOrWhiteSpace(fileName.FullName)) 
               throw new Exception("File name parameter is missing");                        

            FileName = new FileName(fileName.FullPath);
            FileExtension = fileName.Extension;
            DateOnFileMask = dateOnFileMask;

            if (!string.IsNullOrWhiteSpace(DateOnFileMask))
               DateOnFileName = DateTime.ParseExact(FileName.Name.Substring(FileName.Name.Length - DateOnFileMask.Length), DateOnFileMask, null);
         }

      }

      public class FileTools
      {
         #region SEARCH

         public static void FindFiles(ref List<FileInfo> files,
                                      FilePath path,
                                      string searchPattern,
                                      bool clear,
                                      string DateOnFileMask,
                                      System.IO.SearchOption so = System.IO.SearchOption.TopDirectoryOnly)                                      
         {
             System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path.Path);
             System.IO.FileInfo[] aryFi = di.GetFiles(searchPattern, so);

            if (clear) 
               files.Clear();

            foreach (System.IO.FileInfo fi in aryFi)
            {
               FileInfo file = new FileInfo(new FileName(fi.FullName));
               file.DateOnFileMask = DateOnFileMask;
               files.Add(file);
            }
         }

         public static DateTime FindLastDate(ref List<FileInfo> files, DateTime default_date) 
         {
            if (files.Count < 1) 
               return default_date;
            return files.Last().DateOnFileName;
         }

         public static int FindFirstAfter(ref List<FileInfo> files, DateTime date)
         {             
            for (int i = 0; i < files.Count; i++)
               if (files[i].DateOnFileName > date) 
                  return i;
            
            return -1;
         }

         public static int FindSameAs(ref List<FileInfo> files,  DateTime date)
         {
            for (int i = 0; i < files.Count; i++)
               if (files[i].DateOnFileName == date) 
                  return i;
            
            return -1;
         }

         #endregion SEARCH

         #region FILE OPERATIONS

         public static bool FolderExists(FilePath path)
         {
            System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(path.Path);
            return folder.Exists;
         }

         public static FilePath CreateFolder(string newFolder, FilePath path)
         {
            FilePath newFolderPath;

            try
            {
               newFolderPath = new FilePath(path.Path + newFolder);

               if (!FileTools.FolderExists(newFolderPath))
               {
                  System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(newFolderPath.Path);
                  folder.Create();
               }
            }
            catch
            {
               return null;
            }

            return newFolderPath;
         }

         public static bool CopyFile(FileName orig, FileName dest, CopyOptions option)
         {
            if (!System.IO.File.Exists(orig.FullPath))
                  return false;

            if (System.IO.File.Exists(dest.FullPath))
            {
               if (option == CopyOptions.OVERWRIGHT)
                  System.IO.File.Copy(orig.FullPath, dest.FullPath, true);
               else if (option == CopyOptions.IGNORE)
                  return true;
               else
                  return false;
            }
            else
               System.IO.File.Copy(orig.FullPath, dest.FullPath);            

            return true;
         }

         public static bool CopyFile(FilePath orig, FilePath dest, string searchPattern, CopyOptions option)
         {            
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(orig.Path);
            System.IO.FileInfo[] aryFi = di.GetFiles(searchPattern);
            
            FileName o = new FileName();
            o.Path = orig.Path;
            FileName d = new FileName();
            d.Path = dest.Path;
            foreach (System.IO.FileInfo fi in aryFi)
            {
               o.Name = fi.Name;
               d.Name = fi.Name;
               if (!CopyFile(o, d, option))
                  return false;
            }

            return true;
         }

         #endregion FILE OPERATIONS

      }

      public class FileInfoComparer : IComparer<FileInfo>
      {
         public int Compare(FileInfo x, FileInfo y)          
         {
            return DateTime.Compare(x.DateOnFileName, y.DateOnFileName);
         }
      }

      public class FileInfoEqualComparer : IEqualityComparer<FileInfo>
      {
         public bool Equals(FileInfo x, FileInfo y)
         {
            return DateTime.Equals(x.DateOnFileName, y.DateOnFileName);
         }

         public int GetHashCode(FileInfo obj)
         {
            return obj.DateOnFileName.ToString().ToLower().GetHashCode();
         }

      }
   }
}
