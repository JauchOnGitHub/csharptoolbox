namespace Mohid
{
   partial class JoinTimeseriesByFolderForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JoinTimeseriesByFolderForm));
         this.FindOutputDialog = new System.Windows.Forms.OpenFileDialog();
         this.FindFoldersDialog = new System.Windows.Forms.FolderBrowserDialog();
         this.OutputButton = new System.Windows.Forms.Button();
         this.OutputTimeseriesTextbox = new System.Windows.Forms.TextBox();
         this.AddButton = new System.Windows.Forms.Button();
         this.FoldersList = new System.Windows.Forms.ListBox();
         this.JoinButton = new System.Windows.Forms.Button();
         this.ExcludeButton = new System.Windows.Forms.Button();
         this.CloseButton = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.TimeUnitsCombobox = new System.Windows.Forms.ComboBox();
         this.FilterTextbox = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.SearchSubFoldersCheckbox = new System.Windows.Forms.CheckBox();
         this.label3 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // FindOutputDialog
         // 
         this.FindOutputDialog.CheckFileExists = false;
         this.FindOutputDialog.DefaultExt = "tsr";
         this.FindOutputDialog.Filter = "Timeseries files|*.srn;*.srp;*.srs;*.srb;*.tsr|All files|*.*";
         this.FindOutputDialog.Title = "Select Output Timeseries";
         // 
         // OutputButton
         // 
         this.OutputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.OutputButton.Image = ((System.Drawing.Image)(resources.GetObject("OutputButton.Image")));
         this.OutputButton.Location = new System.Drawing.Point(653, 8);
         this.OutputButton.Name = "OutputButton";
         this.OutputButton.Size = new System.Drawing.Size(30, 26);
         this.OutputButton.TabIndex = 8;
         this.OutputButton.UseVisualStyleBackColor = true;
         this.OutputButton.Click += new System.EventHandler(this.OutputButton_Click);
         // 
         // OutputTimeseriesTextbox
         // 
         this.OutputTimeseriesTextbox.Location = new System.Drawing.Point(105, 12);
         this.OutputTimeseriesTextbox.Name = "OutputTimeseriesTextbox";
         this.OutputTimeseriesTextbox.Size = new System.Drawing.Size(542, 20);
         this.OutputTimeseriesTextbox.TabIndex = 9;
         // 
         // AddButton
         // 
         this.AddButton.Image = ((System.Drawing.Image)(resources.GetObject("AddButton.Image")));
         this.AddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.AddButton.Location = new System.Drawing.Point(13, 96);
         this.AddButton.Name = "AddButton";
         this.AddButton.Size = new System.Drawing.Size(175, 62);
         this.AddButton.TabIndex = 10;
         this.AddButton.Text = "Add Folder";
         this.AddButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.AddButton.UseVisualStyleBackColor = true;
         this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
         // 
         // FoldersList
         // 
         this.FoldersList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.FoldersList.FormattingEnabled = true;
         this.FoldersList.Location = new System.Drawing.Point(192, 96);
         this.FoldersList.Name = "FoldersList";
         this.FoldersList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
         this.FoldersList.Size = new System.Drawing.Size(490, 329);
         this.FoldersList.TabIndex = 11;
         // 
         // JoinButton
         // 
         this.JoinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.JoinButton.Image = ((System.Drawing.Image)(resources.GetObject("JoinButton.Image")));
         this.JoinButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.JoinButton.Location = new System.Drawing.Point(13, 232);
         this.JoinButton.Name = "JoinButton";
         this.JoinButton.Size = new System.Drawing.Size(175, 62);
         this.JoinButton.TabIndex = 12;
         this.JoinButton.Text = "Join Timeseries";
         this.JoinButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.JoinButton.UseVisualStyleBackColor = true;
         this.JoinButton.Click += new System.EventHandler(this.JoinButton_Click);
         // 
         // ExcludeButton
         // 
         this.ExcludeButton.Image = ((System.Drawing.Image)(resources.GetObject("ExcludeButton.Image")));
         this.ExcludeButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.ExcludeButton.Location = new System.Drawing.Point(13, 164);
         this.ExcludeButton.Name = "ExcludeButton";
         this.ExcludeButton.Size = new System.Drawing.Size(175, 62);
         this.ExcludeButton.TabIndex = 13;
         this.ExcludeButton.Text = "Exclude Selected";
         this.ExcludeButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.ExcludeButton.UseVisualStyleBackColor = true;
         this.ExcludeButton.Click += new System.EventHandler(this.ExcludeButton_Click);
         // 
         // CloseButton
         // 
         this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.CloseButton.Location = new System.Drawing.Point(11, 363);
         this.CloseButton.Name = "CloseButton";
         this.CloseButton.Size = new System.Drawing.Size(175, 62);
         this.CloseButton.TabIndex = 14;
         this.CloseButton.Text = "Close";
         this.CloseButton.UseVisualStyleBackColor = true;
         this.CloseButton.Click += new System.EventHandler(this.CancelButton_Click);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(9, 45);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(57, 13);
         this.label1.TabIndex = 15;
         this.label1.Text = "Time Units";
         // 
         // TimeUnitsCombobox
         // 
         this.TimeUnitsCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.TimeUnitsCombobox.FormattingEnabled = true;
         this.TimeUnitsCombobox.Items.AddRange(new object[] {
            "SECONDS",
            "MINUTES",
            "HOURS",
            "DAYS",
            "MONTHS",
            "YEARS"});
         this.TimeUnitsCombobox.Location = new System.Drawing.Point(72, 42);
         this.TimeUnitsCombobox.Name = "TimeUnitsCombobox";
         this.TimeUnitsCombobox.Size = new System.Drawing.Size(219, 21);
         this.TimeUnitsCombobox.TabIndex = 16;
         // 
         // FilterTextbox
         // 
         this.FilterTextbox.Location = new System.Drawing.Point(407, 42);
         this.FilterTextbox.Name = "FilterTextbox";
         this.FilterTextbox.Size = new System.Drawing.Size(276, 20);
         this.FilterTextbox.TabIndex = 17;
         this.FilterTextbox.Text = "*.tsr";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(372, 45);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(29, 13);
         this.label2.TabIndex = 18;
         this.label2.Text = "Filter";
         // 
         // SearchSubFoldersCheckbox
         // 
         this.SearchSubFoldersCheckbox.AutoSize = true;
         this.SearchSubFoldersCheckbox.Checked = true;
         this.SearchSubFoldersCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
         this.SearchSubFoldersCheckbox.Location = new System.Drawing.Point(569, 72);
         this.SearchSubFoldersCheckbox.Name = "SearchSubFoldersCheckbox";
         this.SearchSubFoldersCheckbox.Size = new System.Drawing.Size(114, 17);
         this.SearchSubFoldersCheckbox.TabIndex = 19;
         this.SearchSubFoldersCheckbox.Text = "Search sub-folders";
         this.SearchSubFoldersCheckbox.UseVisualStyleBackColor = true;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(10, 15);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(92, 13);
         this.label3.TabIndex = 20;
         this.label3.Text = "Output Timeseries";
         // 
         // JoinTimeseriesByFolderForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(694, 447);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.SearchSubFoldersCheckbox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.FilterTextbox);
         this.Controls.Add(this.TimeUnitsCombobox);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.CloseButton);
         this.Controls.Add(this.ExcludeButton);
         this.Controls.Add(this.JoinButton);
         this.Controls.Add(this.FoldersList);
         this.Controls.Add(this.AddButton);
         this.Controls.Add(this.OutputTimeseriesTextbox);
         this.Controls.Add(this.OutputButton);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MinimumSize = new System.Drawing.Size(700, 475);
         this.Name = "JoinTimeseriesByFolderForm";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Join Timeseries Tool (By Folder)";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.OpenFileDialog FindOutputDialog;
      private System.Windows.Forms.FolderBrowserDialog FindFoldersDialog;
      private System.Windows.Forms.Button OutputButton;
      private System.Windows.Forms.TextBox OutputTimeseriesTextbox;
      private System.Windows.Forms.Button AddButton;
      private System.Windows.Forms.ListBox FoldersList;
      private System.Windows.Forms.Button JoinButton;
      private System.Windows.Forms.Button ExcludeButton;
      private System.Windows.Forms.Button CloseButton;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ComboBox TimeUnitsCombobox;
      private System.Windows.Forms.TextBox FilterTextbox;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.CheckBox SearchSubFoldersCheckbox;
      private System.Windows.Forms.Label label3;
   }
}