﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyFarmToolbox
{
   public partial class MyFarmMainForm : Form
   {
      public MyFarmMainForm()
      {
         InitializeComponent();
      }

      private void exitToolStripMenuItem_Click(object sender, EventArgs e)
      {
         Close();
      }

   }
}
