using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Software
   {
      enum ArgsType
      {
         Unknown,
         Option,
         OptionWithValue,
         Parameter
      }

      public class Args
      {
         protected Dictionary<string, string> f_options;
         protected List<string> f_arguments;

         public char Tag { get; set; }
         public char OptionTag { get; set; }
         public List<string> Arguments { get { return f_arguments; } }
         public Dictionary<string, string> Options { get { return f_options; } }
         public bool HasOption(string option) { return f_options.ContainsKey(option); }
         public string OptionArgument(string option)
         {
            if (f_options.ContainsKey(option))
               return f_options[option];
            return null;
         }

         public virtual void Init()
         {
            Tag = '-';
            OptionTag = '=';
            f_options = new Dictionary<string, string>();
            f_arguments = new List<string>();
         }
         public Args()
         {
            Init();
         }
         public Args(string[] cmdLine)
         {
            Init();
            Parse(cmdLine);
         }

         public void Parse(string[] cmdLine)
         {
            string t;

            foreach (string item in cmdLine)
            {
               if (string.IsNullOrWhiteSpace(item)) continue;
               t = item.Trim();

               if (t[0] == Tag)
               {
                  Console.WriteLine(t);
                  int p = t.IndexOf(OptionTag);

                  if (p > 1)
                  {
                     f_options[t.Substring(1, p - 1)] = t.Substring(p + 1, t.Length - p - 1);
                     Console.WriteLine(t.Substring(1, p -1 ));
                     Console.WriteLine(t.Substring(p + 1, t.Length - p - 1));
                  }
                  else
                  {
                     f_options[t.Substring(1)] = "";
                  }
               }
               else
               {
                  f_arguments.Add(t);
               }
            }
         }
      }
   }
}
