namespace Mohid
{
   partial class Running
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Running));
         this.progressBar1 = new System.Windows.Forms.ProgressBar();
         this.label1 = new System.Windows.Forms.Label();
         this.MyCancelButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // progressBar1
         // 
         this.progressBar1.Location = new System.Drawing.Point(12, 25);
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(366, 23);
         this.progressBar1.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(12, 9);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(366, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "label1";
         // 
         // MyCancelButton
         // 
         this.MyCancelButton.Image = ((System.Drawing.Image)(resources.GetObject("MyCancelButton.Image")));
         this.MyCancelButton.Location = new System.Drawing.Point(119, 57);
         this.MyCancelButton.Name = "MyCancelButton";
         this.MyCancelButton.Size = new System.Drawing.Size(153, 55);
         this.MyCancelButton.TabIndex = 2;
         this.MyCancelButton.Text = "Cancel";
         this.MyCancelButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.MyCancelButton.UseVisualStyleBackColor = true;
         this.MyCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
         // 
         // Running
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(390, 124);
         this.Controls.Add(this.MyCancelButton);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.progressBar1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "Running";
         this.Text = "Running";
         this.Load += new System.EventHandler(this.Running_Load);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ProgressBar progressBar1;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button MyCancelButton;
   }
}