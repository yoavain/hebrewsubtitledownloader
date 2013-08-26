namespace HebrewSubtitleDownloader
{
    partial class SetupForm
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
            this.updateLoginInfo = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // updateLoginInfo
            // 
            this.updateLoginInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.updateLoginInfo.Location = new System.Drawing.Point(83, 76);
            this.updateLoginInfo.Name = "updateLoginInfo";
            this.updateLoginInfo.Size = new System.Drawing.Size(143, 38);
            this.updateLoginInfo.TabIndex = 0;
            this.updateLoginInfo.Text = "Update Login info";
            this.updateLoginInfo.UseVisualStyleBackColor = true;
            this.updateLoginInfo.Click += new System.EventHandler(this.updateLoginInfo_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.status.Location = new System.Drawing.Point(80, 19);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(157, 17);
            this.status.TabIndex = 1;
            this.status.Text = "Sratim.co.il login status:";
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.exitButton.Location = new System.Drawing.Point(83, 152);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(143, 38);
            this.exitButton.TabIndex = 2;
            this.exitButton.Text = "Done";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.statusLabel);
            this.panel1.Controls.Add(this.status);
            this.panel1.Controls.Add(this.exitButton);
            this.panel1.Controls.Add(this.updateLoginInfo);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(319, 237);
            this.panel1.TabIndex = 3;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.statusLabel.Location = new System.Drawing.Point(123, 47);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            this.statusLabel.TabIndex = 3;
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 262);
            this.Controls.Add(this.panel1);
            this.Name = "SetupForm";
            this.Text = "SetupForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button updateLoginInfo;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label statusLabel;
    }
}