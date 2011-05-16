using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubsCenterOrg;
using SubtitleDownloader.Core;

namespace SubsCenterOrgTest
{
  [TestClass]
  public class SubsCenterOrgTest
  {
    [TestMethod]
    public void TestExactMovieSearch()
    {
      var downloader = new SubsCenterOrgDownoader();
      var query = new SearchQuery("batman begins") { Year = 2005, LanguageCodes = new[] { "heb"} };
      var results = downloader.SearchSubtitles(query);

      // make sure there are resuts
      Assert.IsNotNull(results);
      Assert.IsTrue(results.Count > 0);

      // check first result
      var subtitleFiles = downloader.SaveSubtitle(results[10]);
      Assert.AreNotEqual(null, subtitleFiles);
      Assert.AreNotEqual(0, subtitleFiles.Count);
    }

    [TestMethod]
    public void TestNoMatchMovieSearch()
    {
      var downloader = new SubsCenterOrgDownoader();
      var query = new SearchQuery("batman ends") { Year = 2015, LanguageCodes = new[] { "eng"} };
      var results = downloader.SearchSubtitles(query);

      // make sure there are no resuts
      Assert.IsNotNull(results);
      Assert.IsTrue(results.Count == 0);
    }

    [TestMethod]
    public void TestSeriesSearch()
    {
      var downloader = new SubsCenterOrgDownoader();
      var query = new EpisodeSearchQuery("house md", 6, 15) { LanguageCodes = new[] { "heb"} };
      var results = downloader.SearchSubtitles(query);

      // make sure there are resuts
      Assert.IsNotNull(results);
      Assert.IsTrue(results.Count > 0);

      // check first result
      var subtitleFiles = downloader.SaveSubtitle(results[0]);
      Assert.AreNotEqual(null, subtitleFiles);
      Assert.AreNotEqual(0, subtitleFiles.Count);
    }

    /*
    [TestMethod]
    public void TestUnzip()
    {
      const string url = "http://www.subscenter.org/he/subtitle/download/he/10585/?v=batman.begins.X264.hd-DVD.SRT";
      var archiveFile = Path.GetTempFileName() + ".zip";

      var client = new WebClient();
      client.DownloadFile(url, archiveFile);

      var extractFilesFromZipOrRarFile = FileUtils.ExtractFilesFromZipOrRarFile(archiveFile);
      Assert.AreNotEqual(null, extractFilesFromZipOrRarFile);
      Assert.AreEqual(1, extractFilesFromZipOrRarFile.Count);
    }
    */

    /*
    [TestMethod]
    public void TestPattern()
    {
      const string findMoviePagePattern = "<a href=\\\"/he/subtitle/movie/(?<movie>.*)/\\\">";
      const string movieLine = "<a href=\"/he/subtitle/movie/batman-begins/\">באטמן מתחיל / Batman Begins</a>";
      var movieMatch = Regex.Match(movieLine, findMoviePagePattern, RegexOptions.IgnoreCase);
      Assert.AreEqual("batman-begins", movieMatch.Groups[1].Value);

      const string findSeriesPagePattern = "<a href=\\\"/he/subtitle/series/(?<series>.*?)/\\\">";
      const string seriesLine = "<a href=\"/he/subtitle/series/house-md/\">האוס / House M.D.</a>";
      var seriesMatch = Regex.Match(seriesLine, findSeriesPagePattern, RegexOptions.IgnoreCase);
      Assert.AreEqual("house-md", seriesMatch.Groups[1].Value);
    }
    */

