namespace SratimUtils
{
    public class Settings
    {
        #region Singleton

        private Settings()
        {
            LoadSettings();
        }

        private static class SettingsHolder
        {
            public static readonly Settings Instance = new Settings();
        }

        public static Settings GetInstance()
        {
            return SettingsHolder.Instance;
        }

        #endregion Singleton

        #region Settings

        public string SratimEmail { get; set; }

        public string SratimPassword { get; set; }


        public void LoadSettings()
        {
            SratimEmail = SettingManager.GetParamAsString(SettingManager.SratimEmailParam, "");
            SratimPassword = SettingManager.GetEncriptedParamAsString(SettingManager.SratimPasswordParam, "");
        }

        public void SaveSettings()
        {
            SettingManager.SetParam(SettingManager.SratimEmailParam, SratimEmail);
            SettingManager.SetEncryptedParam(SettingManager.SratimPasswordParam, SratimPassword);
        }

        #endregion Settings
    }
}
