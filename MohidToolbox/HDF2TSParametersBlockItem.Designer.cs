namespace Mohid
{
   partial class HDF2TSParametersBlockItem
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HDF2TSParametersBlockItem));
         this.button1 = new System.Windows.Forms.Button();
         this.DeleteButton = new System.Windows.Forms.Button();
         this.SaveButton = new System.Windows.Forms.Button();
         this.NewButton = new System.Windows.Forms.Button();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.NameTextbox = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.ParametersCheckedlistbox = new System.Windows.Forms.CheckedListBox();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.groupBox1.SuspendLayout();
         this.SuspendLayout();
         // 
         // button1
         // 
         this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
         this.button1.Location = new System.Drawing.Point(11, 264);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(175, 76);
         this.button1.TabIndex = 43;
         this.button1.Text = "Close";
         this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.button1.UseVisualStyleBackColor = true;
         // 
         // DeleteButton
         // 
         this.DeleteButton.Location = new System.Drawing.Point(476, 181);
         this.DeleteButton.Name = "DeleteButton";
         this.DeleteButton.Size = new System.Drawing.Size(118, 78);
         this.DeleteButton.TabIndex = 42;
         this.DeleteButton.Text = "Delete";
         this.DeleteButton.UseVisualStyleBackColor = true;
         // 
         // SaveButton
         // 
         this.SaveButton.Location = new System.Drawing.Point(355, 181);
         this.SaveButton.Name = "SaveButton";
         this.SaveButton.Size = new System.Drawing.Size(118, 78);
         this.SaveButton.TabIndex = 41;
         this.SaveButton.Text = "Save";
         this.SaveButton.UseVisualStyleBackColor = true;
         // 
         // NewButton
         // 
         this.NewButton.Location = new System.Drawing.Point(234, 181);
         this.NewButton.Name = "NewButton";
         this.NewButton.Size = new System.Drawing.Size(118, 78);
         this.NewButton.TabIndex = 40;
         this.NewButton.Text = " New";
         this.NewButton.UseVisualStyleBackColor = true;
         // 
         // groupBox1
         // 
         this.groupBox1.Controls.Add(this.textBox1);
         this.groupBox1.Controls.Add(this.label3);
         this.groupBox1.Controls.Add(this.NameTextbox);
         this.groupBox1.Controls.Add(this.label2);
         this.groupBox1.Location = new System.Drawing.Point(232, 23);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(362, 68);
         this.groupBox1.TabIndex = 39;
         this.groupBox1.TabStop = false;
         // 
         // NameTextbox
         // 
         this.NameTextbox.Location = new System.Drawing.Point(47, 13);
         this.NameTextbox.Name = "NameTextbox";
         this.NameTextbox.Size = new System.Drawing.Size(302, 20);
         this.NameTextbox.TabIndex = 1;
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(6, 16);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(35, 13);
         this.label2.TabIndex = 0;
         this.label2.Text = "Name";
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(11, 11);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(79, 13);
         this.label1.TabIndex = 38;
         this.label1.Text = "Parameters List";
         // 
         // ParametersCheckedlistbox
         // 
         this.ParametersCheckedlistbox.FormattingEnabled = true;
         this.ParametersCheckedlistbox.Location = new System.Drawing.Point(12, 29);
         this.ParametersCheckedlistbox.Name = "ParametersCheckedlistbox";
         this.ParametersCheckedlistbox.Size = new System.Drawing.Size(214, 229);
         this.ParametersCheckedlistbox.TabIndex = 37;
         // 
         // textBox1
         // 
         this.textBox1.Location = new System.Drawing.Point(47, 39);
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(302, 20);
         this.textBox1.TabIndex = 3;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(6, 42);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(36, 13);
         this.label3.TabIndex = 2;
         this.label3.Text = "Group";
         // 
         // HDF2TSParametersBlockItem
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(606, 347);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.DeleteButton);
         this.Controls.Add(this.SaveButton);
         this.Controls.Add(this.NewButton);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.ParametersCheckedlistbox);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "HDF2TSParametersBlockItem";
         this.Text = "Parameters Block (HDF to Timeseries Tool)";
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Button DeleteButton;
      private System.Windows.Forms.Button SaveButton;
      private System.Windows.Forms.Button NewButton;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.TextBox NameTextbox;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.CheckedListBox ParametersCheckedlistbox;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.Label label3;
   }
}