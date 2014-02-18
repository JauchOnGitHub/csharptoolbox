namespace CPTECGluer
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
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.label1 = new System.Windows.Forms.Label();
         this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
         this.button1 = new System.Windows.Forms.Button();
         this.button2 = new System.Windows.Forms.Button();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.progressBar1 = new System.Windows.Forms.ProgressBar();
         this.checkBox1 = new System.Windows.Forms.CheckBox();
         this.label3 = new System.Windows.Forms.Label();
         this.textBox2 = new System.Windows.Forms.TextBox();
         this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
         this.groupBox1.SuspendLayout();
         this.SuspendLayout();
         // 
         // groupBox1
         // 
         this.groupBox1.Controls.Add(this.label3);
         this.groupBox1.Controls.Add(this.textBox2);
         this.groupBox1.Controls.Add(this.checkBox1);
         this.groupBox1.Controls.Add(this.label2);
         this.groupBox1.Controls.Add(this.textBox1);
         this.groupBox1.Controls.Add(this.dateTimePicker1);
         this.groupBox1.Controls.Add(this.label1);
         this.groupBox1.Location = new System.Drawing.Point(11, 12);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(325, 125);
         this.groupBox1.TabIndex = 0;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "Input";
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(10, 21);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(55, 13);
         this.label1.TabIndex = 0;
         this.label1.Text = "Start Date";
         // 
         // dateTimePicker1
         // 
         this.dateTimePicker1.Location = new System.Drawing.Point(82, 17);
         this.dateTimePicker1.Name = "dateTimePicker1";
         this.dateTimePicker1.Size = new System.Drawing.Size(231, 20);
         this.dateTimePicker1.TabIndex = 1;
         // 
         // button1
         // 
         this.button1.Location = new System.Drawing.Point(11, 171);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(160, 44);
         this.button1.TabIndex = 1;
         this.button1.Text = "START";
         this.button1.UseVisualStyleBackColor = true;
         // 
         // button2
         // 
         this.button2.Location = new System.Drawing.Point(178, 171);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(159, 44);
         this.button2.TabIndex = 2;
         this.button2.Text = "STOP";
         this.button2.UseVisualStyleBackColor = true;
         // 
         // textBox1
         // 
         this.textBox1.Location = new System.Drawing.Point(82, 43);
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(231, 20);
         this.textBox1.TabIndex = 2;
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(10, 46);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(66, 13);
         this.label2.TabIndex = 3;
         this.label2.Text = "Search Path";
         // 
         // progressBar1
         // 
         this.progressBar1.Location = new System.Drawing.Point(11, 143);
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(323, 22);
         this.progressBar1.TabIndex = 3;
         // 
         // checkBox1
         // 
         this.checkBox1.AutoSize = true;
         this.checkBox1.Location = new System.Drawing.Point(134, 69);
         this.checkBox1.Name = "checkBox1";
         this.checkBox1.Size = new System.Drawing.Size(179, 17);
         this.checkBox1.TabIndex = 4;
         this.checkBox1.Text = "Include sub-folders in the search";
         this.checkBox1.UseVisualStyleBackColor = true;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(10, 95);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(48, 13);
         this.label3.TabIndex = 6;
         this.label3.Text = "File TAG";
         // 
         // textBox2
         // 
         this.textBox2.Location = new System.Drawing.Point(82, 92);
         this.textBox2.Name = "textBox2";
         this.textBox2.Size = new System.Drawing.Size(231, 20);
         this.textBox2.TabIndex = 5;
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(350, 232);
         this.Controls.Add(this.progressBar1);
         this.Controls.Add(this.button2);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.groupBox1);
         this.Name = "Form1";
         this.Text = "CPTECGluer (by Jauch)";
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.TextBox textBox2;
      private System.Windows.Forms.CheckBox checkBox1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.DateTimePicker dateTimePicker1;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Button button2;
      private System.Windows.Forms.ProgressBar progressBar1;
      private System.ComponentModel.BackgroundWorker backgroundWorker1;
   }
}

