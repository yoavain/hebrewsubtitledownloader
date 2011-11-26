using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SubtitleDownloader.Core;
using SubtitleDownloader.Util;

namespace SubsCenterOrg
{
  public class SubsCenterOrgDownloader : ISubtitleDownloader
  {
    private static readonly List<string> SubtitleExtensions = new List<string> { ".aqt", ".asc", ".ass", ".dat", ".dks", "idx", ".js", ".jss",
                                                                                 ".lrc", ".mpl", ".ovr", ".pan", ".pjs", ".psb", ".rt", ".rtf",
                                                                                 ".s2k", ".sbt", ".scr", ".smi", ".son", ".srt", ".ssa", ".sst",
                                                                                 ".ssts", ".stl", ".sub", ".vkt", ".vsf", ".zeg" };

    private const string BaseUrl = "http://www.subscenter.org";
    private const string ExactMovieUrl = "http://www.subscenter.org/he/subtitle/movie/";
    private const string ExactSeriesUrl = "http://www.subscenter.org/he/subtitle/series/";
    private const string SearchUrlBase = "http://www.subscenter.org/he/subtitle/search/?";
    private const string DownloadPageUrl = "http://www.subscenter.org/he/subtitle/download/";
    private const string MovieSubtitlePath = "/he/subtitle/movie/";
    private const string SeriesSubtitlePath = "/he/subtitle/series/";
    private const string Error404String = "Error 404";
    private const string NoResultsString = "processesDescription";


    public int SearchTimeout { get; set; }

    public string GetName()
    {
      return "SubsCenter.Org";
    }

    public List<Subtitle> SearchSubtitles(SearchQuery query)
    {
      var gotMatch = false;
      var retries = 0;

      // try guessing exact movie url
      var exactMovieUrl = ExactMovieUrl + query.Query.Replace(" ", "-").ToLower() + "/";

      //  download "exact" page
      var web = new HtmlWeb();
      var moviePage = web.Load(exactMovieUrl);

      // handle "Error 404" - page not found
      if (IsPageNotFound(moviePage) || !YearMatch(moviePage, query.Year) || !TitleMatch(moviePage, query.Query))
      {
        // search url
        var queryUrl = SearchUrlBase + "q=" + query.Query;
        var queryPage = web.Load(queryUrl);

        if (!IsNoResults(queryPage))
        {
          var htmlNodeCollection = queryPage.DocumentNode.SelectNodes("//a");
          foreach (var node in htmlNodeCollection)
          {
            var attributeValue = node.GetAttributeValue("href", string.Empty);
            if (attributeValue.StartsWith(MovieSubtitlePath))
            {
              // download new page
              moviePage = web.Load(BaseUrl + attributeValue);
              if (YearMatch(moviePage, query.Year) && TitleMatch(moviePage, query.Query))
              {
                gotMatch = true;
                break;
              }
              if (++retries >= 3)
              {
                break;
              }
            }
          }
        }
      }
      else
      {
        gotMatch = true;
      }

      // verify year and title
      if (gotMatch)
      {
        return Search(moviePage, query);
      }
      return new List<Subtitle>();

    }

    public List<Subtitle> SearchSubtitles(EpisodeSearchQuery query)
    {
      var gotMatch = false;
      var retries = 0;
      var web = new HtmlWeb();

      // If needed - use configuration XML file to replace received title with a different value
      var title = SubsCenterOrgDownloaderConfiguration.Instance.OverrideTitleFromConfiguration(query.SerieTitle);

      // clean title
      var cleanTitle = CleanTitleName(title);

      // try guessing exact episode url
      var exactSeriesUrl = ExactSeriesUrl + title.Replace(" ", "-");
      var exactEpisodeUrl = exactSeriesUrl + "/" + query.Season + "/" + query.Episode;

      //  download "exact" pages
      var mainSeriesPage = web.Load(exactSeriesUrl);
      var episodePage = web.Load(exactEpisodeUrl);

      // if page not found and can clean title name - use clean name
      if (!cleanTitle.Equals(title) && (IsPageNotFound(mainSeriesPage) || IsPageNotFound(episodePage)))
      {
        // try guessing exact episode url again
        exactSeriesUrl = ExactSeriesUrl + cleanTitle.Replace(" ", "-");
        exactEpisodeUrl = exactSeriesUrl + "/" + query.Season + "/" + query.Episode;
        //  download "exact" pages again
        mainSeriesPage = web.Load(exactSeriesUrl);
        episodePage = web.Load(exactEpisodeUrl);
      }

      // handle "Error 404" - page not found
      if (IsPageNotFound(episodePage) || !TitleSeasonEpisodeMatch(mainSeriesPage, episodePage, title, cleanTitle, query.Season, query.Episode))
      {
        // todo - get number of result pages and loop
        // search url
        var queryUrl = SearchUrlBase + "q=" + cleanTitle;
        var queryPage = web.Load(queryUrl);

        var htmlNodeCollection = queryPage.DocumentNode.SelectNodes("//a");
        foreach (var node in htmlNodeCollection)
        {
          var attributeValue = node.GetAttributeValue("href", string.Empty);
          if (attributeValue.StartsWith(SeriesSubtitlePath))
          {
            // todo - before downloading page - check title from attribute
            // download new pages
            mainSeriesPage = web.Load(BaseUrl + attributeValue + "/" + query.Season + "/" + query.Episode);
            episodePage = web.Load(BaseUrl + attributeValue + "/" + query.Season + "/" + query.Episode);
            if (TitleSeasonEpisodeMatch(mainSeriesPage, episodePage, title, cleanTitle, query.Season, query.Episode))
            {
              gotMatch = true;
              break;
            }
            if (++retries >= 3)
            {
              break;
            }
          }
        }
      }
      else
      {
        gotMatch = true;
      }

      // verify
      if (gotMatch)
      {
        return Search(episodePage, query);
      }
      return new List<Subtitle>();
    }

