using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace Mohid
{
   public partial class Running : Form
   {
      private BackgroundWorker bw;
      private bool supportsCancellation,
                   reportsProgress,
                   proccessCancelled;
      private object options;

      public String LabelText
      {
         get { return label1.Text; }
         set { label1.Text = value; }
      }

      public BackgroundWorker Worker
      {
         get { return bw; }
         set { bw = value; }
      }

      public bool SupportsCancellation
      {
         get { return supportsCancellation; }
         set { supportsCancellation = value; }
      }

      public bool ReportsProgress
      {
         get { return reportsProgress; }
         set { reportsProgress = value; }
      }

      public object Options
      {
         get { return options; }
         set { options = value; }
      }

      public Running()
      {
         supportsCancellation = false;
         reportsProgress = false;
         proccessCancelled = false;
         options = null;

         InitializeComponent();
      }

      private void Run()
      {
         if (supportsCancellation)
         {
            bw.WorkerSupportsCancellation = true;
         }

         if (reportsProgress)
         {
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
         }

         bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);

         bw.RunWorkerAsync(options);       
      }

      private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         Close();
      }

      private void ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         if (!proccessCancelled)
            progressBar1.Value = e.ProgressPercentage;
      }

      private void CancelButton_Click(object sender, EventArgs e)
      {
         proccessCancelled = true;
         label1.Text = "Cancelling proccess...";
         bw.CancelAsync();
      }

      private void Running_Load(object sender, EventArgs e)
      {
         Run();
      }
   }
}
