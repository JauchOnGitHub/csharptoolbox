using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid;
using Mohid.Core;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;

namespace MohidUPIDownloader
{
   public enum ProjectOutputType
   {
      Database,
      Timeseries
   }

   public class ProjectOutputInfoDB
   {
      public string Connection;      
   }

   public class ProjectOutputInfoTS
   {      
      public int NumberOfColumns;
      public TimeUnits TimeUnits;
      public DateTime Start;
   }

   public class ProjectOutputInfo
   {
      public ProjectOutputType Type;
      public int ID;
      public string Name;
      public string Description;
      public object Data; //ProjectOutputInfoTS or ProjectOutputInfoDB
   }

   public class ProjectNodeInfoDB
   {
      public string Table,
                    Column;
   }

   public class ProjectNodeInfoTS
   {
      public int Position;
      public string Header;
   }

   public class ProjectNodeInfo
   {
      public string NodeID;
      public int OutputID;
      public object Data; //ProjectNodeInfoDB or ProjectNodeInfoTS
   }

   public class Project
   {
      public string DateFormat;
      public FileName ProjectFile;
      public UPILoginData LoginData;
      public List<ProjectOutputInfo> OutputInfoList;
      public List<ProjectNodeInfo> NodeInfoList;
      protected Config cfg;

      public Project()
      {
         cfg = new Config();
         OutputInfoList = new List<ProjectOutputInfo>();
         NodeInfoList = new List<ProjectNodeInfo>();
      }

      public void Load()
      {
         try
         {
            OutputInfoList.Clear();
            NodeInfoList.Clear();

            cfg.ConfigFile = ProjectFile;

            if (!cfg.Load())
               throw new Exception("Was not possible to load the project file '" + ProjectFile.FullPath + "'");

            try
            {
               DateFormat = cfg.Root["date.format"].AsString();
            }
            catch (Exception ex)
            {
               throw new Exception("'date.format' keyword not found in project file.", ex);
            }

            LoadLoginInfo();
            LoadOutputBlocks();
            LoadNodeBlocks();
         }
         catch(Exception ex)
         {
            throw new Exception("Project.Load() failed.", ex);
         }
      }

      protected void LoadLoginInfo()
      {
         ConfigNode node = null;

         if ((node = cfg.Root.ChildNodes.Find(CheckForLoginBlock)) != null)
         {
            LoginData.Server = node["server", ""].AsString();
            LoginData.User = node["user", ""].AsString();
            LoginData.Pass = node["password", ""].AsString();
            LoginData.Mode = node["mode", "t"].AsString();
            LoginData.Version = node["version", "1.2"].AsString();
            LoginData.Timeout = node["timeout", 3600].AsInt();
         }
         else
         {
            throw new Exception("No Login Info block found in project file.");
         }
      }

      protected bool CheckForLoginBlock(ConfigNode toMatch)
      {
         if (toMatch.Name == "login.info")
            return true;
         return false;
      }

      protected void LoadOutputBlocks()
      {
         List<ConfigNode> list = cfg.Root.ChildNodes.FindAll(CheckForOutputBlock);
         string type;
         int id;

         foreach (ConfigNode item in list)
         {
            ProjectOutputInfo poi = new ProjectOutputInfo();

            try 
            {
               type = item["type"].AsString();
            }
            catch(Exception ex)
            {
               throw new Exception("Project.LoadOutputBlocks() failed. 'type' keyword was not found in block", ex);
            }
            try
            {
               id = item["id"].AsInt();
            }
            catch(Exception ex)
            {
               throw new Exception("Project.LoadOutputBlocks() failed. 'id' value is not an inteher or the keyword was not found in block", ex);
            }

            poi.Type = (ProjectOutputType)Enum.Parse(typeof(ProjectOutputType), type, true);
            poi.Name = item["name", ""].AsString();
            poi.ID = id;
            poi.Description = item["description", ""].AsString();

            if (poi.Type == ProjectOutputType.Database)
            {
               poi.Data = new ProjectOutputInfoDB();
               (poi.Data as ProjectOutputInfoDB).Connection = item["connection", ""].AsString();
            }
            else
            {
               poi.Data = new ProjectOutputInfoTS();
               (poi.Data as ProjectOutputInfoTS).TimeUnits = (TimeUnits)Enum.Parse(typeof(TimeUnits), item["time.units", "seconds"].AsString(), true);
               if (item.NodeData.ContainsKey("start.date"))
                  (poi.Data as ProjectOutputInfoTS).Start = item["start.date"].AsDateTime(DateFormat);
               (poi.Data as ProjectOutputInfoTS).NumberOfColumns = item["number.of.columns", 0].AsInt();
            }

            OutputInfoList.Add(poi);
         }
      }

      protected bool CheckForOutputBlock(ConfigNode toMatch)
      {
         if (toMatch.Name == "output.info")
            return true;
         return false;
      }

      protected void LoadNodeBlocks()
      {
         List<ConfigNode> list = cfg.Root.ChildNodes.FindAll(CheckForNodeBlock);

         string str;
         int integer;         

         foreach (ConfigNode item in list)
         {
            ProjectNodeInfo pni = new ProjectNodeInfo();

            try
            {
               str = item["node.id"].AsString();
            }
            catch (Exception ex)
            {
               throw new Exception("Project.LoadNodeBlocks() failed. 'node.id' keyword was not found in block", ex);
            }

            pni.NodeID = str;

            try
            {
               integer = item["output.id"].AsInt();
            }
            catch (Exception ex)
            {
               throw new Exception("Project.LoadNodeBlocks() failed. 'output.id' value is not an integer or keyword was not found in block", ex);
            }

            ProjectOutputInfo poi = OutputInfoList.Find(oi => oi.ID == integer);
            if (poi == null)
               throw new Exception("A node refer to an unknown output ID: '" + integer.ToString() + "'");

            pni.OutputID = integer;

            if (poi.Type == ProjectOutputType.Database)
            {
               pni.Data = new ProjectNodeInfoDB();

               try
               {
                  str = item["table"].AsString();
               }
               catch (Exception ex)
               {
                  throw new Exception("Project.LoadNodeBlocks() failed. 'table' keyword was not found in block", ex);
               }

               (pni.Data as ProjectNodeInfoDB).Table = str;

               try
               {
                  str = item["column.name"].AsString();
               }
               catch (Exception ex)
               {                  
                  throw new Exception("Project.LoadNodeBlocks() failed. 'column.name' keyword was not found in block", ex);
               }

               (pni.Data as ProjectNodeInfoDB).Column = str;

            }
            else //If is not database, than is timeseries
            {
               pni.Data = new ProjectNodeInfoTS();

               try
               {
                  str = item["header"].AsString();
               }
               catch (Exception ex)
               {
                  throw new Exception("Project.LoadNodeBlocks() failed. 'table' keyword was not found in block", ex);
               }

               (pni.Data as ProjectNodeInfoTS).Header = str;

               try
               {
                  integer = item["position"].AsInt();
               }
               catch (Exception ex)
               {
                  throw new Exception("Project.LoadNodeBlocks() failed. 'column.name' keyword was not found in block", ex);
               }

               (pni.Data as ProjectNodeInfoTS).Position = integer;
            }

            NodeInfoList.Add(pni);
         }
      }

      protected bool CheckForNodeBlock(ConfigNode toMatch)
      {
         if (toMatch.Name == "node.info")
            return true;
         return false;
      }

      public void Save()
      {
      }

      public void SaveAs(FileName projectFile)
      {
      }

      public void Close()
      {
      }
   }
}
