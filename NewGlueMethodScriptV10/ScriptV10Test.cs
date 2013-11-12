using Mohid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NewGlueMethodScriptV10
{
    
    
    /// <summary>
    ///This is a test class for ScriptV10Test and is intended
    ///to contain all ScriptV10Test Unit Tests
    ///</summary>
   [TestClass()]
   public class ScriptV10Test
   {


      private TestContext testContextInstance;

      /// <summary>
      ///Gets or sets the test context which provides
      ///information about and functionality for the current test run.
      ///</summary>
      public TestContext TestContext
      {
         get
         {
            return testContextInstance;
         }
         set
         {
            testContextInstance = value;
         }
      }

      #region Additional test attributes
      // 
      //You can use the following additional attributes as you write your tests:
      //
      //Use ClassInitialize to run code before running the first test in the class
      //[ClassInitialize()]
      //public static void MyClassInitialize(TestContext testContext)
      //{
      //}
      //
      //Use ClassCleanup to run code after all tests in a class have run
      //[ClassCleanup()]
      //public static void MyClassCleanup()
      //{
      //}
      //
      //Use TestInitialize to run code before running each test
      //[TestInitialize()]
      //public void MyTestInitialize()
      //{
      //}
      //
      //Use TestCleanup to run code after each test has run
      //[TestCleanup()]
      //public void MyTestCleanup()
      //{
      //}
      //
      #endregion


      /// <summary>
      ///A test for GlueBasinEVTPFile
      ///</summary>
      [TestMethod()]
      [DeploymentItem("GenericScript.dll")]
      public void GlueBasinEVTPFileFindFilesInInterval()
      {
         ScriptV10_Accessor target = new ScriptV10_Accessor(); // TODO: Initialize to an appropriate value

         target.glue_all_basin_evtp = false;
         target.glue_basin_evtp_since = true;
         target.glue_basin_evtp_since_days = 30;
         target.mred = new Mohid.Simulation.MohidRunEngineData();
         target.mred.storeFolder = new Mohid.Files.FilePath(@"L:\Portugal\Douro\Tamega\MyWater\Simulations\MohidLand\100x50.operational\operational.reference\model.results");
         target.basin_evtp_file_name = "basin.evtp.hdf5";
         target.basin_evtp_output_filename = new Mohid.Files.FileName(@"..\..\operational.reference\cumulative\basin.evtp.hdf5");
         target.tool_glue = new Mohid.HDF.HDFGlue();
         target.tool_glue.

         target.GlueBasinEVTPFile();
         Assert.Inconclusive("A method that does not return a value cannot be verified.");
      }
   }
}
