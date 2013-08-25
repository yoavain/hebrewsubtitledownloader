using System;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace HebrewSubtitleDownloader
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
        }

        private void updateLoginInfo_Click(object sender, EventArgs e)
        {
            var loginForm = new LoginForm();
            loginForm.Show();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
