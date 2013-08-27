using System;
using System.Security.Cryptography;
using MediaPortal.Configuration;

namespace SratimUtils
{
    public static class SettingManager
    {
        // Settings filename
        public static readonly string SettingsFileName = "MediaPortal.xml";

        // Prefix
        private const string Prefix = "hebrewSubtitleManager";

        public static MediaPortal.Profile.Settings MediaPortalSettings
        {
            get
            {
                return new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, SettingsFileName));
            }
        }

        #region param names

        public static readonly string SratimEmailParam = "sratim_email";
        public static readonly string SratimPasswordParam = "sratim_password";

        #endregion

        #region public get methods

        /// <summary>
        /// Get param as string
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as string</returns>
        public static string GetParamAsString(string paramName, string defaultValue)
        {
            return MediaPortalSettings.GetValueAsString(Prefix, paramName, defaultValue);
        }

        /// <summary>
        /// Get encrypted param as string
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as string</returns>
        public static string GetEncriptedParamAsString(string paramName, string defaultValue)
        {
            var encriptedString = MediaPortalSettings.GetValueAsString(Prefix, paramName, defaultValue);

            // Decrypt
            string decryptedString = null;
            try
            {
                if (!string.IsNullOrEmpty(encriptedString))
                {
                    decryptedString = PasswordUtility.DecryptData(encriptedString, DataProtectionScope.LocalMachine);
                }
            }
            catch (Exception)
            {
                ;
            }

            return decryptedString;
        }

        /// <summary>
        /// Get param as bool
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as bool</returns>
        public static bool GetParamAsBool(string paramName, bool defaultValue)
        {
            return MediaPortalSettings.GetValueAsBool(Prefix, paramName, defaultValue);
        }

        /// <summary>
        /// Get param as int
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as int</returns>
        public static int GetParamAsInt(string paramName, int defaultValue)
        {
            return MediaPortalSettings.GetValueAsInt(Prefix, paramName, defaultValue);
        }

        #endregion public get methods

        #region public set methods

        /// <summary>
        /// Set param value
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="value">param value</param>
        public static void SetParam(string paramName, string value)
        {
            MediaPortalSettings.SetValue(Prefix, paramName, value);
        }

        /// <summary>
        /// Set encrypted param value
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="decryptedString">param value</param>
        public static void SetEncryptedParam(string paramName, string decryptedString)
        {
            // Encrypt
            var encyptedString = PasswordUtility.EncryptData(decryptedString, DataProtectionScope.LocalMachine);

            MediaPortalSettings.SetValue(Prefix, paramName, encyptedString);
        }

        /// <summary>
        /// Set bool param value to "True" or "False"
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="value">param value</param>
        public static void SetParamAsBool(string paramName, bool value)
        {
            MediaPortalSettings.SetValueAsBool(Prefix, paramName, value);
        }

        #endregion public get methods

    }
}
