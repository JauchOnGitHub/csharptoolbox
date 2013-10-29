namespace Mohid
{
   partial class MainForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.mohidToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.applicationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.joinTimeseriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.byFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.byFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.hDFToTimeseriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.menuStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mohidToolsToolStripMenuItem,
            this.editToolStripMenuItem,
            this.applicationsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.helpToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(859, 24);
         this.menuStrip1.TabIndex = 0;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // mohidToolsToolStripMenuItem
         // 
         this.mohidToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem,
            this.exitToolStripMenuItem});
         this.mohidToolsToolStripMenuItem.Name = "mohidToolsToolStripMenuItem";
         this.mohidToolsToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
         this.mohidToolsToolStripMenuItem.Text = "&File";
         // 
         // configurationToolStripMenuItem
         // 
         this.configurationToolStripMenuItem.Enabled = false;
         this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
         this.configurationToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
         this.configurationToolStripMenuItem.Text = "&Configuration";
         // 
         // exitToolStripMenuItem
         // 
         this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
         this.exitToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
         this.exitToolStripMenuItem.Text = "E&xit";
         this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
         // 
         // editToolStripMenuItem
         // 
         this.editToolStripMenuItem.Name = "editToolStripMenuItem";
         this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
         this.editToolStripMenuItem.Text = "&Edit";
         // 
         // applicationsToolStripMenuItem
         // 
         this.applicationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.joinTimeseriesToolStripMenuItem,
            this.exportToolStripMenuItem});
         this.applicationsToolStripMenuItem.Name = "applicationsToolStripMenuItem";
         this.applicationsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
         this.applicationsToolStripMenuItem.Text = "&Tools";
         // 
         // joinTimeseriesToolStripMenuItem
         // 
         this.joinTimeseriesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.byFileToolStripMenuItem,
            this.byFolderToolStripMenuItem});
         this.joinTimeseriesToolStripMenuItem.Name = "joinTimeseriesToolStripMenuItem";
         this.joinTimeseriesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
         this.joinTimeseriesToolStripMenuItem.Text = "&Join Timeseries";
         // 
         // byFileToolStripMenuItem
         // 
         this.byFileToolStripMenuItem.Name = "byFileToolStripMenuItem";
         this.byFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
         this.byFileToolStripMenuItem.Text = "By &File";
         this.byFileToolStripMenuItem.Click += new System.EventHandler(this.byFileToolStripMenuItem_Click);
         // 
         // byFolderToolStripMenuItem
         // 
         this.byFolderToolStripMenuItem.Name = "byFolderToolStripMenuItem";
         this.byFolderToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
         this.byFolderToolStripMenuItem.Text = "By F&older";
         this.byFolderToolStripMenuItem.Click += new System.EventHandler(this.byFolderToolStripMenuItem_Click);
         // 
         // pluginsToolStripMenuItem
         // 
         this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
         this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
         this.pluginsToolStripMenuItem.Text = "&Plugins";
         // 
         // helpToolStripMenuItem
         // 
         this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
         this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
         this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
         this.helpToolStripMenuItem.Text = "&Help";
         // 
         // aboutToolStripMenuItem
         // 
         this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
         this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
         this.aboutToolStripMenuItem.Text = "&About";
         this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
         // 
         // exportToolStripMenuItem
         // 
         this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hDFToTimeseriesToolStripMenuItem});
         this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
         this.exportToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
         this.exportToolStripMenuItem.Text = "&Export";
         // 
         // hDFToTimeseriesToolStripMenuItem
         // 
         this.hDFToTimeseriesToolStripMenuItem.Name = "hDFToTimeseriesToolStripMenuItem";
         this.hDFToTimeseriesToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
         this.hDFToTimeseriesToolStripMenuItem.Text = "&HDF to Timeseries";
         this.hDFToTimeseriesToolStripMenuItem.Click += new System.EventHandler(this.hDFToTimeseriesToolStripMenuItem_Click);
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(859, 304);
         this.Controls.Add(this.menuStrip1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "MainForm";
         this.Text = "Mohid Tools (Beta)";
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem mohidToolsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem applicationsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem joinTimeseriesToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem byFileToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem byFolderToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem hDFToTimeseriesToolStripMenuItem;
   }
}