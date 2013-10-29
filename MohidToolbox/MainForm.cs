using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mohid
{
   public partial class MainForm : Form
   {
      public MainForm()
      {
         InitializeComponent();
      }

      private void exitToolStripMenuItem_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void byFileToolStripMenuItem_Click(object sender, EventArgs e)
      {
         JoinTimeseriesByFileForm jtf = new JoinTimeseriesByFileForm();
         jtf.ShowDialog(this);
      }

      private void byFolderToolStripMenuItem_Click(object sender, EventArgs e)
      {
         JoinTimeseriesByFolderForm jtf = new JoinTimeseriesByFolderForm();
         jtf.ShowDialog(this);
      }

      private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
      {
         MohidToolboxAboutBox about = new MohidToolboxAboutBox();
         about.ShowDialog();
      }

      private void hDFToTimeseriesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         ExportHDFByFolderForm form = new ExportHDFByFolderForm();
         form.Show();
      }
   }
}
