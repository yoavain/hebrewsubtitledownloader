using System;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using System.Windows.Forms;

namespace HebrewSubtitleDownloader
{
    [PluginIcons("HebrewSubtitleDownloader.Resources.HebrewSubtitleDownloader_icon_enabled.png", "HebrewSubtitleDownloader.Resources.HebrewSubtitleDownloader_icon_disabled.png")]
    public class HebrewSubtitleDownloader : GUIWindow, ISetupForm
    {

        #region ISetupForm Members

        public string PluginName()
        {
            return "Hebrew Subtitle Downloader";
        }

        public string Description()
        {
            return "Plugin that adds Hebrew subtitle providers for SubCentral plugin";
        }

        public string Author()
        {
            return "yoavain";
        }

        public void ShowPlugin()
        {
            try
            {
                var setupForm = new SetupForm();
                // Define the border style of the form to a dialog box.
                setupForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                // Set the MaximizeBox to false to remove the maximize box.
                setupForm.MaximizeBox = false;
                // Set the MinimizeBox to false to remove the minimize box.
                setupForm.MinimizeBox = false;
                // Set the start position of the form to the center of the screen.
                setupForm.StartPosition = FormStartPosition.CenterScreen; 
                // Display the form as a modal dialog box.
                setupForm.ShowDialog();
            }
            catch (Exception)
            {
            	MessageBox.Show("Cannot start plugin");
            }
        }

        public bool CanEnable()
        {
            return false;
        }

        public int GetWindowId()
        {
            return 34171;
        }

        public bool DefaultEnabled()
        {
            return true;
        }

        public bool HasSetup()
        {
            return true;
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, 
            out string strPictureImage)
        {
            strButtonText = String.Empty;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return false;
        }

        public override int GetID
        {
            get { return 34171; }
            set { }
        }

        #endregion
    }
}