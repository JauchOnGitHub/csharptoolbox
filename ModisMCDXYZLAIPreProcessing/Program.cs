using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mohid.Core;
using Mohid.CommandArguments;
using Mohid.Configuration;
using Mohid.MohidTimeSeries;
using Mohid.Files;
using Mohid.Software;

namespace ModisMCDXYZLAIPreProcessing
{
   class Program
   {
      static void Main(string[] args)
      {
         bool verbose = false;

         try
         {


            CmdArgs cmdArgs = new CmdArgs(args);

            if (cmdArgs.HasOption("v"))
               verbose = true;
            else
               verbose = false;


            if (cmdArgs.HasParameter("cfg"))
            {
               if (verbose)
               {
                  Console.WriteLine("");
                  Console.Write("Reading configuration file...");
               }

               Config conf = new Config(cmdArgs.Parameters["cfg"]);
               conf.Load();

               if (verbose)
                  Console.WriteLine("[OK]");

               if (verbose)
                  Console.Write("Looking for 'xyz.to.process' blocks...");

               List<ConfigNode> bkList = conf.Root.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "xyz.to.process"; });

               if (bkList == null || bkList.Count <= 0)
                  throw new Exception("No 'xyz.to.process' block found in configuration file.");

               if (verbose)
               {
                  Console.WriteLine("[OK]");
                  Console.WriteLine("{0} 'xyz.to.process' block(s) found.", bkList.Count);
               }

               int bkCount = 1;
               foreach (ConfigNode bk in bkList)
               {
                  if (verbose)
                     Console.Write("Processing 'xyz.to.process' block #{0}...", bkCount);

                  FileName input = bk["input.file"].AsFileName();
                  FileName output = bk["output.file"].AsFileName();
                  bool eraseNoData = bk["erase.no.data", true].AsBool();
                  double noData = -99.0;
                  if (eraseNoData)
                     noData = bk["no.data.value", -99.0].AsDouble();
                  bool applyScaleFator = bk["apply.scale.fator", true].AsBool();
                  double scaleFactor = 1.0;
                  if (applyScaleFator)
                     scaleFactor = bk["scale.factor", 1.0].AsDouble();

                  Dictionary<string, string> lookupTable = new Dictionary<string, string>();
                  List<ConfigNode> lookupTables = bk.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "lookup.table"; });

                  foreach (ConfigNode lt in lookupTables)
                  {
                     foreach (KeyValuePair<string, KeywordData> kp in lt.NodeData)
                        lookupTable[kp.Key] = kp.Value.AsString();
                  }

                  //Start processing XYZ
                  TextFile file_i = new TextFile(input); file_i.OpenToRead();
                  TextFile file_o = new TextFile(output); file_o.OpenNewToWrite();

                  string line_i = null;
                  string line_o = null;
                  string [] tokens;
                  string [] sep = {" "};
                  double res;
                  while ((line_i = file_i.ReadLine()) != null)
                  {
                     tokens = line_i.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                     if (tokens.Length == 3)
                     {
                        foreach (KeyValuePair<string, string> kp in lookupTable)
                           if (double.Parse(tokens[2]) == double.Parse(kp.Key))
                           {
                              tokens[2] = kp.Value;
                              break;
                           }

                        res = double.Parse(tokens[2]);

                        if (eraseNoData && res == noData)
                           continue;

                        if (applyScaleFator)                                                   
                           res *= scaleFactor;                                                      
                        
                        line_o = string.Format("{0} {1} {2}  ", tokens[0], tokens[1], res);  
                     }
                     else
                        line_o = line_i;

                     file_o.WriteLine(line_o);
                  }

                  file_i.Close();
                  file_o.Close();

                  bkCount++;
                  if (verbose)
                     Console.WriteLine("[OK]");
               }
            }
         }
         catch (Exception ex)
         {
            if (verbose)
            {
               Console.WriteLine("[FAIL]");
               Console.WriteLine("");
               Console.WriteLine("An EXCEPTION was raised. The message returned was:");
               Console.WriteLine(ex.Message);
            }
         }
         if (verbose)
         {
            Console.WriteLine("Process finished.");
            Console.WriteLine("");
         }
      }
   }
}
