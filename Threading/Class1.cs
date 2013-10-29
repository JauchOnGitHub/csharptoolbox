using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace Mohid
{
   namespace Threading
   {
      public class BaseThreading : BackgroundWorker
      {
         //private BackgroundWorker worker;

         public BaseThreading()
         {
         }

         public void Run(object options)
         {
            //worker = new BackgroundWorker();

            //.DoWork += new DoWorkEventHandler(DoWork);
            //this.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            //worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            //worker.WorkerReportsProgress = true;
            //worker.WorkerSupportsCancellation = true;

            RunWorkerAsync(options);
         }

         private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
         {

         }

         private void ProgressChanged(object sender, ProgressChangedEventArgs e)
         {
         }
      }
   }
}