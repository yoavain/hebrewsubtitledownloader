using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SubtitleDownloader.Core;
using SubtitleDownloader.Util;

namespace SubsCenterOrg
{
  public class SubsCenterOrgDownoader : ISubtitleDownloader
  {
    private const string DownloadPageUrl = "http://www.subscenter.org/he/subtitle/download/";
    private const string SearchUrlBase = "http://www.subscenter.org/he/subtitle/search/?";
    private const string BaseUrl = "http://www.subscenter.org/he/subtitle";

    public int SearchTimeout { get; set; }

    public string GetName()
    {
      return "SubCenterOrg";
    }

    public List<Subtitle> SearchSubtitles(SearchQuery query)
    {
      var url = SearchUrlBase + "q=" + query.Query;
      // RegEx to find lyrics page
      const string findMoviePagePattern = "<a href=\\\"/he/subtitle/movie/(?<movie>.*?)/\\\">";

      // Get link to video
      var moviePath = FindPatternInPage(url, findMoviePagePattern).Groups[1].Value;
      var movieUrl = BaseUrl + "/movie/" + moviePath + "/";

      return Search(movieUrl, query);
    }

    public List<Subtitle> SearchSubtitles(EpisodeSearchQuery query)
    {
      var url = SearchUrlBase + "q=" + query.SerieTitle;
      // RegEx to find lyrics page
      const string findSeriesPagePattern = "<a href=\\\"/he/subtitle/series/(?<series>.*?)/\\\">";

      // Get link to series
      var seriesPath = FindPatternInPage(url, findSeriesPagePattern).Groups[1].Value;
      var episodeUrl = BaseUrl + "/series/" + seriesPath + "/" + query.Season + "/" + query.Episode;

      return Search(episodeUrl, query);
    }

    public List<Subtitle> SearchSubtitles(ImdbSearchQuery query)
    {
      throw new NotSupportedException();
    }

    public List<FileInfo> SaveSubtitle(Subtitle subtitle)
    {
      var url = DownloadPageUrl + GetLanguagePath(subtitle.LanguageCode) + "/" + subtitle.Id + "/?v=" + subtitle.FileName;
      var archiveFile = Path.GetTempFileName() + ".zip";

      var client = new WebClient();
      client.DownloadFile(url, archiveFile);

      return FileUtils.ExtractFilesFromZipOrRarFile(archiveFile);
    }

    private Match FindPatternInPage(string url, string findPagePattern)
    {
      Match match = null;

      var request = (HttpWebRequest)WebRequest.Create(url);
      if (SearchTimeout > 0)
      {
        request.Timeout = SearchTimeout * 1000;
      }
      var response = (HttpWebResponse)request.GetResponse();
      var responseStream = response.GetResponseStream();
      if (responseStream == null)
      {
        throw new Exception("Exception: Unable to query site");
      }

      var reader = new StreamReader(responseStream, Encoding.UTF8);
      try
      {
        var thisMayBeTheCorrectPage = false;

        while (!thisMayBeTheCorrectPage)
        {
          // Read line
          if (reader.EndOfStream)
          {
            break;
          }
          var line = reader.ReadLine() ?? "";

          // Try to find match in line
          match = Regex.Match(line, findPagePattern, RegexOptions.IgnoreCase);

          if (match.Success)
          {
            // Found page
            thisMayBeTheCorrectPage = true;
          }
        }

        // Not found
        if (!thisMayBeTheCorrectPage)
        {
          throw new Exception("Cannot find match");
        }
      }
      catch
      {
        throw new Exception("Exception: Cannot find match");
      }
      finally
      {
        reader.Close();
        responseStream.Close();
      }

      // return match
      return match;
    }

    private List<Subtitle> Search(string url, SubtitleSearchQuery query)
    {
      var subtitles = new List<Subtitle>();

      var subtitlesGroups = GetSubtitlesGroups(url);

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
                  default:
                    {
                      break;
                    }
                }
              }

              // Add subtitles - todo - add only if language is in query
              if (id != "" && subtitleVersion != "")
              {
                var version = subtitleVersion.Replace("\"", "");
                var languageCode = Languages.GetLanguageCode(language.Key.Replace("\"", ""));
                subtitles.Add(new Subtitle(id, version, version, languageCode));
              }
            }
          }
        }
      }

      return subtitles;
    }

    private string GetSubtitlesGroups(string url)
    {
      const string subtitlesGroupsPattern = "subtitles_groups = (?<subs>.*)";

      var match = FindPatternInPage(url, subtitlesGroupsPattern);
      if (match.Success)
      {
        return match.Groups[1].Value;
      }
      throw new Exception("cannot find subtitles");
    }

    /**
     * Returns subtitles as a map of:
     * Dictionary<language, Dictionary<group, Dictionary<quality, Dictionary<index, Dictionary<name, value>>>>>
     */
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

    private static string GetLanguagePath(string language)
    {
      if (language != null && language.Length >= 2)
      {
        return language.Substring(0, 2);
      }
      throw new Exception("Unknow language");

    }

    private static Dictionary<string, string> ParseLanguageOptions(SubtitleSearchQuery query)
    {
      return new Dictionary<string, string>()
                       {
                            { "Albanian", "29"},
                            { "Arabic", "12" },
                            { "Argentino", "14" },
                            { "Belarus", "50" },
                            { "Bosnian", "10" },
                            { "Brazilian", "48" },
                            { "Bulgarian", "33" },
                            { "Catalan", "53" },
                            { "Chinese", "17" },
                            { "Croatian", "38" },
                            { "Czech", "7" },
                            { "Danish", "24" },
                            { "Dutch", "23" },
                            { "English", "2" },
                            { "Estonian", "20" },
                            { "Farsi", "52" },
                            { "Finnish", "31" },
                            { "French", "8" },
                            { "German", "5" },
                            { "Greek", "16" },
                            { "Hebrew", "22" },
                            { "Hindi", "42" },
                            { "Hungarian", "15" },
                            { "Icelandic", "6" },
                            { "Indonesian", "54" },
                            { "Irish", "49" },
                            { "Italian", "9" },
                            { "Japanese", "11" },
                            { "Korean", "4" },
                            { "Latvian", "21" },
                            { "Lithuanian", "19" },
                            { "Macedonian", "35" },
                            { "Malay", "55" },
                            { "Mandarin", "40" },
                            { "Norwegian", "3" },
                            { "Polish", "26" },
                            { "Portuguese", "32" },
                            { "Romanian", "13" },
                            { "Russian", "27" },
                            { "Serbian", "36" },
                            { "Slovak", "37" },
                            { "Slovenian", "1" },
                            { "Spanish", "28" },
                            { "Swedish", "25" },
                            { "Thai", "44" },
                            { "Turkish", "30" },
                            { "Ukrainian", "46" },
                            { "Vietnamese", "51" },
                       };
    }
  }
}
