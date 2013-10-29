using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace CommandArguments
   {
      public class CmdArgs
      {
         #region DATA

         protected Dictionary<string, string> parameters;
         protected List<String> options;
         protected List<string> arguments;
         bool result;

         public char OptionTAG { get; set; }
         public char ParameterTAG { get; set; }
         public char TextDelimiter { get; set; }
         public bool Result { get { return result; } }

         #endregion DATA

         #region CONSTRUCTOR

         public virtual void Init()
         {
            OptionTAG = '\\';
            ParameterTAG = '-'; //For parameters, the sign must be used twice: --parameter 
            TextDelimiter = '\"';
            parameters = new Dictionary<string, string>();
            options = new List<string>();
            arguments = new List<string>();
            result = false;
         }

         public CmdArgs()
         {
            Init();
         }

         public CmdArgs(string[] cmdLine)
         {
            Init();
            result = Parse(cmdLine);
         }

         public CmdArgs(string cmdLine)
         {
            Init();
            result = Parse(cmdLine);
         }

         #endregion CONSTRUCTOR

         #region ENGINE

         public bool Parse(string[] cmdLine)
         {
            string t;
            int type = 0;
            string param = "", arg = "";

            for (int i = 0; i < cmdLine.Length; i++)
            {
               if (string.IsNullOrWhiteSpace(cmdLine[i])) continue;

               if (type != 2)
               {
                  t = cmdLine[i].TrimStart();
                  if (t[0] == '\\')
                  {
                     type = 1;
                     param = t.Trim().Substring(1);
                  }
                  else if (t.Length > 1 && t[0] == '-' && t[1] == '-')
                  {
                     type = 2;
                     param = t.Trim().Substring(2);
                  }
                  else
                  {
                     arg = cmdLine[i];
                  }
               }
               else
               {
                  type = 3;
                  arg = cmdLine[i];
               }

               switch (type)
               {
                  case 1:
                     type = 0;
                     options.Add(param);
                     break;
                  case 2:                     
                     break;
                  case 3:
                     type = 0;
                     parameters[param] = arg;
                     break;
                  default:                     
                     arguments.Add(arg);
                     break;
               }
            }

            return true;
         }

         public bool Parse(string cmdLine)
         {
            if (string.IsNullOrWhiteSpace(cmdLine))
               return true;

            cmdLine = cmdLine.Trim();
            int cmdCount = cmdLine.Length - 1;

            int p, c;
            int i = 0;
            string param, arg;

            while (i < cmdLine.Length)
            {
               if (cmdLine[i] == '\\')
               {
                  //It's an OPTION
                  p = cmdLine.IndexOf(' ', i);

                  c = 0;
                  if (p < 0)
                     c = cmdLine.Length - (i + 1);
                  else if (p > i + 1)
                     c = p - (i + 1);

                  if (c > 0)
                  {
                     options.Add(cmdLine.Substring(i + 1, c));
                  }

                  i = i + c + 1;
               }
               else if (i < cmdCount && cmdLine[i] == ParameterTAG && cmdLine[i + 1] == ParameterTAG)
               {
                  //It's a PARAMETER
                  i = i + 2;
                  p = cmdLine.IndexOf(' ', i);
                  c = 0;
                  if (p > 0)
                  {
                     c = p - i;
                     param = cmdLine.Substring(i, c);
                     i = i + c;
                     arg = "";

                     while (i <= cmdCount && cmdLine[i] == ' ') i++;

                     if (i < cmdCount)
                     {
                        if (cmdLine[i] == '\"')
                        {
                           i++;
                           p = cmdLine.IndexOf('\"', i);
                           if (p < 0)
                              c = cmdCount - i;
                           else
                              c = p - i;
                           arg = cmdLine.Substring(i, c);
                        }
                        else
                        {
                           p = cmdLine.IndexOf(' ', i);
                           if (p < 0)
                              c = cmdCount - i + 1;
                           else
                              c = p - i;
                           arg = cmdLine.Substring(i, c);
                        }
                     }
                     parameters[param] = arg;
                     i = i + c + 1;
                  }

               }
               else if (cmdLine[i] == ' ')
                  i++;
               else
               {
                  if (cmdLine[i] == '\"')
                  {
                     i++;
                     p = cmdLine.IndexOf('\"', i);
                     if (p < 0)
                        c = cmdCount - i;
                     else
                        c = p - i;
                     arguments.Add(cmdLine.Substring(i + 1, c));
                  }
                  else
                  {
                     p = cmdLine.IndexOf(' ', i);
                     if (p < 0)
                        c = cmdCount - i + 1;
                     else
                        c = p - i;
                     arguments.Add(cmdLine.Substring(i, c));
                  }
                  i = i + c + 1;
               }

            }

            return (result = true);
         }         

         #endregion ENGINE

         #region INFO

         public bool HasParameter(string param)
         {
            if (parameters.ContainsKey(param))
               return true;
            return false;
         }
         public string Parameter(string param)
         {
            if (parameters.ContainsKey(param))
               return parameters[param];
            return null;
         }
         public bool HasOption(string option)
         {
            if (options.Contains(option))
               return true;
            return false;
         }
         public string Argument(int index)
         {
            return arguments[index];
         }
         public Dictionary<string, string> Parameters { get { return parameters; } }
         public List<string> Arguments { get { return arguments; } }
         public List<string> Options { get { return options; } }

         #endregion
      }
   }
}