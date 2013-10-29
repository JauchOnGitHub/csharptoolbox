using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Files;
using Mohid.Configuration;

namespace Mohid
{
   namespace Log
   {
      public class LogEngine
      {
         #region INTERNAL DATA

         protected Config conf;
         //protected List<ConfigNode> entries;
         protected string entryToFind;

         #endregion INTERNAL DATA

         #region CONSTRUCTOR

         public void Init(FileName file)
         {            
            conf = new Config(file);
         }

         public LogEngine() 
         {
            Init(new FileName("info.log"));
         }
         public LogEngine(FileName logFile) 
         {
            Init(logFile);
         }
         public LogEngine(string logFile) 
         {
            Init(new FileName(logFile));
         }

         #endregion CONSTRUCTOR

         #region FILE MANAGEMENT

         public bool Load()
         {
            if (File.Exists(conf.ConfigFile.FullPath))
               return conf.Load();
            else
               conf.Root = new ConfigNode();
            return true;
         }
         public bool Load(FileName logFile)
         {
            conf.ConfigFile = logFile;
            return Load();
         }
         public bool Load(string logFile)
         {
            conf.ConfigFile = new FileName(logFile);
            return Load();
         }
         public bool Save()
         {
            return conf.Save();
         }
         public bool Save(FileName logFile)
         {
            conf.ConfigFile = logFile;
            return Save();
         }
         public bool Save(string logFile)
         {
            conf.ConfigFile = new FileName(logFile);
            return Save();
         }

         #endregion FILE MANAGEMENT

         #region INFO

         public int Count { get { return conf.Root.ChildNodes.Count; } }
         public int LastIndex { get { return conf.Root.ChildNodes.Count - 1; } }
         public ConfigNode this[int index] { get { return conf.Root.ChildNodes[index]; } }
         public List<ConfigNode> FindEntries(string toMatch)
         {
            entryToFind = toMatch;
            return conf.Root.ChildNodes.FindAll(FindLogEntries);
         }

         #endregion INFO

         public bool AddEntry(ConfigNode data)
         {
            try
            {
               data.Father = conf.Root;
               conf.Root.ChildNodes.Add(data);
               return true;
            }
            catch
            {
               return false;
            }
         }

         #region AUX

         protected bool FindLogEntries(ConfigNode nodeToMatch)
         {
            if (nodeToMatch.Name == entryToFind)
               return true;
            else
               return false;
         }

         #endregion AUX
      }

      //public class SimLogEngine : LogEngine
      //{         
      //   protected List<ConfigNode> entries;
      //   protected bool open, changed;

      //   protected void Init(FileName logFile)
      //   {
      //      base.Init(logFile);
      //      open = false;
      //      changed = false;         
      //   }

      //   public SimLogEngine() : base()
      //   {
      //      Init(new FileName("info.log"));
      //   }

      //   public SimLogEngine(FileName logFile) : base()
      //   {
      //      Init(logFile);
      //   }

      //   public SimLogEngine(string logFile)
      //      : base()
      //   {
      //      Init(new FileName(logFile));
      //   }

      //   public override bool Load()
      //   {
      //      if (File.Exists(base.ConfigFile.FullPath))
      //      {
      //         try
      //         {
      //            if (!base.Load())
      //               return false;

      //            entries = base.Root.ChildNodes.FindAll(FindLogEntries);
      //            entries.Sort(SortEntriesByID);
      //         }
      //         catch
      //         {
      //            return false;
      //         }
      //      }
      //      else
      //      {
      //         entries = new List<ConfigNode>();
      //      }

      //      open = true;
      //      changed = false;

      //      return true;
      //   }

      //   public bool Load(FileName logFile)
      //   {
      //      base.ConfigFile = logFile;
      //      return Load();
      //   }

      //   public bool Load(string logFile)
      //   {
      //      base.ConfigFile = new FileName(logFile);
      //      return Load();
      //   }
      //   public bool Save(FileName logFile)
      //   {
      //      bool res = base.Save(logFile);
      //      if (res)
      //         changed = false;
      //      return res;
      //   }
      //   public bool Save(string logFile)
      //   {
      //      bool res = base.Save(logFile);
      //      if (res)
      //         changed = false;
      //      return res;
      //   }
      //   public bool Close(bool saveChanges = true)
      //   {
      //      if (open && changed && saveChanges)
      //         Save();

      //      changed = false;
      //      open = false;

      //      return true;
      //   }

      //   protected bool FindLogEntries(ConfigNode nodeToMatch)
      //   {
      //      if (nodeToMatch.Name == "entry")
      //         return true;
      //      else
      //         return false;
      //   }

      //   protected int SortEntriesByID(ConfigNode a, ConfigNode b)
      //   {
      //      int aId = a.NodeData["ID"].AsInt();
      //      int bId = b.NodeData["ID"].AsInt();

      //      if (aId > bId)
      //         return 1;
      //      else if (bId > aId)
      //         return -1;
      //      else
      //         return 0;
      //   }

      //   public int Count { get { return entries.Count; } }
      //   public int LastIndex { get { return entries.Count - 1; } }
      //   public ConfigNode LastEntry 
      //   { 
      //      get 
      //      {
      //         if (entries != null && entries.Count > 0)
      //            return entries[entries.Count - 1];
      //         else
      //            return null;
      //      } 
      //   }
      //   public ConfigNode this[int index] { get { return entries[index]; } }
      //   public bool AddEntry(Dictionary<string, KeywordData> data)
      //   {
      //      try
      //      {
      //         ConfigNode newNode = new ConfigNode("log.entry", false);
      //         newNode.NodeData = data;
      //         newNode.NodeData["ID"] = new KeywordData(entries.Count.ToString());
      //         base.Root.ChildNodes.Add(newNode);
      //         return true;
      //      }
      //      catch
      //      {
      //         return false;
      //      }
      //   }


      //}
   }
}