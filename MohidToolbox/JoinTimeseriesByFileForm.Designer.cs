namespace Mohid
{
   partial class JoinTimeseriesByFileForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JoinTimeseriesByFileForm));
         this.AddButton = new System.Windows.Forms.Button();
         this.TimeseriesList = new System.Windows.Forms.ListBox();
         this.JoinButton = new System.Windows.Forms.Button();
         this.ExcludeButton = new System.Windows.Forms.Button();
         this.CloseButton = new System.Windows.Forms.Button();
         this.OutputButton = new System.Windows.Forms.Button();
         this.OutputTimeseriesTextbox = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.TimeUnitsCombobox = new System.Windows.Forms.ComboBox();
         this.FindOutputDialog = new System.Windows.Forms.OpenFileDialog();
         this.FindTimeseriesDialog = new System.Windows.Forms.OpenFileDialog();
         this.label2 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // AddButton
         // 
         this.AddButton.Image = ((System.Drawing.Image)(resources.GetObject("AddButton.Image")));
         this.AddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.AddButton.Location = new System.Drawing.Point(13, 96);
         this.AddButton.Name = "AddButton";
         this.AddButton.Size = new System.Drawing.Size(175, 62);
         this.AddButton.TabIndex = 0;
         this.AddButton.Text = "Add Timeseries";
         this.AddButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.AddButton.UseVisualStyleBackColor = true;
         this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
         // 
         // TimeseriesList
         // 
         this.TimeseriesList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.TimeseriesList.FormattingEnabled = true;
         this.TimeseriesList.Location = new System.Drawing.Point(192, 96);
         this.TimeseriesList.Name = "TimeseriesList";
         this.TimeseriesList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
         this.TimeseriesList.Size = new System.Drawing.Size(490, 329);
         this.TimeseriesList.TabIndex = 1;
         // 
         // JoinButton
         // 
         this.JoinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.JoinButton.Image = ((System.Drawing.Image)(resources.GetObject("JoinButton.Image")));
         this.JoinButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.JoinButton.Location = new System.Drawing.Point(13, 232);
         this.JoinButton.Name = "JoinButton";
         this.JoinButton.Size = new System.Drawing.Size(175, 62);
         this.JoinButton.TabIndex = 2;
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
         this.ExcludeButton.TabIndex = 3;
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
         this.CloseButton.TabIndex = 4;
         this.CloseButton.Text = "Close";
         this.CloseButton.UseVisualStyleBackColor = true;
         this.CloseButton.Click += new System.EventHandler(this.CancelButton_Click);
         // 
         // OutputButton
         // 
         this.OutputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.OutputButton.Image = ((System.Drawing.Image)(resources.GetObject("OutputButton.Image")));
         this.OutputButton.Location = new System.Drawing.Point(653, 8);
         this.OutputButton.Name = "OutputButton";
         this.OutputButton.Size = new System.Drawing.Size(30, 26);
         this.OutputButton.TabIndex = 6;
         this.OutputButton.UseVisualStyleBackColor = true;
         this.OutputButton.Click += new System.EventHandler(this.OutputButton_Click);
         // 
         // OutputTimeseriesTextbox
         // 
         this.OutputTimeseriesTextbox.Location = new System.Drawing.Point(105, 12);
         this.OutputTimeseriesTextbox.Name = "OutputTimeseriesTextbox";
         this.OutputTimeseriesTextbox.Size = new System.Drawing.Size(542, 20);
         this.OutputTimeseriesTextbox.TabIndex = 7;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(9, 45);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(57, 13);
         this.label1.TabIndex = 8;
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
         this.TimeUnitsCombobox.TabIndex = 9;
         // 
         // FindOutputDialog
         // 
         this.FindOutputDialog.CheckFileExists = false;
         this.FindOutputDialog.DefaultExt = "tsr";
         this.FindOutputDialog.Filter = "Timeseries files|*.srn;*.srp;*.srs;*.srb;*.tsr|All files|*.*";
         this.FindOutputDialog.Title = "Select Output Timeseries";
         // 
         // FindTimeseriesDialog
         // 
         this.FindTimeseriesDialog.DefaultExt = "tsr";
         this.FindTimeseriesDialog.Filter = "Timeseries files|*.srn;*.srp;*.srs;*.srb;*.tsr|All files|*.*";
         this.FindTimeseriesDialog.Multiselect = true;
         this.FindTimeseriesDialog.Title = "Add Timeseries";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(10, 15);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(92, 13);
         this.label2.TabIndex = 10;
         this.label2.Text = "Output Timeseries";
         // 
         // JoinTimeseriesByFileForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(694, 447);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.TimeUnitsCombobox);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.OutputTimeseriesTextbox);
         this.Controls.Add(this.OutputButton);
         this.Controls.Add(this.CloseButton);
         this.Controls.Add(this.ExcludeButton);
         this.Controls.Add(this.JoinButton);
         this.Controls.Add(this.TimeseriesList);
         this.Controls.Add(this.AddButton);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MinimumSize = new System.Drawing.Size(700, 475);
         this.Name = "JoinTimeseriesByFileForm";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Join Timeseries Tool (By File)";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button AddButton;
      private System.Windows.Forms.ListBox TimeseriesList;
      private System.Windows.Forms.Button JoinButton;
      private System.Windows.Forms.Button ExcludeButton;
      private System.Windows.Forms.Button CloseButton;
      private System.Windows.Forms.Button OutputButton;
      private System.Windows.Forms.TextBox OutputTimeseriesTextbox;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ComboBox TimeUnitsCombobox;
      private System.Windows.Forms.OpenFileDialog FindOutputDialog;
      private System.Windows.Forms.OpenFileDialog FindTimeseriesDialog;
      private System.Windows.Forms.Label label2;
   }
}