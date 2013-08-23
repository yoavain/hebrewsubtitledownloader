using System;
using System.IO;
using System.Net;
using System.Xml;

namespace Sratim
{
    public sealed class SratimDownloaderConfiguration
    {
        private static volatile SratimDownloaderConfiguration _instance;
        private static readonly object SyncObject = new Object();

        private const string SettingsFilename = "SubtitleDownloaders\\Sratim.xml";

        // xml
        private const string LoginPath = "login";
        private const string EmailString = "email";
        private const string PasswordString = "password";
        private const string LoginString = "Login";

        // cookie
        private const string UserIdString = "slcoo_user_id";
        private const string UserPassString = "slcoo_user_pass";
        private const string UserNameString = "slcoo_user_name";

        // Cookie container
        private CookieContainer _sratimCookieContainer;


        private SratimDownloaderConfiguration()
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load(SettingsFilename);
                
                // Get the cookie
                var loginNode = xml.SelectNodes("/settings/" + LoginPath);
                if (loginNode != null)
                {
                    // Check for login data in settings/login
                    var loginData = ReadLoginDataFromConfigurationFile(loginNode);
                    if (loginData != null)
                    {
                        // Fetch for cookie
                        var cookieData = GetCookieDataFromLoginData(loginData);

                        if (cookieData != null)
                        {
                            return;
                        }

                        // No cookie & no login data
                        throw new InvalidDataException("Could not get cookie. Please check you user/password in configruation file");
                    }
                }
                else
                {
                    // No cookie & no login data
                    throw new InvalidDataException("Missing cookie data & missing login data");
                }
            }
            catch (FileNotFoundException)
            {
                // no file --> blank list
                throw new InvalidDataException("Missing configuration file");
            }
            catch (XmlException)
            {
                // invalid file --> blank list
                throw new InvalidDataException("Invalid configuration file");
            }
        }

        public static SratimDownloaderConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncObject)
                    {
                        if (_instance == null)
                            _instance = new SratimDownloaderConfiguration();
                    }
                }
                return _instance;
            }
        }

        public CookieContainer GetSratimCookieContainer()
        {
            return _sratimCookieContainer;
        }

        /// <summary>
        /// Reads LoginData from configuration file
        /// </summary>
        /// <param name="loginNodeList">xml node</param>
        /// <returns>Login data. (null if missing or fails)</returns>
        private static LoginData ReadLoginDataFromConfigurationFile(XmlNodeList loginNodeList)
        {
            string email = null;
            string password = null;
            string login = null;

            foreach (XmlElement element in loginNodeList)
            {
                var emailNode = element.SelectSingleNode(EmailString);
                if (emailNode == null || string.IsNullOrEmpty(emailNode.InnerText))
                {
                    return null;
                }
                email = emailNode.InnerText;

                var passwordNode = element.SelectSingleNode(PasswordString);
                if (passwordNode == null || string.IsNullOrEmpty(passwordNode.InnerText))
                {
                    return null;
                }
                password = passwordNode.InnerText;

                var loginNode = element.SelectSingleNode(LoginString);
                if (loginNode == null || string.IsNullOrEmpty(loginNode.InnerText))
                {
                    return null;
                }
                login = loginNode.InnerText;
            }
            return new LoginData(email, password, login);
        }

        /// <summary>
        /// Get cookie data
        /// </summary>
        /// <param name="loginData">the login data (user/password)</param>
        /// <returns>cookie data</returns>
        private CookieData GetCookieDataFromLoginData(LoginData loginData)
        {
            const string url = SratimDownloader.BaseUrl + SratimDownloader.LoginPath;
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

            public static CookieData CreateFromCookieContainer(CookieContainer cookieContainer)
            {
                string slcooUserId = null;
                string slcooUserPass = null;
                string slcooUserName = null;

                // Parse cookie container
                var cookieCollection = cookieContainer.GetCookies(new Uri(SratimDownloader.BaseUrl));
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
    }
}