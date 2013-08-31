using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SratimUtils;
using SubtitleDownloader.Core;
using SubtitleDownloader.Util;

namespace Sratim
{
    /// <summary>
    /// This class is an implementation of SubtitleDownloader API (http://www.assembla.com/spaces/subtitledownloader/wiki)
    /// For the site Sratim (http://www.sratim.co.il)
    /// Code is inspired by the XBMC implementaion by orivar (https://github.com/orivar)
    /// Original Python code can be found here: https://github.com/amet/script.xbmc.subtitles/tree/master/script.xbmc.subtitles/resources/lib/services/Sratim
    /// </summary>
    public class SratimDownloader : ISubtitleDownloader
    {
        // private const string BaseUrl = "http://www.sratim.co.il/"; old URL
        public const string BaseUrl = "http://www.subtitle.co.il/";

        public const string LoginPath = "login.php";
        // ===============================================================================
        // Regular expression patterns
        // ===============================================================================

        private const string TvSearchResultsPattern = @"<a href=""viewseries.php\?id=(\d+)";

        private const string SearchResultsPattern =
            @"<a href=\""view.php\?id=(?<movie_id>\d+)&amp;q=(?<movie_query>[^\""""]*)\"" itemprop=\""url\"">(?<movie_hebrew>[^(]*)</a></div><div style=\""direction:ltr;\"" class=\""smtext\"">(?<movie_english>[^(]*)</div><span class=""smtext"">(?<movie_year>\d+)</span>";

        private const string SubtitleListPattern =
            @"downloadsubtitle.php\?id=(?<fid>\d*).*?subt_lang.*?title=\""(?<language>.*?)\"".*?subtitle_title.*?title=\""(?<title>.*?)\""";

        private const string TvSeasonPattern = @"seasonlink_(?<season_link>\d+).*?>(?<season_num>\d+)</a>";
        private const string TvEpisodePattern = @"episodelink_(?<episode_link>\d+).*?>(?<episode_num>\d+)</a>";


        // Maximum number of movie pages to look at (when exact match is not found)
        private const int MaxMoviePagesToFollow = 5;

        private static CookieContainer _sratimCookieContainer;

        public string GetName()
        {
            return "Sratim.co.il";
        }

        public List<Subtitle> SearchSubtitles(SearchQuery query)
        {
            var languageList = ConvertThreeLetterToTwoLetterLanguageCodes(query.LanguageCodes);

            var subtitles = SearchSubtitles(query.Query, query.Year, null, null, null, languageList, false);
            return subtitles.Select(subtitle => new Subtitle(subtitle.SubtitleId, query.Query, subtitle.Filename, Languages.GetLanguageCode(subtitle.LanguageName))).ToList();
        }

        public List<Subtitle> SearchSubtitles(EpisodeSearchQuery query)
        {
            var languageList = ConvertThreeLetterToTwoLetterLanguageCodes(query.LanguageCodes);

            var subtitles = SearchSubtitles(null, 0, query.SerieTitle,
                query.Season.ToString(CultureInfo.InvariantCulture),
                query.Episode.ToString(CultureInfo.InvariantCulture), languageList, false);
            return subtitles.Select(subtitle => new Subtitle(subtitle.SubtitleId, query.SerieTitle, subtitle.Filename, Languages.GetLanguageCode(subtitle.LanguageName))).ToList();
        }

        public List<Subtitle> SearchSubtitles(ImdbSearchQuery query)
        {
            var languageList = ConvertThreeLetterToTwoLetterLanguageCodes(query.LanguageCodes);

            var subtitles = SearchSubtitles(query.ImdbId, null, null, null, null, languageList, true);
            return subtitles.Select(subtitle => new Subtitle(subtitle.SubtitleId, query.ImdbId, subtitle.Filename, Languages.GetLanguageCode(subtitle.LanguageName))).ToList();
        }

        public List<FileInfo> SaveSubtitle(Subtitle subtitle)
        {
            var fileName = subtitle.FileName;
            var subtitleId = subtitle.Id;
            var languageName = subtitle.LanguageCode;
            string languageCode;
            ToOpenSubtitlesTwoLetters.TryGetValue(languageName, out languageCode);

            var subtitleData = new SubtitleData(fileName, subtitleId, languageName);
            var subtitles = new List<SubtitleData>(1) {subtitleData};

            var savedSubtitle = DownloadSubtitles(subtitles, 0);

            return savedSubtitle;
        }

        public int SearchTimeout { get; set; }


        // ===============================================================================
        // Private utility functions
        // ===============================================================================

        public SratimDownloader(int searchTimeout)
        {
            SearchTimeout = searchTimeout;
            Init();
        }

        public SratimDownloader()
        {
            Init();
        }

        private static void Init()
        {
            Settings.GetInstance().LoadSettings();
            SratimDownloaderConfiguration.GetInstance().ValidateCredentials(Settings.GetInstance().SratimEmail, Settings.GetInstance().SratimPassword);
            _sratimCookieContainer = SratimDownloaderConfiguration.GetInstance().GetSratimCookieContainer();
        }

        /// <summary>
        /// Returns the content of the given URL
        /// </summary>
        /// <param name="url">the url</param>
        /// <returns>the content of the page</returns>
        private static string GetUrl(string url)
        {
            if (_sratimCookieContainer == null)
            {
                throw new Exception("Could not login to site. Please install HebrewSubtitleDownloader extension to config your email and password");
            }

            try
            {
                var client = new WebClientEx(_sratimCookieContainer) { Encoding = Encoding.UTF8 };
                return client.DownloadString(url);
            }
            catch (Exception)
            {
                throw new Exception("Could not retrieve URL: " + url);
            }
        }

        /// <summary>
        /// Downloads a file of the given URL to the given download filename
        /// </summary>
        /// <param name="url">the url</param>
        /// <param name="downloadFile">filename to download</param>
        private static void DownloadFile(string url, string downloadFile)
        {
            var sratimCookieContainer = SratimDownloaderConfiguration.GetInstance().GetSratimCookieContainer();
            if (sratimCookieContainer == null)
            {
                return; // todo - yoav - message
            }

            try
            {
                var client = new WebClientEx(sratimCookieContainer) { Encoding = Encoding.UTF8 };
                client.DownloadFile(url, downloadFile);
            }
            catch (Exception)
            {
                ;
            }
        }

        /// <summary>
        /// WebClient with cookies
        /// </summary>
        public class WebClientEx : WebClient
        {
            private readonly CookieContainer _cookieContainer;

            public WebClientEx(CookieContainer cookieContainer)
            {
                _cookieContainer = cookieContainer;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = _cookieContainer;
                }
                return request;
            }
        }

        /// <summary>
        /// The function receives a subtitles page id number, a list of user selected
        /// languages and the current subtitles list and adds all found subtitles matching
        /// the language selection to the subtitles list.
        /// </summary>
        /// <param name="subtitlePageId">subtitle page id</param>
        /// <param name="languageList">list of languages</param>
        /// <returns></returns>
        private static IEnumerable<SubtitleData> GetAllSubtitles(string subtitlePageId, ICollection<string> languageList)
        {
            // return object
            var subtitlesList = new List<SubtitleData>();

            // Retrieve the subtitles page (html)
            var subtitlePage = GetUrl(BaseUrl + "view.php?id=" + subtitlePageId + "&m=subtitles#");
            if (subtitlePage == null)
            {
                return subtitlesList;
            }

            // Compile the RegEx string
            var subtitleListRegex = new Regex(SubtitleListPattern);

            // Create a list of all subtitles found on page
            var foundSubtitles = subtitleListRegex.Matches(subtitlePage);

            // Subtitles
            foreach (var foundSubtitle in foundSubtitles)
            {
                var groupCollection = subtitleListRegex.Match(foundSubtitle.ToString()).Groups;
                var fid = groupCollection["fid"].Value;
                var language = groupCollection["language"].Value;
                var title = groupCollection["title"].Value;

                // skip 0 length titles
                if (title.Length == 0)
                {
                    break;
                }

                // Check if the subtitles found match one of our languages was selected by the user
                string languageName;
                SratimToScript.TryGetValue(language, out languageName);
                if (languageName != null)
                {
                    string languageTwoLetter;
                    ToOpenSubtitlesTwoLetters.TryGetValue(languageName, out languageTwoLetter);
                    if (languageList.Contains(languageTwoLetter))
                    {
                        subtitlesList.Add(new SubtitleData(title, fid, languageName));
                    }
                }
            }

            return subtitlesList;
        }

        /// <summary>
        /// Same as getAllSubtitles() but receives season and episode numbers and find them.
        /// </summary>
        /// <param name="subtitlePageId">subtitle page id</param>
        /// <param name="languageList">list of languages</param>
        /// <param name="season">season number</param>
        /// <param name="episode">episode number</param>
        /// <returns></returns>
        private static IEnumerable<SubtitleData> GetAllTvSubtitles(string subtitlePageId,
            ICollection<string> languageList, string season, string episode)
        {
            // return object
            var subtitlesList = new List<SubtitleData>();

            // Retrieve the subtitles page (html)
            var subtitlePage = GetUrl(BaseUrl + "viewseries.php?id=" + subtitlePageId + "&m=subtitles#");
            if (subtitlePage == null)
            {
                return subtitlesList;
            }

            // Compile the RegEx string
            var tvSeasonRegex = new Regex(TvSeasonPattern);
            var tvEpisodeRegex = new Regex(TvEpisodePattern);
            var subtitleListRegex = new Regex(SubtitleListPattern);

            // Retrieve the requested season
            var foundSeasons = tvSeasonRegex.Matches(subtitlePage);

            // Seaosons
            foreach (var foundSeason in foundSeasons)
            {
                var tvSeasonGroups = tvSeasonRegex.Match(foundSeason.ToString()).Groups;
                var seasonLink = tvSeasonGroups["season_link"].Value;
                var seasonNum = tvSeasonGroups["season_num"].Value;

                if (seasonNum.Equals(season))
                {
                    // Retrieve the requested episode
                    subtitlePage =
                        GetUrl(BaseUrl + "viewseries.php?id=" + subtitlePageId + "&m=subtitles&s=" + seasonLink);
                    if (subtitlePage == null)
                    {
                        break;
                    }

                    var foundEpisodes = tvEpisodeRegex.Matches(subtitlePage);

                    // Episodes
                    foreach (var foundEpisode in foundEpisodes)
                    {
                        var episodeGroups = tvEpisodeRegex.Match(foundEpisode.ToString()).Groups;
                        var episodeLink = episodeGroups["episode_link"].Value;
                        var episodeNum = episodeGroups["episode_num"].Value;

                        if (episodeNum.Equals(episode))
                        {
                            subtitlePage =
                                GetUrl(BaseUrl + "viewseries.php?id=" + subtitlePageId + "&m=subtitles&s=" + seasonLink +
                                       "&e=" + episodeLink);
                            if (subtitlePage == null)
                            {
                                break;
                            }

                            // Create a list of all subtitles found on page
                            var foundSubtitles = subtitleListRegex.Matches(subtitlePage);

                            // Subtitles
                            foreach (var foundSubtitle in foundSubtitles)
                            {
                                var subtitleGroups = subtitleListRegex.Match(foundSubtitle.ToString()).Groups;
                                var fid = subtitleGroups["fid"].Value;
                                var language = subtitleGroups["language"].Value;
                                var title = subtitleGroups["title"].Value;

                                // skip 0 length titles
                                if (title.Length == 0)
                                {
                                    break;
                                }

                                // Check if the subtitles found match one of our languages was selected by the user
                                string languageName;
                                SratimToScript.TryGetValue(language, out languageName);
                                if (languageName != null)
                                {
                                    string languageTwoLetter;
                                    ToOpenSubtitlesTwoLetters.TryGetValue(languageName, out languageTwoLetter);
                                    if (languageList.Contains(languageTwoLetter))
                                    {
                                        subtitlesList.Add(new SubtitleData(title, fid, languageName));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return subtitlesList;
        }

        // ===============================================================================
        // Public interface functions
        // ===============================================================================

        /// <summary>
        /// This function is called when the service is selected through the subtitles addon OSD.
        /// </summary>
        /// <param name="title">Title of the movie or episode name</param>
        /// <param name="year">Query year</param>
        /// <param name="tvshow">Name of a tv show. Empty if video isn't a tv show (as are season and episode)</param>
        /// <param name="season">Season number</param>
        /// <param name="episode">Episode number</param>
        /// <param name="languageList">List of languages selected by the user</param>
        /// <param name="isImdb">is IMDB ID search, which means no match check</param>
        private static IEnumerable<SubtitleData> SearchSubtitles(string title, int? year, string tvshow, string season, string episode, ICollection<string> languageList, bool isImdb)
        {
            var subtitlesList = new List<SubtitleData>();

            // Check if searching for tv show or movie and build the search string
            string searchString;
            if (tvshow != null)
            {
                searchString = tvshow.Replace(" ", "+");
            }
            else
            {
                searchString = title.Replace(" ", "+");

                if (isImdb)
                {
                    searchString = "tt" + searchString;
                }

            }

            // Retrieve the search results (html)
            var searchResults = GetUrl(BaseUrl + @"browse.php\?q=" + searchString);
            if (searchResults == null)
            {
                throw new Exception("Search timed out, please try again later.");
            }

            // When searching for episode 1 Sratim.co.il returns episode 1,10,11,12 etc'
            // so we need to catch with out pattern the episode and season numbers and
            // only retrieve subtitles from the right result pages.
            if (tvshow != null)
            {
                // Find sratim's subtitle page IDs
                var tvSearchResultRegex = new Regex(TvSearchResultsPattern);
                var subtitleIDs = tvSearchResultRegex.Matches(searchResults);

                // Get a set of subtitle ids (this is to avoid searching the same id twice)
                var subtitleIDsSet = new HashSet<String>();
                foreach (var subtitleId in subtitleIDs)
                {
                    var groups = tvSearchResultRegex.Match(subtitleId.ToString()).Groups;
                    subtitleIDsSet.Add(groups[1].Value);
                }

                // Go over all the subtitle id and add results to our list if season and episode match
                foreach (var sid in subtitleIDsSet)
                {
                    // Get Subtitles
                    var tvSubtitles = GetAllTvSubtitles(sid, languageList, season, episode);
                    subtitlesList.AddRange(tvSubtitles);
                }
            }
            else // movie
            {
                // Lower case title for comparison
                var lowTitle = title.ToLower();
                // parse movie year from query for comparison
                var queryYear = (year != null) ? year.Value : 0;

                // Find sratim's subtitle page IDs
                var searchResultsRegex = new Regex(SearchResultsPattern);
                var subtitleIDs = searchResultsRegex.Matches(searchResults);

                // Maps sid --> title distance from query * year distance from query
                var moviesMatchDistance = new Dictionary<string, int>();

                // Go over all the results to find exact match or closest matches
                foreach (var subtitleId in subtitleIDs)
                {
                    var groups = searchResultsRegex.Match(subtitleId.ToString()).Groups;

                    // parse movie data from page
                    var sid = groups["movie_id"].Value;
                    var movieEnglishTitile = groups["movie_english"].Value.Trim().ToLower();
                    int movieYear;
                    if (!int.TryParse(groups["movie_year"].Value, out movieYear))
                    {
                        movieYear = 0;
                    }

                    // calculate distance
                    var titleDistance = LevenshteinDistance(movieEnglishTitile, lowTitle);

                    // if exact match (both title and year) search for subtitles and stop if found
                    if ((titleDistance == 0) && (YearsMatch(queryYear, movieYear)))
                    {
                        // Get exact match subtitles
                        var subtitles = GetAllSubtitles(sid, languageList);
                        var exactMatchSubtitles = new List<SubtitleData>(subtitles);
                        if (exactMatchSubtitles.Count > 0)
                        {
                            // exact match got results
                            return exactMatchSubtitles;
                        }
                    }
                    else // add not exact match to list
                    {
                        moviesMatchDistance.Add(sid, titleDistance);
                    }
                }

                // sort list
                var sortedMatches =
                    (from entry in moviesMatchDistance orderby entry.Value ascending select entry).ToDictionary(
                        pair => pair.Key, pair => pair.Value);

                // Try to find from closest 5 matches
                for (var i = 0; i < Math.Min(MaxMoviePagesToFollow, sortedMatches.Count); i++)
                {
                    var sid = sortedMatches.ElementAt(i).Key;
                    var distance = sortedMatches.ElementAt(i).Value;

                    // Get Subtitles
                    var subtitles = GetAllSubtitles(sid, languageList);
                    subtitlesList.AddRange(subtitles);

                    // break if exact match and there are some results
                    if ((distance == 1) && (subtitlesList.Count > 0))
                    {
                        break;
                    }
                }
            }

            return subtitlesList;
        }


        /// <summary>
        /// This function is called when a specific subtitle from the list generated by search_subtitles() is selected in the subtitles addon OSD.
        /// </summary>
        /// <param name="subtitlesList">Same list returned in search function</param>
        /// <param name="pos">The selected item's number in subtitles_list</param>
        private static List<FileInfo> DownloadSubtitles(IList<SubtitleData> subtitlesList, int pos)
        {
            var subtitleId = subtitlesList[pos].SubtitleId;

            var url = BaseUrl + "downloadsubtitle.php?id=" + subtitleId;

            // Going to write them to standrad zip file (always zips in sratim)
            var archiveFile = Path.GetTempFileName().Replace(".tmp", ".zip");

            // Download the file, use cookie
            DownloadFile(url, archiveFile);

            // Extract the zip file and find the new sub/srt file
            var extractFilesFromZipOrRarFile = FileUtils.ExtractFilesFromZipOrRarFile(archiveFile);

            return extractFilesFromZipOrRarFile.Where(HasSubtitleExtension).ToList();
        }


        // ===============================================================================
        // Utility functions
        // ===============================================================================

        /// <summary>
        /// Checks if file has subtitle extension
        /// </summary>
        /// <param name="fileInfo">file info</param>
        /// <returns>true if file has subtitle extension</returns>
        private static bool HasSubtitleExtension(FileSystemInfo fileInfo)
        {
            return SubtitleExtensions.Contains(fileInfo.Extension.ToLowerInvariant());
        }

        // ===============================================================================
        // Private data structures
        // ===============================================================================

        /// <summary>
        /// Data structure for subtitle object
        /// </summary>
        private class SubtitleData
        {
            private readonly string _filename;
            private readonly string _subtitleId;
            private readonly string _languageName;

            public SubtitleData(string filename, string subtitleId, string languageName)
            {
                _filename = filename;
                _subtitleId = subtitleId;
                _languageName = languageName;
            }

            public string Filename
            {
                get { return _filename; }
            }

            public string SubtitleId
            {
                get { return _subtitleId; }
            }

            public string LanguageName
            {
                get { return _languageName; }
            }
        }


        // ===============================================================================
        // Private dictionaries
        // ===============================================================================

        private static readonly List<string> SubtitleExtensions = new List<string>
        {
            ".aqt",
            ".asc",
            ".ass",
            ".dat",
            ".dks",
            "idx",
            ".js",
            ".jss",
            ".lrc",
            ".mpl",
            ".ovr",
            ".pan",
            ".pjs",
            ".psb",
            ".rt",
            ".rtf",
            ".s2k",
            ".sbt",
            ".scr",
            ".smi",
            ".son",
            ".srt",
            ".ssa",
            ".sst",
            ".ssts",
            ".stl",
            ".sub",
            ".vkt",
            ".vsf",
            ".zeg"
        };

        // Returns the corresponding script language name for the Hebrew unicode language
        private static readonly Dictionary<string, string> SratimToScript = new Dictionary<string, string>
        {
            {"עברית", "Hebrew"},
            {"אנגלית", "English"},
            {"ערבית", "Arabic"},
            {"צרפתית", "French"},
            {"גרמנית", "German"},
            {"רוסית", "Russian"},
            {"טורקית", "Turkish"},
            {"ספרדית", "Spanish"}
        };

        // Returns the corresponding two letters language code
        private static readonly Dictionary<string, string> ToOpenSubtitlesTwoLetters = new Dictionary<string, string>
        {
            {"None", "none"},
            {"Albanian", "sq"},
            {"Arabic", "ar"},
            {"Belarusian", "hy"},
            {"Bosnian", "bs"},
            {"BosnianLatin", "bs"},
            {"Bulgarian", "bg"},
            {"Catalan", "ca"},
            {"Chinese", "zh"},
            {"Croatian", "hr"},
            {"Czech", "cs"},
            {"Danish", "da"},
            {"Dutch", "nl"},
            {"English", "en"},
            {"Esperanto", "eo"},
            {"Estonian", "et"},
            {"Farsi", "fa"},
            {"Persian", "fa"},
            {"Finnish", "fi"},
            {"French", "fr"},
            {"Galician", "gl"},
            {"Georgian", "ka"},
            {"German", "de"},
            {"Greek", "el"},
            {"Hebrew", "he"},
            {"Hindi", "hi"},
            {"Hungarian", "hu"},
            {"Icelandic", "is"},
            {"Indonesian", "id"},
            {"Italian", "it"},
            {"Japanese", "ja"},
            {"Kazakh", "kk"},
            {"Korean", "ko"},
            {"Latvian", "lv"},
            {"Lithuanian", "lt"},
            {"Luxembourgish", "lb"},
            {"Macedonian", "mk"},
            {"Malay", "ms"},
            {"Norwegian", "no"},
            {"Occitan", "oc"},
            {"Polish", "pl"},
            {"Portuguese", "pt"},
            {"PortugueseBrazil", "pb"},
            {"Portuguese(Brazil)", "pb"},
            {"Brazilian", "pb"},
            {"Romanian", "ro"},
            {"Russian", "ru"},
            {"SerbianLatin", "sr"},
            {"Serbian", "sr"},
            {"Slovak", "sk"},
            {"Slovenian", "sl"},
            {"Spanish", "es"},
            {"Swedish", "sv"},
            {"Syriac", "syr"},
            {"Thai", "th"},
            {"Turkish", "tr"},
            {"Ukrainian", "uk"},
            {"Urdu", "ur"},
            {"Vietnamese", "vi"},
            {"English(US)", "en"},
            {"English(UK)", "en"},
            {"Portuguese(Brazilian)", "pt-br"},
            {"Español(Latinoamérica)", "es"},
            {"Español(España)", "es"},
            {"Spanish(LatinAmerica)", "es"},
            {"Español", "es"},
            {"Spanish(Spain)", "es"},
            {"Chinese(Traditional)", "zh"},
            {"Chinese(Simplified)", "zh"},
            {"Portuguese-BR", "pb"},
            {"All", "all"}
        };

        /// <summary>
        /// Convert 3 letters language code list to 2 letters language code list
        /// </summary>
        /// <param name="languageCodes">3 letters language code list</param>
        /// <returns>2 letters language code list</returns>
        private static List<string> ConvertThreeLetterToTwoLetterLanguageCodes(IEnumerable<string> languageCodes)
        {
            return languageCodes.Select(language => language.Substring(0, 2)).ToList();
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        private static int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                //Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        /// <summary>
        /// Returns true if movie year match query year ±1
        /// </summary>
        /// <param name="queryYear">query year</param>
        /// <param name="movieYear">movie year</param>
        /// <returns>true if a match</returns>
        private static bool YearsMatch(int queryYear, int movieYear)
        {
            // ignore 1 year distance
            return ((queryYear != 0) && (movieYear != 0) && (Math.Abs(queryYear - movieYear) <= 1));
        }
    }
}
