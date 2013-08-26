using System;
using System.Windows.Forms;

namespace HebrewSubtitleDownloader
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
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
            // todo

            // Close
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // Close
            Close();
        }
    }
}
