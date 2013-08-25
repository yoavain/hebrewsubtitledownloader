using System;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace HebrewSubtitleDownloader
{
    [PluginIcons("HebrewSubtitleDownloader.Resources.HebrewSubtitleDownloader_icon_enabled.png", "HebrewSubtitleDownloader.Resources.HebrewSubtitleDownloader_icon_disabled.png")]
    public class HebrewSubtitleDownloader : GUIWindow, ISetupForm
    {
        public HebrewSubtitleDownloader ()
        {
            
        }

        #region ISetupForm Members

        public string PluginName()
        {
            return "Hebrew Subtitle Downloader";
        }

        public string Description()
        {
            return "Plugin to add Hebrew subtitle providers for SubCentral plugin";
        }

        public string Author()
        {
            return "yoavain";
        }

        public void ShowPlugin()
        {
            var setupForm = new SetupForm();
            setupForm.Show();
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