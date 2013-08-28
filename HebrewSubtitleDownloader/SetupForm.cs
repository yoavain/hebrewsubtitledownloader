﻿using System;
using System.Drawing;
using System.Windows.Forms;
using SratimUtils;

namespace HebrewSubtitleDownloader
{
    public partial class SetupForm : Form
    {
        private LoginForm _loginForm = null;

        public SetupForm()
        {
            InitializeComponent();

            var sratimEmail = Settings.GetInstance().SratimEmail;
            var sratimPassword = Settings.GetInstance().SratimPassword;
            var validated = SratimDownloaderConfiguration.GetInstance().ValidateCredentials(sratimEmail, sratimPassword);
            UpdateStatus(validated);
        }

        public void UpdateStatus(bool validate)
        {
            if (validate)
            {
                statusLabel.Text = "User account validated";
                statusLabel.ForeColor = Color.Green;
                updateLoginInfo.Enabled = false;
                SaveCredentials();
            }
            else
            {
                statusLabel.Text = "Could not login using credentials";
                statusLabel.ForeColor = Color.Red;
                updateLoginInfo.Enabled = true;
            }
            
            Refresh();
        }

        private void SaveCredentials()
        {
            if (_loginForm != null)
            {
                Settings.GetInstance().SaveSettings(_loginForm.GetEmail(), _loginForm.GetPassword());
            }
        }

        private void updateLoginInfo_Click(object sender, EventArgs e)
        {
            _loginForm = new LoginForm(this);
            // Define the border style of the form to a dialog box.
            _loginForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            _loginForm.MaximizeBox = false;
            // Set the MinimizeBox to false to remove the minimize box.
            _loginForm.MinimizeBox = false;
            // Set the start position of the form to the center of the screen.
            _loginForm.StartPosition = FormStartPosition.CenterScreen;
            // Display the form as a modal dialog box.
            _loginForm.ShowDialog();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {

        }

        private void setupFormPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