    public List<Subtitle> SearchSubtitles(ImdbSearchQuery query)
    {
      throw new NotSupportedException();
    }

    public List<FileInfo> SaveSubtitle(Subtitle subtitle)
    {
      var idAndKey = subtitle.Id.Split(' ');
      if (idAndKey.Length != 2)
      {
        throw new FileNotFoundException("Cannot find file");
      }

      var url = DownloadPageUrl + GetLanguagePath(subtitle.LanguageCode) + "/" + idAndKey[0] + "/?v=" + subtitle.FileName + "&key=" + idAndKey[1];
      var archiveFile = Path.GetTempFileName().Replace(".tmp", ".zip");

      var client = new WebClient();
      client.DownloadFile(url, archiveFile);

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


    /// <summary>
    /// Search subtitles in page
    /// </summary>
    /// <param name="page">html page</param>
    /// <param name="query">the search query</param>
    /// <returns>List of subtitles</returns>
    private static List<Subtitle> Search(HtmlDocument page, SubtitleSearchQuery query)
    {
      var subtitles = new List<Subtitle>();

      var subtitlesGroups = GetSubtitlesGroups(page);

      var parsedSubtitles = ParseSubtitlesGroups(subtitlesGroups);

      foreach (var language in parsedSubtitles)
      {
        foreach (var group in language.Value)
        {
          foreach (var quality in group.Value)
          {
            foreach (var index in quality.Value)
            {
              // parse each subtitle here
              // mandatory fields: id, subtitle_version, 
              var id = "";
              var subtitleVersion = "";
              var key = "";
              foreach (var param in index.Value)
              {
                switch (param.Key)
                {
                  case "\"id\"":
                    {
                      id = param.Value;
                      break;
                    }
                  case "\"subtitle_version\"":
                    {
                      subtitleVersion = param.Value;
                      break;
                    }
                  case "\"key\"":
                    {
                      key = param.Value;
                      break;
                    }
                  default:
                    {
                      break;
                    }
                }
              }

              // Add subtitles
              if (id != "" && subtitleVersion != "" && key != "")
              {
                var version = subtitleVersion.Replace("\"", "");
                var cleanKey = key.Replace("\"", "");
                string languageName;
                LanguageShortToLongCodeDictionary.TryGetValue(language.Key.Replace("\"", ""), out languageName);
                var languageCode = Languages.GetLanguageCode(languageName);

                // Check language
                if (query.HasLanguageCode(languageCode))
                {
                  subtitles.Add(new Subtitle(id + " " + cleanKey, version, version, languageCode));
                }
              }
            }
          }
        }
      }

      return subtitles;
    }
    
    /// <summary>
    /// Return the subtitles_groups from java script
    /// </summary>
    /// <param name="page">html page</param>
    /// <returns>subtitles_groups</returns>
    private static string GetSubtitlesGroups(HtmlDocument page)
    {
      var htmlNodeCollection = page.DocumentNode.SelectNodes("//script");
      foreach (var node in htmlNodeCollection)
      {
        if (node.InnerText.Contains("subtitles_groups"))
        {
          var match = Regex.Match(node.InnerText, "subtitles_groups = (?<subs>.*)");
          if (match.Groups.Count == 2)
          {
            return match.Groups[1].Value;
          }
        }
      }
      throw new Exception("cannot find subtitles.");
    }

    /// <summary>
    /// Returns map of subtitles
    /// </summary>
    /// <param name="subtitlesGroup">the subtitles_groups from java script</param>
    /// <returns>Dictionary&lt;language, Dictionary&lt;group, Dictionary&lt;quality, Dictionary&lt;index, Dictionary&lt;name, value&gt;&gt;&gt;&gt;&gt;</returns>
    private static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>> ParseSubtitlesGroups(string subtitlesGroup)
    {
      // tokens
      const string splitPattern = @"({)|(})|(, )|(: )";
      var tokens = Regex.Split(subtitlesGroup, splitPattern);

      // parsing state
      var level = 0;
      var isValue = false;
      var output = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
      Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> currentLanguage = null;
      Dictionary<string, Dictionary<string, Dictionary<string, string>>> currentGroup = null;
      Dictionary<string, Dictionary<string, string>> currentQuality = null;
      Dictionary<string, string> currentIdx = null;
      var currentName = "";

      foreach (var token in tokens)
      {
        if (token.Length == 0)
        {
          continue;
        }

        switch (token[0])
        {
          case '{':
            {
              level++;
              break;
            }
          case '}':
            {
              level--;
              break;
            }
          case ',':
            {
              isValue = false;
              break;
            }
          case ':':
            {
              if (level == 5)
              {
                isValue = true;
              }
              break;
            }
          default:
            {
              switch (level)
              {
                case 1:
                  {
                    // language
                    currentLanguage = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();
                    output.Add(token, currentLanguage);
                    break;
                  }
                case 2:
                  {
                    // group
                    currentGroup = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    if (currentLanguage != null)
                    {
                      currentLanguage.Add(token, currentGroup);
                    }
                    break;
                  }
                case 3:
                  {
                    // quality
                    currentQuality = new Dictionary<string, Dictionary<string, string>>();
                    if (currentGroup != null)
                    {
                      currentGroup.Add(token, currentQuality);
                    }
                    break;
                  }
                case 4:
                  {
                    // index 
                    currentIdx = new Dictionary<string, string>();
                    if (currentQuality != null)
                    {
                      currentQuality.Add(token, currentIdx);
                    }
                    break;
                  }
                default:
                  {
                    // name/value
                    if (!isValue)
                    {
                      currentName = token;
                    }
                    else
                    {
                      if (currentIdx != null && !currentIdx.ContainsKey(currentName))
                      {
                        currentIdx.Add(currentName, token);
                      }
                      isValue = false;
                    }
                  } // end inner default
                  break;
              } // end inner switch
            } // end default
            break;
        } // end switch
      }

      // output
      return output;
    }

    /// <summary>
    /// Return language path name
    /// </summary>
    /// <param name="language">full language name</param>
    /// <returns>language path name ("en" for English, "he" for Hebrew", etc.)</returns>
    private static string GetLanguagePath(string language)
    {
      if (language != null && language.Length >= 2)
      {
        return language.Substring(0, 2);
      }
      throw new Exception("Unknow language " + language);
    }

    /// <summary>
    /// Checks if page is site's "Error 404"
    /// </summary>
    /// <param name="page">html page</param>
    /// <returns>true if "Error 404" is found in page</returns>
    private static bool IsPageNotFound(HtmlDocument page)
    {
      // Get all img nodes and search for "Error 404"
      var images = page.DocumentNode.SelectNodes("//img");
      foreach (var node in images)
      {
        var alt = node.GetAttributeValue("alt", string.Empty);
        if (alt.Equals(Error404String))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Checks if query has no results
    /// </summary>
    /// <param name="page">html page</param>
    /// <returns>true if query had no result</returns>
    private static bool IsNoResults(HtmlDocument page)
    {
      // Get all img nodes and search for indicator of no results
      var images = page.DocumentNode.SelectNodes("//div");
      foreach (var node in images)
      {
        var alt = node.GetAttributeValue("id", string.Empty);
        if (alt.Equals(NoResultsString))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Checks if year matches
    /// </summary>
    /// <param name="moviePage">movie html page</param>
    /// <param name="expectedYear">expected year</param>
    /// <returns>true if year matches</returns>
    private static bool YearMatch(HtmlDocument moviePage, int? expectedYear)
    {
      if (expectedYear == null)
      {
        return true; // no year in query
      }

      try
      {
        var year = int.Parse(Regex.Match(moviePage.DocumentNode.SelectNodes("//h1")[0].ParentNode.InnerText, "\\d+").Value);
        return Math.Abs((int)(expectedYear - year)) <= 1;
      }
      catch
      {
        return false; // page does not contain year
      }
    }

    /// <summary>
    /// Checks if title matches
    /// </summary>
    /// <param name="moviePage">movie page</param>
    /// <param name="expectedTitle">expected title</param>
    /// <returns>true if title matches</returns>
    private static bool TitleMatch(HtmlDocument moviePage, string expectedTitle)
    {
      try
      {
        var title = moviePage.DocumentNode.SelectNodes("//h3")[0].InnerText.Trim();
        return expectedTitle.Equals(title, StringComparison.OrdinalIgnoreCase);
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Checks if series title, season & episode match
    /// </summary>
    /// <param name="mainSeriesPage">main series html page</param>
    /// <param name="episodePage">episode html page</param>
    /// <param name="expectedTitle">expected title</param>
    /// <param name="expectedCleanTitle">expected clean title</param>
    /// <param name="expectedSeason">expected season</param>
    /// <param name="expectedEpisode">expected episode</param>
    /// <returns>true if Title, Season & Episode match</returns>
    private static bool TitleSeasonEpisodeMatch(HtmlDocument mainSeriesPage, HtmlDocument episodePage, string expectedTitle, string expectedCleanTitle, int expectedSeason, int expectedEpisode)
    {
      try
      {
        var title = mainSeriesPage.DocumentNode.SelectNodes("//h3")[0].InnerText.Replace(".", "").ToLower().Trim();

        var spanPageName = episodePage.DocumentNode.SelectNodes("//span");
        foreach (var node in spanPageName)
        {
          var atr = node.GetAttributeValue("class", string.Empty);
          if (atr.Equals("pageName"))
          {
            var seasonEpisodeText = node.InnerText.Trim();
            var matches = Regex.Matches(seasonEpisodeText, "\\d+");
            var season = int.Parse(matches[0].Value);
            var episode = int.Parse(matches[1].Value);

            return ((title.Equals(expectedTitle) || (title.Equals(expectedCleanTitle))) && (season == expectedSeason) && (episode == expectedEpisode));
          }
        }

        return false;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Clean title name. Removes anything in parenthesis, change to lower case and trims.
    /// </summary>
    /// <param name="title">title</param>
    /// <returns>clean title name</returns>
    private static string CleanTitleName(string title)
    {
      if (title == null)
      {
        return null;
      }
      return !title.Contains("(") ? title.ToLower().Trim() : title.Substring(0, title.IndexOf("(")).ToLower().Trim();
    }

    /// <summary>
    /// Checks if file has subtitle extension
    /// </summary>
    /// <param name="fileInfo">file info</param>
    /// <returns>true if file has subtitle extension</returns>
    private static bool HasSubtitleExtension(FileSystemInfo fileInfo)
    {
      return SubtitleExtensions.Contains(fileInfo.Extension.ToLowerInvariant());
    }

    /// <summary>
    /// Translate 2 letters language code to language full name
    /// </summary>
    private static readonly Dictionary<string, string> LanguageShortToLongCodeDictionary = new Dictionary<string, string>
                                                                              {
                                                                                {"ar", "Arabic"},
                                                                                {"be", "Belarus"},
                                                                                {"bg", "Bulgarian"},
                                                                                {"br", "Brazilian"},
                                                                                {"bs", "Bosnian"},
                                                                                {"ca", "Catalan"},
                                                                                {"cs", "Czech"},
                                                                                {"da", "Danish"},
                                                                                {"de", "German"},
                                                                                {"en", "English"},
                                                                                {"es", "Spanish"},
                                                                                {"et", "Estonian"},
                                                                                {"fi", "Finnish"},
                                                                                {"fr", "French"},
                                                                                {"ga", "Irish"},
                                                                                {"gr", "Greek"},
                                                                                {"he", "Hebrew"},
                                                                                {"hi", "Hindi"},
                                                                                {"hu", "Hungarian"},
                                                                                {"hr", "Croatian"},
                                                                                {"is", "Icelandic"},
                                                                                {"id", "Indonesian"},
                                                                                {"it", "Italian"},
                                                                                {"ja", "Japanese"},
                                                                                {"ko", "Korean"},
                                                                                {"lv", "Latvian"},
                                                                                {"lt", "Lithuanian"},
                                                                                {"mk", "Macedonian"},
                                                                                {"ms", "Malay"},
                                                                                {"nl", "Dutch"},
                                                                                {"no", "Norwegian"},
                                                                                {"pl", "Polish"},
                                                                                {"pt", "Portuguese"},
                                                                                {"ro", "Romanian"},
                                                                                {"ru", "Russian"},
                                                                                {"sr", "Serbian"},
                                                                                {"sk", "Slovak"},
                                                                                {"sl", "Slovenian"},
                                                                                {"sv", "Swedish"},
                                                                                {"th", "Thai"},
                                                                                {"tr", "Turkish"},
                                                                                {"uk", "Ukrainian"},
                                                                                {"vi", "Vietnamese"},
                                                                                {"zh", "Chinese"}
                                                                              };

  }
}
