﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using SubtitleDownloader.Core;
using SubtitleDownloader.Util;

namespace Sratim
{
  public class SratimDownloader : ISubtitleDownloader
  {

    private const string BaseUrl = "http://www.sratim.co.il/";

    // ===============================================================================
    // Regular expression patterns
    // ===============================================================================

    private const string TvSearchResultsPattern = @"<div style=""""><a href=""viewseries.php\?id=(\d+)";
    private const string SearchResultsPattern = @"<div style=""""><a href=""view.php\?id=(\d+)";
    private const string SubtitleListPattern = @"downloadsubtitle.php\?id=(?<fid>\d*).*?subt_lang.*?title=\""(?<language>.*?)\"".*?subtitle_title.*?title=\""(?<title>.*?)\""";
    private const string TvSeasonPattern = @"seasonlink_(?<season_link>\d+).*?>(?<season_num>\d+)</a>";
    private const string TvEpisodePattern = @"episodelink_(?<episode_link>\d+).*?>(?<episode_num>\d+)</a>";

    public string GetName()
    {
      return "Sratim.co.il";
    }

    public List<Subtitle> SearchSubtitles(SearchQuery query)
    {
      var subsList = new List<Subtitle>();
      var subtitles = SearchSubtitles("", query.Query, null, query.Year.ToString(), null, null, false, false, query.LanguageCodes, "");
      foreach (var subtitle in subtitles)
      {
        subsList.Add(new Subtitle(subtitle.SubtitleId, query.Query, subtitle.Filename, subtitle.LanguageFlag));
      }
      return subsList;
    }

    public List<Subtitle> SearchSubtitles(EpisodeSearchQuery query)
    {
      var subsList = new List<Subtitle>();
      var subtitles = SearchSubtitles("", null, query.SerieTitle, "" , query.Season.ToString(), query.Episode.ToString(), false, false, query.LanguageCodes, "");
      foreach (var subtitle in subtitles)
      {
        subsList.Add(new Subtitle(subtitle.SubtitleId, query.SerieTitle, subtitle.Filename, subtitle.LanguageName));
      }
      return subsList;
    }

    public List<Subtitle> SearchSubtitles(ImdbSearchQuery query)
    {
      throw new NotImplementedException();
    }

    public List<FileInfo> SaveSubtitle(Subtitle subtitle)
    {
      var fileName = subtitle.FileName;
      var subtitleId = subtitle.Id;
      var languageName = subtitle.LanguageCode;
      string languageCode;
      ToOpenSubtitlesTwoLetters.TryGetValue(languageName, out languageCode);

      var subtitleData = new SubtitleData("", false, fileName, subtitleId, languageCode, languageName);
      var subtitles = new List<SubtitleData>(1) {subtitleData};

      var savedSubtitle = DownloadSubtitles(subtitles, 0, "", "", "", "");

      return savedSubtitle;
    }

    public int SearchTimeout { get; set; }


    // ===============================================================================
    // Private utility functions
    // ===============================================================================
    

    /// <summary>
    /// Returns the content of the given URL
    /// </summary>
    /// <param name="url">the url</param>
    /// <returns>the content of the page</returns>
    private static string GetUrl(string url)
    {
      var client = new WebClient();
      return client.DownloadString(url);
    }

    /// <summary>
    /// The function receives a subtitles page id number, a list of user selected
    /// languages and the current subtitles list and adds all found subtitles matching
    /// the language selection to the subtitles list.
    /// </summary>
    /// <param name="subtitlePageId">subtitle page id</param>
    /// <param name="languageList">list of languages</param>
    /// <returns></returns>
    private static List<SubtitleData> GetAllSubtitles(string subtitlePageId, ICollection<string> languageList)
    {
      // return object
      var subtitlesList = new List<SubtitleData>();

      // Retrieve the subtitles page (html)
      var subtitlePage = GetUrl(BaseUrl + "view.php?id=" + subtitlePageId + "&m=subtitles#");

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

        Console.Write("fid = " + fid);
        Console.Write("language = " + language);
        Console.Write("title = " + title);

        // Check if the subtitles found match one of our languages was selected by the user
        string languageName;
        SratimToScript.TryGetValue(language, out languageName);
        string languageTwoLetter;
        ToOpenSubtitlesTwoLetters.TryGetValue(languageName, out languageTwoLetter);
        if (languageList.Contains(languageName))
        {
          subtitlesList.Add(new SubtitleData("0", false, title, fid, "flags" + languageTwoLetter + ".gif", languageName));
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
    private static List<SubtitleData>  GetAllTvSubtitles(string subtitlePageId, ICollection<string> languageList, string season,string episode)
    {
      // return object
      var subtitlesList = new List<SubtitleData>();

      // Retrieve the subtitles page (html)
      var subtitlePage = GetUrl(BaseUrl + "viewseries.php?id=" + subtitlePageId + "&m=subtitles#");

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
          subtitlePage = GetUrl(BaseUrl + "viewseries.php?id=" + subtitlePageId + "&m=subtitles&s=" + seasonLink);
          var foundEpisodes = tvEpisodeRegex.Matches(subtitlePage);

          // Episodes
          foreach(var foundEpisode in foundEpisodes)
          {
            var episodeGroups = tvEpisodeRegex.Match(foundEpisode.ToString()).Groups;
            var episodeLink = episodeGroups["episode_link"].Value;
            var episodeNum = episodeGroups["episode_num"].Value;

            if (episodeNum.Equals(episode))
            {
              subtitlePage = GetUrl(BaseUrl + "viewseries.php?id=" + subtitlePageId + "&m=subtitles&s=" + seasonLink + "&e=" + episodeLink);

              // Create a list of all subtitles found on page
              var foundSubtitles = subtitleListRegex.Matches(subtitlePage);

              // Subtitles
              foreach (var foundSubtitle in foundSubtitles)
              {
                var subtitleGroups = subtitleListRegex.Match(foundSubtitle.ToString()).Groups;
                var fid = subtitleGroups["fid"].Value;
                var language = subtitleGroups["language"].Value;
                var title = subtitleGroups["title"].Value;

                // Check if the subtitles found match one of our languages was selected by the user
                string languageName;
                SratimToScript.TryGetValue(language, out languageName);
                string languageTwoLetter;
                ToOpenSubtitlesTwoLetters.TryGetValue(languageName, out languageTwoLetter);
                if (languageList.Contains(languageName))
                {
                  subtitlesList.Add(new SubtitleData("0", false, title, fid, "flags" + languageTwoLetter + ".gif", languageName));
                }
              }
            }
          }
        }
      }

      return subtitlesList;
    }

    /// <summary>
    /// Extracts the downloaded file and find a new sub/srt file to return.
    /// Sratim.co.il currently isn't hosting subtitles in .txt format but
    /// is adding txt info files in their zips, hence not looking for txt.
    /// </summary>
    /// <param name="tempSubDir">temporary subtitle directory</param>
    /// <param name="tempZipFile">tmep zip file</param>
    private static void ExtractAndFindSub(string tempSubDir,string tempZipFile)
    {
      throw new NotImplementedException();
    }

    // ===============================================================================
    // Public interface functions
    // ===============================================================================

    /// <summary>
    /// This function is called when the service is selected through the subtitles addon OSD.
    /// </summary>
    /// <param name="fileOriginalPath">Original system path of the file playing</param>
    /// <param name="title">Title of the movie or episode name</param>
    /// <param name="tvshow">Name of a tv show. Empty if video isn't a tv show (as are season and episode)</param>
    /// <param name="year">Year</param>
    /// <param name="season">Season number</param>
    /// <param name="episode">Episode number</param>
    /// <param name="setTemp">True iff video is http:// stream</param>
    /// <param name="rar">True iff video is inside a rar archive</param>
    /// <param name="languageList">List of languages selected by the user</param>
    /// <param name="stack"></param>
    private static IEnumerable<SubtitleData> SearchSubtitles(string fileOriginalPath, string title, string tvshow, string year, string season, string episode, bool setTemp, bool rar,
      ICollection<string> languageList, string stack)
    {
      var subtitlesList = new List<SubtitleData>();

      // Check if searching for tv show or movie and build the search string
      var searchString = tvshow != null ? tvshow.Replace(" ", "+") : title.Replace(" ", "+");

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

        // Go over all the subtitle pages and add results to our list if season and episode match
        foreach (var subtitleId in subtitleIDs)
        {
          var groups = tvSearchResultRegex.Match(subtitleId.ToString()).Groups;
          var sid = groups[1].Value;

          subtitlesList = GetAllTvSubtitles(sid, languageList, season, episode);
        }
      }
      else
      {
        // Find sratim's subtitle page IDs
        var searchResultsRegex = new Regex(SearchResultsPattern);
        var subtitleIDs = searchResultsRegex.Matches(searchResults);

        // Go over all the subtitle pages and add results to our list
        foreach(var subtitleId in subtitleIDs)
        {
          var groups = searchResultsRegex.Match(subtitleId.ToString()).Groups;
          var sid = groups[1].Value;

          subtitlesList = GetAllSubtitles(sid, languageList);
        }
      }

      return subtitlesList;
    }


    /// <summary>
    /// This function is called when a specific subtitle from the list generated by search_subtitles() is selected in the subtitles addon OSD.
    /// </summary>
    /// <param name="subtitlesList">Same list returned in search function</param>
    /// <param name="pos">The selected item's number in subtitles_list</param>
    /// <param name="zipSubs">Full path of zipsubs.zip located in tmp location, if automatic extraction is used (see return values for details)</param>
    /// <param name="tmpSubDir">Temp folder used for both automatic and manual extraction</param>
    /// <param name="subFolder">Folder where the sub will be saved</param>
    /// <param name="sessionId">Same session_id returned in search function</param>
    private static List<FileInfo> DownloadSubtitles(IList<SubtitleData> subtitlesList, int pos, string zipSubs, string tmpSubDir, string subFolder, string sessionId)
    {
      var subtitleId = subtitlesList[pos].SubtitleId;
      var language = subtitlesList[pos].LanguageName;

      var url = BaseUrl + "downloadsubtitle.php?id=" + subtitleId;
      
      // Going to write them to standrad zip file (always zips in sratim)
      var archiveFile = Path.GetTempFileName().Replace(".tmp", ".zip");

      var client = new WebClient();
      client.DownloadFile(url, archiveFile);

      // Extract the zip file and find the new sub/srt file
      var extractFilesFromZipOrRarFile = FileUtils.ExtractFilesFromZipOrRarFile(archiveFile);
      var fileFist = new List<FileInfo>();
      foreach (var fileInfo in extractFilesFromZipOrRarFile)
      {
        if (HasSubtitleExtension(fileInfo))
        {
          fileFist.Add(fileInfo);
        }
      }

      return fileFist;
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
      private readonly string _rating;
      private readonly bool _sync;
      private readonly string _filename;
      private readonly string _subtitleId;
      private readonly string _languageFlag;
      private readonly string _languageName;

      public SubtitleData(string rating, bool sync, string filename, string subtitleId, string languageFlag, string languageName)
      {
        this._rating = rating;
        this._sync = sync;
        this._filename = filename;
        _subtitleId = subtitleId;
        _languageFlag = languageFlag;
        _languageName = languageName;
      }

      public string Rating
      {
        get { return _rating; }
      }

      public bool Sync
      {
        get { return _sync; }
      }

      public string Filename
      {
        get { return _filename; }
      }

      public string SubtitleId
      {
        get { return _subtitleId; }
      }

      public string LanguageFlag
      {
        get { return _languageFlag; }
      }

      public string LanguageName
      {
        get { return _languageName; }
      }
    }


    // ===============================================================================
    // Private dictionaries
    // ===============================================================================

    private static readonly List<string> SubtitleExtensions = new List<string> { ".aqt", ".asc", ".ass", ".dat", ".dks", "idx", ".js", ".jss",
                                                                                 ".lrc", ".mpl", ".ovr", ".pan", ".pjs", ".psb", ".rt", ".rtf",
                                                                                 ".s2k", ".sbt", ".scr", ".smi", ".son", ".srt", ".ssa", ".sst",
                                                                                 ".ssts", ".stl", ".sub", ".vkt", ".vsf", ".zeg" };

    // Returns the corresponding script language name for the Hebrew unicode language
    private static readonly Dictionary<string, string> SratimToScript = new Dictionary<string, string>
                                                                          {
                                                                            {"עברית", "Hebrew"},
                                                                            {"אנגלית", "English"},
                                                                            {"ערבית", "Arabic"},
                                                                            {"צרפתית","French"},
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

  }
}