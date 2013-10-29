using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace Mohid
{
   namespace Software
   {
      class BackgroundApp
      {
         private BackgroundWorker worker;
         public Options Options;         

         public BackgroundApp()
         {
         }

         public void Run()
         {
            worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler(DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerAsync(Options);
         }

         private void DoWork(object sender, DoWorkEventArgs e)
         {
            ExternalApp app = new ExternalApp();
            Options opts = sender as Options;

            app.Options = sender as Options;

            if (!app.Run())
            {
               //MessageBox.Show("HDFExporter tool has failed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return;
            }
         }

         private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
         {

         }

         private void ProgressChanged(object sender, ProgressChangedEventArgs e)
         {
         }

         public void Cancel()
         {
            if (worker != null)
            {
               worker.CancelAsync();
               worker = null;
            }
         }
      }
   }
}
