using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Mohid.Files;

namespace Mohid
{
   namespace Configuration
   {
      public enum NodeType
      {
         COMMON,
         ARRAY
      }

      public class KeywordData
      {
         #region DATA

         protected string data;
         protected string[] list;
         protected string[] separator;

         public string DateTimeFormat { get; set; }
         public StringSplitOptions RemoveEmpty { get; set; }

         #endregion DATA

         #region CONSTRUCTOR

         protected virtual void Init()
         {
            data = "";
            DateTimeFormat = "yyyy M d H m s";            
            separator = new string[1];
            separator[0] = " ";
         }

         public KeywordData()
         {
            Init();
         }

         public KeywordData(string data)
         {
            Init();
            Set(data);
         }

         #endregion CONSTRUCTOR

         #region ENGINE

         public void Set(string newData)
         {
            data = newData;
            list = data.Split(separator, RemoveEmpty);
         }        

         #endregion ENGINE

         #region GET-SET
               
         public string AsString(int index = -1)
         {
            if (index >= 0)
               return list[index];
            else
               return data;
         }
         public string this[int index]
         {
            get { return list[index]; }
            set { list[index] = value; }
         }
         public FileName AsFileName(int index = -1)
         {
            if (index >= 0)
               return new FileName(list[index]);
            else
               return new FileName(data);
         }
         public FilePath AsFilePath(int index = -1)
         {
            if (index >= 0)
               return new FilePath(list[index]);
            else
               return new FilePath(data);
         }
         public int AsInt(int index = -1)
         {
            if (index >= 0)
               return int.Parse(list[index]);
            else
               return int.Parse(data);
         }
         public long AsLong(int index = -1)
         {
            if (index >= 0)
               return long.Parse(list[index]);
            else
               return long.Parse(data);
         }       
         public double AsDouble(int index = -1)
         {
            if (index >= 0)
               return double.Parse(list[index], CultureInfo.InvariantCulture);
            else
               return double.Parse(data, CultureInfo.InvariantCulture);
         }
         public bool AsBool(int index = -1)
         {
            if (index >= 0)
               return bool.Parse(list[index]); 
            else
               return bool.Parse(data); 
         }
         public DateTime AsDateTime(int index = -1)
         {
            if (index >= 0)
               return DateTime.ParseExact(list[index], DateTimeFormat, null); 
            else
               return DateTime.ParseExact(data, DateTimeFormat, null); 
         }
         public DateTime AsDateTime(string format, int index = -1)
         {
            if (index >= 0)
               return DateTime.ParseExact(list[index], format, null);
            else
            {
               CultureInfo provider = CultureInfo.InvariantCulture;
               return DateTime.ParseExact(data, format, provider);
            }
         }

         public int Count
         {
            get { return list.Length; }
         }
         public string Separator 
         { 
            get { return separator[0]; }
            set
            {               
               separator[0] = value;
               list = data.Split(separator, RemoveEmpty);
            }
         }

         #endregion GET-SET
      }

      public class ConfigNode
      {
         #region DATA

         public ConfigNode Father;
         public List<ConfigNode> ChildNodes;
         public Dictionary<string, KeywordData> NodeData;
         public List<string> SimpleData;

         #endregion DATA

         #region CONSTRUCTOR

         protected virtual void Init(string name, bool ownNodeData)
         {
            Name = name;
            Father = null;
            ChildNodes = new List<ConfigNode>();
            if (ownNodeData)
               NodeData = new Dictionary<string, KeywordData>();
            else
               NodeData = null;
         }

         public ConfigNode()
         {
            Init("", true);
         }
         public ConfigNode(string name)
         {
            Init(name, true);            
         }
         public ConfigNode(string name, bool ownNodeData)
         {
            Init(name, ownNodeData);
         }

         #endregion CONSTRUCTOR

         #region GET-SET

         public KeywordData this[string key]
         {
            get { return NodeData[key]; }
            set { NodeData[key] = value; }
         }

         public KeywordData this[string key, object defaultValue]
         {
            get
            {
               if (NodeData.ContainsKey(key))
                  return NodeData[key];
               else
                  return new KeywordData(defaultValue.ToString());
            }
         }
         public string Name { get; set; }
         public NodeType NodeType { get; set; }

         public bool Contains(string key)
         {
            return NodeData.ContainsKey(key);
         }

         #endregion GET-SET
      }
   }
}
