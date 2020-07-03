using System.Drawing;
using System.Windows.Forms;

namespace Examples.HelloForms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private ToolTip toolTip1;
        private RadioButton rbEnUs;
        private RadioButton rbFrFr;
        private RadioButton rbRuRu;
        private GroupBox gbSwitch;
        private GroupBox gbForms;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private TextBox textBox1;

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
            this.components = new System.ComponentModel.Container();
            this.gbSwitch = new System.Windows.Forms.GroupBox();
            this.rbEnUs = new System.Windows.Forms.RadioButton();
            this.rbFrFr = new System.Windows.Forms.RadioButton();
            this.rbRuRu = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gbForms = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.gbSwitch.SuspendLayout();
            this.gbForms.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbSwitch
            // 
            this.gbSwitch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSwitch.Controls.Add(this.rbEnUs);
            this.gbSwitch.Controls.Add(this.rbFrFr);
            this.gbSwitch.Controls.Add(this.rbRuRu);
            this.gbSwitch.Location = new System.Drawing.Point(5, 5);
            this.gbSwitch.Name = "gbSwitch";
            this.gbSwitch.Size = new System.Drawing.Size(425, 35);
            this.gbSwitch.TabIndex = 0;
            this.gbSwitch.TabStop = false;
            // 
            // rbEnUs
            // 
            this.rbEnUs.AutoSize = true;
            this.rbEnUs.Location = new System.Drawing.Point(15, 5);
            this.rbEnUs.Name = "rbEnUs";
            this.rbEnUs.Size = new System.Drawing.Size(69, 21);
            this.rbEnUs.TabIndex = 0;
            this.rbEnUs.Text = "en-US";
            this.toolTip1.SetToolTip(this.rbEnUs, "Switch to English");
            this.rbEnUs.Click += new System.EventHandler(this.OnLocaleChanged);
            // 
            // rbFrFr
            // 
            this.rbFrFr.AutoSize = true;
            this.rbFrFr.Location = new System.Drawing.Point(150, 5);
            this.rbFrFr.Name = "rbFrFr";
            this.rbFrFr.Size = new System.Drawing.Size(61, 21);
            this.rbFrFr.TabIndex = 1;
            this.rbFrFr.Text = "fr-FR";
            this.toolTip1.SetToolTip(this.rbFrFr, "Switch to French");
            this.rbFrFr.Click += new System.EventHandler(this.OnLocaleChanged);
            // 
            // rbRuRu
            // 
            this.rbRuRu.AutoSize = true;
            this.rbRuRu.Location = new System.Drawing.Point(280, 5);
            this.rbRuRu.Name = "rbRuRu";
            this.rbRuRu.Size = new System.Drawing.Size(67, 21);
            this.rbRuRu.TabIndex = 2;
            this.rbRuRu.Text = "ru-RU";
            this.toolTip1.SetToolTip(this.rbRuRu, "Switch to Russian");
            this.rbRuRu.Click += new System.EventHandler(this.OnLocaleChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Hello, world!";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 17);
            this.label2.TabIndex = 2;
            // 
            // gbForms
            // 
            this.gbForms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbForms.Controls.Add(this.label3);
            this.gbForms.Controls.Add(this.label4);
            this.gbForms.Controls.Add(this.label5);
            this.gbForms.Location = new System.Drawing.Point(10, 105);
            this.gbForms.Name = "gbForms";
            this.gbForms.Size = new System.Drawing.Size(420, 70);
            this.gbForms.TabIndex = 3;
            this.gbForms.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 17);
            this.label3.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 17);
            this.label4.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 17);
            this.label5.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 180);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 17);
            this.label6.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 200);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 17);
            this.label7.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 220);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 17);
            this.label8.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(10, 250);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(420, 80);
            this.textBox1.TabIndex = 7;
            this.textBox1.Text = "Here is an example of how one might continue a very long string\nfor the common ca" +
    "se the string represents multi-line output.\n";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 353);
            this.Controls.Add(this.gbSwitch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gbForms);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Hello, world!";
            this.gbSwitch.ResumeLayout(false);
            this.gbSwitch.PerformLayout();
            this.gbForms.ResumeLayout(false);
            this.gbForms.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}