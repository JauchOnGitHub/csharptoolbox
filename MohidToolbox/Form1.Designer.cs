namespace Mohid
{
   partial class Form1
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
         this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.AddButton = new System.Windows.Forms.Button();
         this.button1 = new System.Windows.Forms.Button();
         this.button2 = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // checkedListBox1
         // 
         this.checkedListBox1.FormattingEnabled = true;
         this.checkedListBox1.Location = new System.Drawing.Point(12, 78);
         this.checkedListBox1.Name = "checkedListBox1";
         this.checkedListBox1.Size = new System.Drawing.Size(234, 274);
         this.checkedListBox1.TabIndex = 0;
         // 
         // textBox1
         // 
         this.textBox1.Location = new System.Drawing.Point(12, 12);
         this.textBox1.Multiline = true;
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(777, 60);
         this.textBox1.TabIndex = 1;
         // 
         // AddButton
         // 
         this.AddButton.Image = ((System.Drawing.Image)(resources.GetObject("AddButton.Image")));
         this.AddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.AddButton.Location = new System.Drawing.Point(252, 290);
         this.AddButton.Name = "AddButton";
         this.AddButton.Size = new System.Drawing.Size(175, 62);
         this.AddButton.TabIndex = 11;
         this.AddButton.Text = "Add Folder";
         this.AddButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.AddButton.UseVisualStyleBackColor = true;
         // 
         // button1
         // 
         this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
         this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button1.Location = new System.Drawing.Point(433, 290);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(175, 62);
         this.button1.TabIndex = 12;
         this.button1.Text = "Add Folder";
         this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.button1.UseVisualStyleBackColor = true;
         // 
         // button2
         // 
         this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
         this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button2.Location = new System.Drawing.Point(614, 290);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(175, 62);
         this.button2.TabIndex = 13;
         this.button2.Text = "Add Folder";
         this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
         this.button2.UseVisualStyleBackColor = true;
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(801, 367);
         this.Controls.Add(this.button2);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.AddButton);
         this.Controls.Add(this.textBox1);
         this.Controls.Add(this.checkedListBox1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "Form1";
         this.Text = "Form1";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.CheckedListBox checkedListBox1;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.Button AddButton;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Button button2;
   }
}