using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using Mohid.Files;
using Mohid.CommandArguments;

namespace Mohid
{
   namespace Script
   {
      public enum ScriptLanguage
      {
         BYEXTENSION,
         VB,
         CSHARP
      }

      public class ScriptCompiler
      {
         #region PRIVATE DATA

         private List<string> dllList;

         #endregion PRIVATE DATA

         #region PUBLIC DATA

         public ScriptLanguage Language { get; set; }

         #endregion PUBLIC DATA

         #region INITIALIZATION

         protected virtual void Init()
         {           
            Language = ScriptLanguage.BYEXTENSION;
            dllList = new List<string>();
         }

         public ScriptCompiler()
         {
            Init();
         }

         #endregion INITIALIZATION

         #region ENGINE

         public bool Run(FileName script, CmdArgs cmdArgs)
         {
            Assembly compiledAssembly = Compile(script);
            IMohidScript scriptInterface = (IMohidScript)FindScriptInterface("IMohidScript", compiledAssembly);

            if (scriptInterface == null)
               return false;

            return scriptInterface.Run(cmdArgs);
         }         

         #endregion ENGINE

         #region SCRIPT ASSEMBLAGE

         public Assembly Compile(FileName scriptFileName, ScriptLanguage language = ScriptLanguage.BYEXTENSION)
         {
            if (language == ScriptLanguage.BYEXTENSION)
            {
               switch (scriptFileName.Extension)
               {
                  case "cs":
                     language = ScriptLanguage.CSHARP;
                     break;
                  case "vb":
                     language = ScriptLanguage.VB;
                     break;
                  default:
                     throw new Exception("Unknown script extension: '" + scriptFileName.Extension + "'");                     
               }
            }

            switch (language)
            {
               case ScriptLanguage.VB:
                  throw new Exception("VB scripts are not yet implemented.");               
               case ScriptLanguage.CSHARP:
                  return CompileCSharpScript(LoadCSharpScript(scriptFileName));                  
               default:
                  return null;
            }
         }

         private string LoadCSharpScript(FileName scriptFileName)
         {
            TextFile scriptFile = new TextFile(scriptFileName);
            scriptFile.OpenToRead();

            List<string> script = scriptFile.ReadLines();

            int lineNumber = 0;
            string line;
            string[] dllName;
            string[] seps = { ":" };

            for (lineNumber = 0; lineNumber < script.Count; lineNumber++)
            {
               line = script[lineNumber].Trim();
               if (line == "") continue;

               if (line.StartsWith("//DLLNAME:"))
               {
                  dllName = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                  if (!dllName[1].Trim().EndsWith(".dll"))
                     dllName[1] = dllName[1].Trim() + ".dll";                  
                     
                  dllList.Add(dllName[1].Trim());
               }
            }

            StringBuilder temp = new StringBuilder();
            for (lineNumber = 0; lineNumber < script.Count; lineNumber++)
               temp.AppendLine(script[lineNumber]);

            scriptFile.Close();

            return temp.ToString();
         }

         private Assembly CompileCSharpScript(string script)
         {
            //Declare a compiler
            CodeDomProvider compiler = CodeDomProvider.CreateProvider("CSharp");

            //Set the compiler parameters
            CompilerParameters parameters = new CompilerParameters();
            parameters.CompilerOptions = "/optimize";
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = false;

            foreach (string dll in dllList)
               parameters.ReferencedAssemblies.Add(dll);

            //Compile Assembly
            CompilerResults compilerResults = compiler.CompileAssemblyFromSource(parameters, script);


            //Do we have any compiler errors?
            if (compilerResults.Errors.Count > 0)
            {
               Console.WriteLine("There are errors in the script.");
               foreach (CompilerError err in compilerResults.Errors)
                  Console.WriteLine(err.ErrorText);
               return null;
            }

            return compilerResults.CompiledAssembly;
         }

         #endregion SCRIPT ASSEMBLAGE

         #region AUX

         public object FindScriptInterface(string interfaceName, Assembly compiledAssembly)
         {
            if (compiledAssembly == null)
               return null;

            //Loop through types looking for one that implements the given interface name
            foreach (Type t in compiledAssembly.GetTypes())
            {
               if (t.GetInterface(interfaceName, true) != null)
                  return compiledAssembly.CreateInstance(t.FullName);
            }

            return null;
         }

         #endregion AUX
      }
   }
}