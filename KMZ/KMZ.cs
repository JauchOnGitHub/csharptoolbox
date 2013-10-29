using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Mohid.Files;
using Mohid.Zip;

namespace Mohid
{
   namespace KMZ
   {
      public class KMZEngine
      {
         public CopyOptions OverwriteKMZ { get; set; }
         public bool ChangeTemplate { get; set; }
         public FilePath SearchPath { get; set; }
         public FileName KMLTemplate { get; set; }
         public FileName KMLOutput { get; set; }
         public string ImageFileNameMask { get; set; }
         public string ImageExtension { get; set; }
         public FilePath KMZImageFolder { get; set; }
         public FileName KMZOutput { get; set; }
                  
         public Dictionary<string, string> ChangeList;

         public KMZEngine()
         {
            OverwriteKMZ = CopyOptions.IGNORE;
            SearchPath = new FilePath(".\\");
            KMLTemplate = new FileName("doc.kml");
            ChangeTemplate = false;
            KMLOutput = new FileName("doc.kml");
            ImageFileNameMask = "";
            ImageExtension = "*.png";
            KMZImageFolder = new FilePath("folder");
            KMZOutput = new FileName();                        
            ChangeList = new Dictionary<string, string>();
         }

         public void CreateKMZ()
         {
            ZipEngine zip = new ZipEngine();
            List<Mohid.Files.FileInfo> files = new List<Mohid.Files.FileInfo>();
            
            ChangeList.Add("<<name>>", null);

            FileTools.FindFiles(ref files, SearchPath, ImageExtension, true, null);
            
            foreach (Mohid.Files.FileInfo image_file in files)
            {
               if (string.IsNullOrWhiteSpace(KMZOutput.FullName))
                  KMZOutput.FullName = image_file.FileName.Name + ".kmz";
               else
                  KMZOutput.Extension = "kmz";
               
               if (Regex.IsMatch(image_file.FileName.FullName, ImageFileNameMask))
               {
                  if (OverwriteKMZ == CopyOptions.IGNORE)
                     if (File.Exists(KMZOutput.FullPath))
                        continue;

                  if (ChangeTemplate)
                  {
                     ChangeList["<<name>>"] = image_file.FileName.Name;
                     TextFile.Replace(KMLTemplate.FullPath, KMLOutput.FullPath, ref ChangeList);
                  }
                  else
                     FileTools.CopyFile(KMLTemplate, KMLOutput, CopyOptions.OVERWRIGHT);

                  zip.AddFile(SearchPath.Path + image_file.FileName.FullName, KMZImageFolder.Path);
                  zip.AddFile(KMLOutput.FullPath, "");
                  zip.SaveZipToFile(KMZOutput);
                  zip.ClearList();
               }
            }
         }  
      }
   }
}