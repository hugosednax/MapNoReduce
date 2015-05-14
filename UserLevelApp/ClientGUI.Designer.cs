namespace UserLevelApp
{
    partial class ClientGUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.entryIP = new System.Windows.Forms.TextBox();
            this.entryPort = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.startJobBt = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.mapDllBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.numSlicesBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ouputBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Entry:";
            // 
            // entryIP
            // 
            this.entryIP.Location = new System.Drawing.Point(54, 13);
            this.entryIP.Name = "entryIP";
            this.entryIP.Size = new System.Drawing.Size(136, 20);
            this.entryIP.TabIndex = 1;
            this.entryIP.Text = "localhost";
            // 
            // entryPort
            // 
            this.entryPort.Location = new System.Drawing.Point(196, 12);
            this.entryPort.Name = "entryPort";
            this.entryPort.Size = new System.Drawing.Size(65, 20);
            this.entryPort.TabIndex = 2;
            this.entryPort.Text = "30001";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(54, 40);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.button1_MouseClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Input:";
            // 
            // startJobBt
            // 
            this.startJobBt.Location = new System.Drawing.Point(54, 155);
            this.startJobBt.Name = "startJobBt";
            this.startJobBt.Size = new System.Drawing.Size(75, 23);
            this.startJobBt.TabIndex = 5;
            this.startJobBt.Text = "Start Job";
            this.startJobBt.UseVisualStyleBackColor = true;
            this.startJobBt.MouseClick += new System.Windows.Forms.MouseEventHandler(this.startJobBt_MouseClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Map Dll:";
            // 
            // mapDllBtn
            // 
            this.mapDllBtn.Location = new System.Drawing.Point(54, 63);
            this.mapDllBtn.Name = "mapDllBtn";
            this.mapDllBtn.Size = new System.Drawing.Size(75, 23);
            this.mapDllBtn.TabIndex = 7;
            this.mapDllBtn.Text = "browse...";
            this.mapDllBtn.UseVisualStyleBackColor = true;
            this.mapDllBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mapDllBtn_MouseClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Number of Slices:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // numSlicesBox
            // 
            this.numSlicesBox.Location = new System.Drawing.Point(109, 115);
            this.numSlicesBox.Name = "numSlicesBox";
            this.numSlicesBox.Size = new System.Drawing.Size(47, 20);
            this.numSlicesBox.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Output Directory:";
            // 
            // ouputBtn
            // 
            this.ouputBtn.Location = new System.Drawing.Point(106, 89);
            this.ouputBtn.Name = "ouputBtn";
            this.ouputBtn.Size = new System.Drawing.Size(75, 23);
            this.ouputBtn.TabIndex = 11;
            this.ouputBtn.Text = "browse...";
            this.ouputBtn.UseVisualStyleBackColor = true;
            this.ouputBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ouputBtn_MouseClick);
            // 
            // ClientGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.ouputBtn);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numSlicesBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.mapDllBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.startJobBt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.entryPort);
            this.Controls.Add(this.entryIP);
            this.Controls.Add(this.label1);
            this.Name = "ClientGUI";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox entryIP;
        private System.Windows.Forms.TextBox entryPort;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button startJobBt;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button mapDllBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox numSlicesBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button ouputBtn;
    }
}

