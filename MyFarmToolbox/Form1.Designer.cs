namespace MyFarmToolbox
{
   partial class MyFarmMainForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.meteoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.resultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.farmersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
         this.menuStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.meteoToolStripMenuItem,
            this.resultsToolStripMenuItem,
            this.farmersToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(944, 24);
         this.menuStrip1.TabIndex = 0;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // fileToolStripMenuItem
         // 
         this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
         this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
         this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
         this.fileToolStripMenuItem.Text = "&File";
         // 
         // exitToolStripMenuItem
         // 
         this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
         this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
         this.exitToolStripMenuItem.Text = "&Exit";
         this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
         // 
         // statusStrip1
         // 
         this.statusStrip1.Location = new System.Drawing.Point(0, 483);
         this.statusStrip1.Name = "statusStrip1";
         this.statusStrip1.Size = new System.Drawing.Size(944, 22);
         this.statusStrip1.TabIndex = 1;
         this.statusStrip1.Text = "statusStrip1";
         // 
         // meteoToolStripMenuItem
         // 
         this.meteoToolStripMenuItem.Name = "meteoToolStripMenuItem";
         this.meteoToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
         this.meteoToolStripMenuItem.Text = "&Meteo";
         // 
         // resultsToolStripMenuItem
         // 
         this.resultsToolStripMenuItem.Name = "resultsToolStripMenuItem";
         this.resultsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
         this.resultsToolStripMenuItem.Text = "&Results";
         // 
         // farmersToolStripMenuItem
         // 
         this.farmersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripMenuItem2});
         this.farmersToolStripMenuItem.Name = "farmersToolStripMenuItem";
         this.farmersToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
         this.farmersToolStripMenuItem.Text = "&Farmers";
         // 
         // newToolStripMenuItem
         // 
         this.newToolStripMenuItem.Name = "newToolStripMenuItem";
         this.newToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
         this.newToolStripMenuItem.Text = "&New";
         // 
         // toolStripMenuItem2
         // 
         this.toolStripMenuItem2.Name = "toolStripMenuItem2";
         this.toolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
         this.toolStripMenuItem2.Text = "&";
         // 
         // MyFarmMainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(944, 505);
         this.Controls.Add(this.statusStrip1);
         this.Controls.Add(this.menuStrip1);
         this.Name = "MyFarmMainForm";
         this.Text = "MyFarm Tool Box";
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
      private System.Windows.Forms.StatusStrip statusStrip1;
      private System.Windows.Forms.ToolStripMenuItem meteoToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem resultsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem farmersToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;

   }
}

