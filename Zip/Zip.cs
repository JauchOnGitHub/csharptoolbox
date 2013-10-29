using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Files;
using Ionic.Zip;

namespace Mohid
{
   namespace Zip
   {
      public enum ZipListItemType
      {
         UNKNOWN,
         FILE,
         FOLDER
      }

      public class ZipListItem
      {
         public ZipListItemType Type { get; set; }
         public string Item { get; set; }
         public FilePath PathInZip { get; set; }

         public ZipListItem()
         {
            Type = ZipListItemType.UNKNOWN;
            Item = "";
            PathInZip = new FilePath();
         }

         public ZipListItem(ZipListItemType type, string item, string pathInZip)
         {
            Type = type;
            Item = item;
            PathInZip = new FilePath(pathInZip);
         }

         public ZipListItem(ZipListItemType type, string item, FilePath pathInZip)
         {
            Type = type;
            Item = item;
            PathInZip = pathInZip;
         }

         public ZipListItem(ZipListItemType type, FileName item, FilePath pathInZip)
         {
            Type = type;
            Item = item.FullName;
            PathInZip = pathInZip;
         }

         public ZipListItem(ZipListItemType type, FilePath item, FilePath pathInZip)
         {
            Type = type;
            Item = item.Path;
            PathInZip = pathInZip;
         }
      }

      public class ZipEngine
      {
         protected List<ZipListItem> list;

         public ZipEngine()
         {
            list = new List<ZipListItem>();
         }

         public void AddFile(FileName file, FilePath pathInZip)
         {
            list.Add(new ZipListItem(ZipListItemType.FILE, file, pathInZip));
         }
         public void AddFile(string file, string pathInZip)
         {
            list.Add(new ZipListItem(ZipListItemType.FILE, file, pathInZip));
         }

         public void AddFolder(FilePath folder, FilePath pathInZip)
         {
            list.Add(new ZipListItem(ZipListItemType.FOLDER, folder, pathInZip));
         }

         public void SaveZipToFile(FileName toSave)
         {
            ZipFile zip = new ZipFile(toSave.FullName);

            foreach (ZipListItem item in list)
            {
               switch (item.Type)
               {
                  case ZipListItemType.FILE:
                     zip.AddFile(item.Item, item.PathInZip.Path);
                     break;
                  case ZipListItemType.FOLDER:
                     zip.AddDirectory(item.Item, item.PathInZip.Path);
                     break;
                  default:
                     break;
               }
            }

            zip.Save();
         }


         public void ClearList()
         {
            list.Clear();
         }

      }
   }
}