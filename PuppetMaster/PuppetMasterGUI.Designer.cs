namespace PuppetMaster
{
    partial class PuppetMasterGUI
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
            this.scriptBtn = new System.Windows.Forms.Button();
            this.cmdBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.submitBtn = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.exeNextBtn = new System.Windows.Forms.Button();
            this.exeAllBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Load Script:";
            // 
            // scriptBtn
            // 
            this.scriptBtn.Location = new System.Drawing.Point(82, 57);
            this.scriptBtn.Name = "scriptBtn";
            this.scriptBtn.Size = new System.Drawing.Size(75, 23);
            this.scriptBtn.TabIndex = 1;
            this.scriptBtn.Text = "Browse...";
            this.scriptBtn.UseVisualStyleBackColor = true;
            this.scriptBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // cmdBox
            // 
            this.cmdBox.Location = new System.Drawing.Point(101, 6);
            this.cmdBox.Name = "cmdBox";
            this.cmdBox.Size = new System.Drawing.Size(200, 20);
            this.cmdBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Insert command:";
            // 
            // submitBtn
            // 
            this.submitBtn.Location = new System.Drawing.Point(307, 6);
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.Size = new System.Drawing.Size(50, 20);
            this.submitBtn.TabIndex = 4;
            this.submitBtn.Text = "Submit";
            this.submitBtn.UseVisualStyleBackColor = true;
            this.submitBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.submitBtn_MouseClick);
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(13, 83);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(416, 225);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // exeNextBtn
            // 
            this.exeNextBtn.Location = new System.Drawing.Point(101, 314);
            this.exeNextBtn.Name = "exeNextBtn";
            this.exeNextBtn.Size = new System.Drawing.Size(99, 23);
            this.exeNextBtn.TabIndex = 6;
            this.exeNextBtn.Text = "Exec Selected";
            this.exeNextBtn.UseVisualStyleBackColor = true;
            this.exeNextBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.exeNextBtn_MouseClick);
            // 
            // exeAllBtn
            // 
            this.exeAllBtn.Location = new System.Drawing.Point(237, 314);
            this.exeAllBtn.Name = "exeAllBtn";
            this.exeAllBtn.Size = new System.Drawing.Size(64, 23);
            this.exeAllBtn.TabIndex = 7;
            this.exeAllBtn.Text = "Exec All";
            this.exeAllBtn.UseVisualStyleBackColor = true;
            this.exeAllBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.exeAllBtn_MouseClick);
            // 
            // PuppetMasterGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 349);
            this.Controls.Add(this.exeAllBtn);
            this.Controls.Add(this.exeNextBtn);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.submitBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmdBox);
            this.Controls.Add(this.scriptBtn);
            this.Controls.Add(this.label1);
            this.Name = "PuppetMasterGUI";
            this.Text = "PuppetMasterGUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button scriptBtn;
        private System.Windows.Forms.TextBox cmdBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button submitBtn;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button exeNextBtn;
        private System.Windows.Forms.Button exeAllBtn;
    }
}

