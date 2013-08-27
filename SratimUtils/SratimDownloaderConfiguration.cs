using System;
using System.IO;
using System.Net;

namespace SratimUtils
{
    public sealed class SratimDownloaderConfiguration
    {
        #region Singleton

        private SratimDownloaderConfiguration()
        {
        }

        private static class SratimDownloaderConfigurationHolder
        {
            public static readonly SratimDownloaderConfiguration Instance = new SratimDownloaderConfiguration();
        }

        public static SratimDownloaderConfiguration GetInstance()
        {
            return SratimDownloaderConfigurationHolder.Instance;
        }

        #endregion Singleton

        #region const

        // Sratim url
        public const string SratimBaseUrl = "http://www.subtitle.co.il/";
        public const string SraitmLoginPath = "login.php";

        // login
        private const string EmailString = "email";
        private const string PasswordString = "password";
        private const string LoginString = "Login";

        // cookie
        private const string UserIdString = "slcoo_user_id";
        private const string UserPassString = "slcoo_user_pass";
        private const string UserNameString = "slcoo_user_name";

        private const string LoginValue = "%D7%94%D7%AA%D7%97%D7%91%D7%A8";

        #endregion const

        #region private
        // Cookie container
        private CookieContainer _sratimCookieContainer;

        #endregion private

        #region public methods

        public Boolean ValidateCredentials(string email, string password)
        {
            // Already have cookie
            if (_sratimCookieContainer != null)
            {
                return true;
            }
            
            // Prepare login data
            var loginData = new LoginData(email, password, LoginValue);

            // Fetch for cookie
            var cookieData = GetCookieDataFromLoginData(loginData);

            return cookieData != null;
        }

        public CookieContainer GetSratimCookieContainer()
        {
            return _sratimCookieContainer;
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Get cookie data
        /// </summary>
        /// <param name="loginData">the login data (user/password)</param>
        /// <returns>cookie data</returns>
        private CookieData GetCookieDataFromLoginData(LoginData loginData)
        {
            const string url = SratimBaseUrl + SraitmLoginPath;
            var postData = loginData.ToString();
            var cookieContainer = new CookieContainer();

            // ReSharper disable once UnusedVariable
            var response = DoWebRequest(url, postData, cookieContainer);

            if (cookieContainer.Count > 0)
            {
                var cookieData = CookieData.CreateFromCookieContainer(cookieContainer);
                if (cookieData != null)
                {
                    // Save _sratimCookieContainer object
                    _sratimCookieContainer = cookieContainer;

                    // Return the cookieData
                    return cookieData;
                }
            }
            return null;
        }

        /// <summary>
        /// Do web request
        /// </summary>
        /// <param name="url">Post request</param>
        /// <param name="postData">data to post</param>
        /// <param name="cookieContainer">the cookie container to hold cookies</param>
        /// <returns>response data</returns>
        private static string DoWebRequest(string url, string postData, CookieContainer cookieContainer)
        {
            string responseData = null;
            var webRequest = WebRequest.Create(url) as HttpWebRequest;
            if (webRequest != null)
            {
                // Set request details.
                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.2; rv:7.0) Gecko/20120909 Firefox/7.0";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.CookieContainer = cookieContainer;

                // For getting through any proxy
                webRequest.Proxy = WebRequest.DefaultWebProxy;
                webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;

                // Write the post data into the request's stream.
                var requestWriter = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    requestWriter.Write(postData);
                }
                finally
                {
                    requestWriter.Close();
                }

                // Process the data.
                StreamReader responseReader = null;
                Stream responseStream = null;

                try
                {
                    responseStream = webRequest.GetResponse().GetResponseStream();
                    if (responseStream != null)
                    {
                        responseReader = new StreamReader(responseStream);
                        responseData = responseReader.ReadToEnd();
                    }
                }
                finally
                {
                    if (responseStream != null) responseStream.Close();
                    if (responseReader != null) responseReader.Close();
                }
            }

            return responseData;
        }

        #endregion private methods

        #region inner classes

        /// <summary>
        /// Login data
        /// 1. email
        /// 2. password
        /// 3. Login
        /// </summary>
        private class LoginData
        {
            private readonly string _email;
            private readonly string _password;
            private readonly string _login;

            public LoginData(string email, string password, string login)
            {
                _email = email;
                _password = password;
                _login = login;
            }

            public override string ToString()
            {
                return string.Format("{0}={1}&{2}={3}&{4}={5}",
                    EmailString, _email.Replace("@", "%40"),
                    PasswordString, _password,
                    LoginString, _login);
            }
        }

        /// <summary>
        /// Cookie data object, holds:
        /// 1. slcoo_user_id
        /// 2. slcoo_user_pass
        /// 3. slcoo_user_name
        /// </summary>
        private class CookieData
        {
            private readonly string _slcooUserId;
            private readonly string _slcooUserPass;
            private readonly string _slcooUserName;

            private CookieData(string slcooUserID, string slcooUserPass, string slcooUserName)
            {
                _slcooUserId = slcooUserID;
                _slcooUserPass = slcooUserPass;
                _slcooUserName = slcooUserName;
            }

            public override string ToString()
            {
                return string.Format("{0}={1}; {2}={3}; {4}={5};",
                    UserIdString, _slcooUserId,
                    UserPassString, _slcooUserPass,
                    UserNameString, _slcooUserName);
            }

            public static CookieData CreateFromCookieContainer (CookieContainer cookieContainer)
            {
                string slcooUserId = null;
                string slcooUserPass = null;
                string slcooUserName = null;

                // Parse cookie container
                var cookieCollection = cookieContainer.GetCookies(new Uri(SratimBaseUrl));
                foreach (Cookie cookie in cookieCollection)
                {
                    if (UserIdString.Equals(cookie.Name))
                    {
                        slcooUserId = cookie.Value;
                    }
                    if (UserPassString.Equals(cookie.Name))
                    {
                        slcooUserPass = cookie.Value;
                    }
                    if (UserNameString.Equals(cookie.Name))
                    {
                        slcooUserName = cookie.Value;
                    }
                }

                // Check all fields
                if (!string.IsNullOrEmpty(slcooUserId) &&
                    !string.IsNullOrEmpty(slcooUserPass) &&
                    !string.IsNullOrEmpty(slcooUserName))
                {
                    return new CookieData(slcooUserId, slcooUserPass, slcooUserName);
                }

                return null;
            }
        }

        #endregion inner classes
    }
}