    /*
    [TestMethod]
    public void TestSubtitlesParse()
    {
      const string pattern = "subtitles_groups = (?<subs>.*)";
      const string line =
        "subtitles_groups = {\"en\": {\"1.Private_Translated\": {\"hdtv\": {\"3\": {\"created_by\": \"galoren.com\", \"created_on\": \"27.01.2011 ,10:26\", \"credits\": {\"1\": {\"\u05ea\u05e8\u05d2\u05d5\u05dd\": \"grzesiek11\"}}, \"downloaded\": 305, \"hearing_impaired\": 0, \"id\": 71088, \"is_sync\": 0, \"notes\": \"\", \"subtitle_version\": \"House.S07E10.HDTV.XviD-LOL\"}}}}, \"he\": {\"2.Qsubs\": {\"hdtv\": {\"1\": {\"created_by\": \"Qsubs\", \"created_on\": \"28.01.2011 ,18:35\", \"credits\": {\"1\": {\"\u05ea\u05e8\u05d2\u05d5\u05dd\": \"\u05d0\u05dc\u05db\u05e1\u05e0\u05d3\u05e8 \u05e4\u05df \"}, \"2\": {\"\u05e1\u05e0\u05db\u05e8\u05d5\u05df\": \"ZIPC \"}}, \"downloaded\": 2377, \"hearing_impaired\": 0, \"id\": 71173, \"is_sync\": 0, \"notes\": \"\u05e4\u05d5\u05e8\u05de\u05d8 SRT!!!\", \"subtitle_version\": \"House.S07E10.HDTV.XviD-LOL\"}, \"2\": {\"created_by\": \"Qsubs\", \"created_on\": \"28.01.2011 ,18:35\", \"credits\": {\"1\": {\"\u05ea\u05e8\u05d2\u05d5\u05dd\": \"\u05d0\u05dc\u05db\u05e1\u05e0\u05d3\u05e8 \u05e4\u05df \"}, \"2\": {\"\u05e1\u05e0\u05db\u05e8\u05d5\u05df\": \"ZIPC \"}}, \"downloaded\": 1095, \"hearing_impaired\": 0, \"id\": 71173, \"is_sync\": 0, \"notes\": \"\u05e4\u05d5\u05e8\u05de\u05d8 SRT!!!\", \"subtitle_version\": \"House.S07E10.720p.HDTV.X264-DIMENSION\"}}}}}";
      //      const string line = "subtitles_groups = {\\\"en\\\": {\\\"1.Private Translated\\\": {\\\"hdtv\\\": {\\\"3\\\": {\\\"created_by\\\": \\\"galoren.com\\\", \\\"created_on\\\": \\\"27.01.2011 ,10:26\\\", \\\"credits\\\": {\\\"1\\\": {\\\"\u05ea\u05e8\u05d2\u05d5\u05dd\\\": \\\"grzesiek11\\\"}}, \\\"downloaded\\\": 305, \\\"hearing_impaired\\\": 0, \\\"id\\\": 71088, \\\"is_sync\\\": 0, \\\"notes\\\": \\\"\\\", \\\"subtitle_version\\\": \\\"House.S07E10.HDTV.XviD-LOL\\\"}}}}, \\\"he\\\": {\\\"2.Qsubs\\\": {\\\"hdtv\\\": {\\\"1\\\": {\\\"created_by\\\": \\\"Qsubs\\\", \\\"created_on\\\": \\\"28.01.2011 ,18:35\\\", \\\"credits\\\": {\\\"1\\\": {\\\"\u05ea\u05e8\u05d2\u05d5\u05dd\\\": \\\"\u05d0\u05dc\u05db\u05e1\u05e0\u05d3\u05e8 \u05e4\u05df \\\"}, \\\"2\\\": {\\\"\u05e1\u05e0\u05db\u05e8\u05d5\u05df\\\": \\\"ZIPC \\\"}}, \\\"downloaded\\\": 2377, \\\"hearing_impaired\\\": 0, \\\"id\\\": 71173, \\\"is_sync\\\": 0, \\\"notes\\\": \\\"\u05e4\u05d5\u05e8\u05de\u05d8 SRT!!!\\\", \\\"subtitle_version\\\": \\\"House.S07E10.HDTV.XviD-LOL\\\"}, \\\"2\\\": {\\\"created_by\\\": \\\"Qsubs\\\", \\\"created_on\\\": \\\"28.01.2011 ,18:35\\\", \\\"credits\\\": {\\\"1\\\": {\\\"\u05ea\u05e8\u05d2\u05d5\u05dd\\\": \\\"\u05d0\u05dc\u05db\u05e1\u05e0\u05d3\u05e8 \u05e4\u05df \\\"}, \\\"2\\\": {\\\"\u05e1\u05e0\u05db\u05e8\u05d5\u05df\\\": \\\"ZIPC \\\"}}, \\\"downloaded\\\": 1095, \\\"hearing_impaired\\\": 0, \\\"id\\\": 71173, \\\"is_sync\\\": 0, \\\"notes\\\": \\\"\u05e4\u05d5\u05e8\u05de\u05d8 SRT!!!\\\", \\\"subtitle_version\\\": \\\"House.S07E10.720p.HDTV.X264-DIMENSION\\\"}}}}}";
      var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase).Groups[1].Value;
      //      Assert.AreEqual("{\"en\"", match.Substring(0, 5));

      // Dictionary<language, Dictionary<group, Dictionary<quality, Dictionary<subIdx, Dictionary<name, value>>>>> 
      var output = ParseSubtitlesGroups(match);

    }
    */

    /*
    [TestMethod]
    public void TestOtherDownloader()
    {
      var results = new List<Subtitle>();
      var downloader = new SublightDownloader();
      var query = new EpisodeSearchQuery("Heroes", 1, 3);
      //query.LanguageCodes = new string[] { "fin" };// default language is "eng"
      results = downloader.SearchSubtitles(query);              // return English subtitles for "Heroes" season 1 episode 3;
      Console.Write("Testing");
    }
    */

    /*
    [TestMethod]
    public void TestWebHtml404()
    {
      var url = "http://www.subscenter.org/he/subtitle/movie/batbam-begins/";
      var web = new HtmlWeb();
      var htmlDocument = web.Load(url);

      // verify 404
      var is404 = false;
      var images = htmlDocument.DocumentNode.SelectNodes("//img");
      foreach (var node in images)
      {
        var alt = node.GetAttributeValue("alt", string.Empty);
        if (alt.Equals("Error 404"))
        {
          is404 = true;
          break;
        }
      }
      Assert.IsTrue(is404);
    }
    */

    /*
    [TestMethod]
    public void TestWebHtml()
    {
      var url = "http://www.subscenter.org/he/subtitle/movie/batman-begins/";
      var web = new HtmlWeb();
      var htmlDocument = web.Load(url);

      // verify year
      var year = int.Parse(Regex.Match(htmlDocument.DocumentNode.SelectNodes("//h1")[0].ParentNode.InnerText, "\\d+").Value);
      Assert.AreEqual(2005, year);

      // verify title
      var title = htmlDocument.DocumentNode.SelectNodes("//h3")[0].InnerText;
      Assert.AreEqual("Batman Begins", title);
    }
    */
  }

}
