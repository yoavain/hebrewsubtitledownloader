namespace HebrewSubtitleDownloader
{
    partial class LoginForm
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
            this.emailTextBox = new System.Windows.Forms.TextBox();
            this.emailLabel = new System.Windows.Forms.Label();
            this.passwordMaskedTextBox = new System.Windows.Forms.MaskedTextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.loginFormPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.showPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this.loginFormPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // emailTextBox
            // 
            this.emailTextBox.Location = new System.Drawing.Point(12, 66);
            this.emailTextBox.Name = "emailTextBox";
            this.emailTextBox.Size = new System.Drawing.Size(134, 20);
            this.emailTextBox.TabIndex = 0;
            this.emailTextBox.TextChanged += new System.EventHandler(this.emailTextBox1_TextChanged);
            // 
            // emailLabel
            // 
            this.emailLabel.AutoSize = true;
            this.emailLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.emailLabel.Location = new System.Drawing.Point(9, 45);
            this.emailLabel.Name = "emailLabel";
            this.emailLabel.Size = new System.Drawing.Size(45, 16);
            this.emailLabel.TabIndex = 1;
            this.emailLabel.Text = "Email:";
            // 
            // passwordMaskedTextBox
            // 
            this.passwordMaskedTextBox.Location = new System.Drawing.Point(12, 124);
            this.passwordMaskedTextBox.Name = "passwordMaskedTextBox";
            this.passwordMaskedTextBox.PasswordChar = '*';
            this.passwordMaskedTextBox.Size = new System.Drawing.Size(134, 20);
            this.passwordMaskedTextBox.TabIndex = 2;
            this.passwordMaskedTextBox.UseSystemPasswordChar = true;
            this.passwordMaskedTextBox.MaskInputRejected += new System.Windows.Forms.MaskInputRejectedEventHandler(this.maskedTextBox1_MaskInputRejected);
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.passwordLabel.Location = new System.Drawing.Point(9, 103);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(71, 16);
            this.passwordLabel.TabIndex = 3;
            this.passwordLabel.Text = "Password:";
            // 
            // saveButton
            // 
            this.saveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.saveButton.Location = new System.Drawing.Point(71, 188);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.titleLabel.Location = new System.Drawing.Point(9, 12);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(213, 16);
            this.titleLabel.TabIndex = 5;
            this.titleLabel.Text = "Please enter your login information";
            // 
            // loginFormPanel
            // 
            this.loginFormPanel.Controls.Add(this.showPasswordCheckBox);
            this.loginFormPanel.Controls.Add(this.cancelButton);
            this.loginFormPanel.Controls.Add(this.titleLabel);
            this.loginFormPanel.Controls.Add(this.saveButton);
            this.loginFormPanel.Controls.Add(this.emailLabel);
            this.loginFormPanel.Controls.Add(this.passwordMaskedTextBox);
            this.loginFormPanel.Controls.Add(this.passwordLabel);
            this.loginFormPanel.Controls.Add(this.emailTextBox);
            this.loginFormPanel.Location = new System.Drawing.Point(12, 12);
            this.loginFormPanel.Name = "loginFormPanel";
            this.loginFormPanel.Size = new System.Drawing.Size(320, 238);
            this.loginFormPanel.TabIndex = 6;
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.Location = new System.Drawing.Point(152, 188);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // showPasswordCheckBox
            // 
            this.showPasswordCheckBox.AutoSize = true;
            this.showPasswordCheckBox.Location = new System.Drawing.Point(152, 127);
            this.showPasswordCheckBox.Name = "showPasswordCheckBox";
            this.showPasswordCheckBox.Size = new System.Drawing.Size(101, 17);
            this.showPasswordCheckBox.TabIndex = 7;
            this.showPasswordCheckBox.Text = "Show password";
            this.showPasswordCheckBox.UseVisualStyleBackColor = true;
            this.showPasswordCheckBox.CheckedChanged += new System.EventHandler(this.showPasswordCheckBox_CheckedChanged);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 262);
            this.Controls.Add(this.loginFormPanel);
            this.Name = "LoginForm";
            this.Text = "LoginForm";
            this.loginFormPanel.ResumeLayout(false);
            this.loginFormPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox emailTextBox;
        private System.Windows.Forms.Label emailLabel;
        private System.Windows.Forms.MaskedTextBox passwordMaskedTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Panel loginFormPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox showPasswordCheckBox;
    }
}