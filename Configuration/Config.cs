using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Files;

namespace Mohid
{
   namespace Configuration
   {
      public class Config
      {
         #region DATA

         protected string exceptionMessage;
         public string ExceptionMessage { get { return exceptionMessage; } }
         public ConfigNode Root;
         protected string[] keywordSplit;

         #endregion DATA

         #region CONSTRUCTOR

         protected void Init(FileName confFile)
         {
            Root = new ConfigNode();
            Root.NodeType = NodeType.COMMON;
            Comment = "!";
            LineContinuation = " _";
            ConfigFile = confFile;
            keywordSplit = new string[1];
            keywordSplit[0] = "=";
         }

         public Config()
         {
            Init(new FileName("mohidtoolbox.config"));
         }

         public Config(FileName confFile)
         {
            Init(confFile);
         }

         public Config(string confFile)
         {
            Init(new FileName(confFile));
         }

         #endregion CONSTRUCTOR

         #region ENGINE

         public virtual bool Load()
         {
            ConfigNode n = Root;
            Dictionary<string, string> defines = new Dictionary<string, string>();
            string [] defSeps = { "=" };
            string value;

            TextFile cf = new TextFile(ConfigFile);
            cf.OpenToRead();
            bool newBlockFound = false;
            bool newArrayBlockFound = false;

            int line = 0;

            try
            {
               List<string> lines = cf.ReadLines();

               string toRead = "";
               string temp;
               string t;

               foreach (string t_l in lines)
               {
                  line++;

                  if ((temp = t_l.Trim()) == "")
                     continue;
                  else if (temp.EndsWith(" _"))
                  {
                     toRead += temp.Substring(0, temp.Length - 2);
                     continue;
                  }
                  else if(temp == "{")
                  {
                     if (newBlockFound)
                     {
                        newBlockFound = false;                        
                        continue;
                     }
                     else if (newArrayBlockFound)
                     {
                        newArrayBlockFound = false;
                        continue;
                     }
                     else
                     {
                        throw new Exception("Found unexpected '{'"); 
                     }
                  }
                  else
                     toRead += temp;

                  if (toRead[0] == '.') //it's a new block
                  {
                     ConfigNode newNode = new ConfigNode(toRead.Substring(1).Trim().ToLower());
                     newNode.NodeType = NodeType.COMMON;
                     n.ChildNodes.Add(newNode);
                     newNode.Father = n;
                     n = newNode;
                     newBlockFound = true;
                     //Console.WriteLine("New '.' found on line {0}", line.ToString());
                  }
                  else if (toRead[0] == '+')
                  {
                     ConfigNode newNode = new ConfigNode(toRead.Substring(1).Trim().ToLower());
                     newNode.NodeType = NodeType.ARRAY;
                     n.ChildNodes.Add(newNode);
                     newNode.Father = n;
                     n = newNode;
                     newArrayBlockFound = true;
                     //Console.WriteLine("New '+' found on line {0}", line.ToString());
                  }
                  else if (toRead == "}") //it's the end of the current block
                  {
                     n = n.Father;
                  }
                  else if (toRead[0] == '#') //it's a pre-processing directive
                  {
                     if (toRead.StartsWith("#define "))
                     {
                        string[] tokens = toRead.Substring(8).Split(defSeps, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length == 2)
                        {
                           defines[tokens[0].Trim()] = tokens[1].Trim();
                        }
                     }
                  }
                  else if(toRead[0] != '!') //it's a keyword
                  {
                     if (n.NodeType == NodeType.COMMON)
                     {
                        string[] tokens = toRead.Split(keywordSplit, 2, StringSplitOptions.RemoveEmptyEntries);
                        value = tokens[1].Trim();
                        
                        foreach (KeyValuePair<string, string> def in defines)
                        {
                           t = value.Replace(def.Key, def.Value);
                           value = t;
                        }
                        n.NodeData[tokens[0].Trim().ToLower()] = new KeywordData(value.Trim());
                     }
                     else if (n.NodeType == NodeType.ARRAY)
                     {
                        value = toRead.Trim();
                        foreach (KeyValuePair<string, string> def in defines)
                        {
                           t = value.Replace(def.Key, def.Value);
                           value = t;
                        }
                        n.NodeData[(n.NodeData.Count + 1).ToString()] = new KeywordData(value);
                     }
                  }

                  toRead = "";
               }

               cf.Close();

               return true;
            }
            catch(Exception ex)
            {
               exceptionMessage = "Error on line " + line.ToString() + ": " + ex.Message;
               cf.Close();
               return false;
            }
         }
         public virtual bool Save()
         {
            TextFile cf = new TextFile(ConfigFile);
            cf.OpenNewToWrite();

            bool result = false;

            try
            {
               result = SaveToConfigFile(ref cf, Root);               
            }
            catch(Exception ex)
            {
               exceptionMessage = ex.Message;
               cf.Close();
               return false;
            }

            cf.Close();
            return result;
         }

         protected bool SaveToConfigFile(ref TextFile cf, ConfigNode nodeToSave)
         {
            if (nodeToSave.NodeData != null)
               foreach (KeyValuePair<string, KeywordData> pair in nodeToSave.NodeData)
               {
                  if (nodeToSave.NodeType == NodeType.COMMON)
                     cf.WriteLine(pair.Key + keywordSplit[0] + pair.Value.AsString());
                  else if (nodeToSave.NodeType == NodeType.ARRAY)
                     cf.WriteLine(pair.Value.AsString());
               }

            if (nodeToSave.SimpleData != null)
               foreach (string simple in nodeToSave.SimpleData)
               {
                  cf.WriteLine(simple);
               }

            if (nodeToSave.ChildNodes != null)
               foreach (ConfigNode child in nodeToSave.ChildNodes)
               {
                  if (child.NodeType == NodeType.COMMON)
                     cf.WriteLine("." + child.Name);
                  else if (child.NodeType == NodeType.ARRAY)
                     cf.WriteLine("+" + child.Name);

                  cf.WriteLine("{");
                  if (!SaveToConfigFile(ref cf, child))
                     return false;
                  cf.WriteLine("}");
               }

            return true;
         }

         #endregion ENGINE

         #region GET-SET

         public FileName ConfigFile { get; set; }
         public string Comment { get; set; }
         public string LineContinuation { get; set; }
         public string KeywordSplit 
         {
            get { return keywordSplit[0]; }
            set { keywordSplit[0] = value; }
         }

         #endregion GET-SET
      }
   }
}