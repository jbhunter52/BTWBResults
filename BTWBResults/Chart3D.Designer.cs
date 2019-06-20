namespace BTWBResults
{
    partial class Chart3D
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
            this.myRenderer3D = new nzy3D.Plot3D.Rendering.View.Renderer3D();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textBoxCalMin = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxCalHr = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxRestTime = new System.Windows.Forms.TextBox();
            this.textBoxRoundTime = new System.Windows.Forms.TextBox();
            this.textBoxRounds = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // myRenderer3D
            // 
            this.myRenderer3D.BackColor = System.Drawing.Color.Black;
            this.myRenderer3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myRenderer3D.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.myRenderer3D.Location = new System.Drawing.Point(0, 0);
            this.myRenderer3D.Name = "myRenderer3D";
            this.myRenderer3D.Size = new System.Drawing.Size(523, 460);
            this.myRenderer3D.TabIndex = 0;
            this.myRenderer3D.VSync = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.myRenderer3D);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBoxCalMin);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxCalHr);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxRestTime);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxRoundTime);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxRounds);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(720, 460);
            this.splitContainer1.SplitterDistance = 523;
            this.splitContainer1.TabIndex = 1;
            // 
            // textBoxCalMin
            // 
            this.textBoxCalMin.Location = new System.Drawing.Point(115, 184);
            this.textBoxCalMin.Name = "textBoxCalMin";
            this.textBoxCalMin.Size = new System.Drawing.Size(52, 20);
            this.textBoxCalMin.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(66, 187);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Cal/min";
            // 
            // textBoxCalHr
            // 
            this.textBoxCalHr.Location = new System.Drawing.Point(115, 158);
            this.textBoxCalHr.Name = "textBoxCalHr";
            this.textBoxCalHr.Size = new System.Drawing.Size(52, 20);
            this.textBoxCalHr.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(73, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Cal/hr";
            // 
            // textBoxRestTime
            // 
            this.textBoxRestTime.Location = new System.Drawing.Point(115, 96);
            this.textBoxRestTime.Name = "textBoxRestTime";
            this.textBoxRestTime.Size = new System.Drawing.Size(52, 20);
            this.textBoxRestTime.TabIndex = 5;
            this.textBoxRestTime.Leave += new System.EventHandler(this.textBoxRestTime_Leave);
            // 
            // textBoxRoundTime
            // 
            this.textBoxRoundTime.Location = new System.Drawing.Point(115, 70);
            this.textBoxRoundTime.Name = "textBoxRoundTime";
            this.textBoxRoundTime.Size = new System.Drawing.Size(52, 20);
            this.textBoxRoundTime.TabIndex = 4;
            this.textBoxRoundTime.Leave += new System.EventHandler(this.textBoxRoundTime_Leave);
            // 
            // textBoxRounds
            // 
            this.textBoxRounds.Location = new System.Drawing.Point(115, 44);
            this.textBoxRounds.Name = "textBoxRounds";
            this.textBoxRounds.Size = new System.Drawing.Size(52, 20);
            this.textBoxRounds.TabIndex = 3;
            this.textBoxRounds.Leave += new System.EventHandler(this.textBoxRounds_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Rest Time (secs)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Round Time (secs)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(65, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rounds";
            // 
            // Chart3D
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 460);
            this.Controls.Add(this.splitContainer1);
            this.MinimizeBox = false;
            this.Name = "Chart3D";
            this.Text = "Chart3D";
            this.Load += new System.EventHandler(this.Chart3D_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private nzy3D.Plot3D.Rendering.View.Renderer3D myRenderer3D;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBoxCalMin;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxCalHr;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxRestTime;
        private System.Windows.Forms.TextBox textBoxRoundTime;
        private System.Windows.Forms.TextBox textBoxRounds;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;

    }
}