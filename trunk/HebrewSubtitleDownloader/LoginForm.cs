using System;
using System.Windows.Forms;
using SratimUtils;

namespace HebrewSubtitleDownloader
{
    public partial class LoginForm : Form
    {

        private readonly SetupForm _setupForm;

        public LoginForm(SetupForm setupForm)
        {
            _setupForm = setupForm;
            InitializeComponent();
        }

        private void emailTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Save
            var validated = SratimDownloaderConfiguration.GetInstance().ValidateCredentials(emailTextBox.Text, passwordMaskedTextBox.Text);
            _setupForm.UpdateStatus(validated);

            // Close
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // Close
            Close();
        }

        private void showPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            passwordMaskedTextBox.PasswordChar = showPasswordCheckBox.Checked ? '\0' : '*';
            passwordMaskedTextBox.UseSystemPasswordChar = !showPasswordCheckBox.Checked;
            Refresh();
        }
    }
}
