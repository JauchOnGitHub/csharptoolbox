//DLLNAME: Mohid.Configuration
//DLLNAME: Mohid.Script
//DLLNAME: Mohid.Files
//DLLNAME: System

using System;
using System.Collections.Generic;
using System.Text;
using Mohid.Files;
using Mohid.Configuration;
using Mohid.Script;
using Mohid.CommandArguments;

namespace Script
{
   public class ScriptTest : IMohidScript
   {
      protected Config cfg;

      public bool Run(CmdArgs args)
      {
         if (!LoadConfig())
            return false;

         for (int i = 0; i < cfg.Root.ChildNodes.Count; i++)
         {
            ConfigNode cn = cfg.Root.ChildNodes[i];
            Console.WriteLine("Block name: {0}", cn.Name);
         }

         return true;
      }

      public bool LoadConfig()
      {
         cfg = new Config();
         cfg.ConfigFile.FullName = "test.cfg";
         return cfg.Load();
      }
   }
}
