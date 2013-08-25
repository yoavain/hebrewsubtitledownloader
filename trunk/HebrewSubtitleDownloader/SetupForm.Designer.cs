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
            this.SuspendLayout();
            // 
            // updateLoginInfo
            // 
            this.updateLoginInfo.Location = new System.Drawing.Point(114, 74);
            this.updateLoginInfo.Name = "updateLoginInfo";
            this.updateLoginInfo.Size = new System.Drawing.Size(111, 38);
            this.updateLoginInfo.TabIndex = 0;
            this.updateLoginInfo.Text = "Update Login info";
            this.updateLoginInfo.UseVisualStyleBackColor = true;
            this.updateLoginInfo.Click += new System.EventHandler(this.updateLoginInfo_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(111, 9);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(117, 13);
            this.status.TabIndex = 1;
            this.status.Text = "Sratim.co.il login status:";
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(114, 163);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(111, 38);
            this.exitButton.TabIndex = 2;
            this.exitButton.Text = "Done";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 262);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.status);
            this.Controls.Add(this.updateLoginInfo);
            this.Name = "SetupForm";
            this.Text = "SetupForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button updateLoginInfo;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Button exitButton;
    }
}