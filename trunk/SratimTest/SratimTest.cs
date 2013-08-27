using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sratim;
using SubtitleDownloader.Core;

namespace SratimTest
{
    [TestClass]
    public class SratimTest
    {
        [TestMethod]
        public void TestExactMovieSearch()
        {
            var downloader = new SratimDownloader();
            var query = new SearchQuery("batman begins") {Year = 2005, LanguageCodes = new[] {"heb"}};
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
        public void TestExactMovieSearch2()
        {
            var downloader = new SratimDownloader();
            var query = new SearchQuery("intouchables") {Year = 2012, LanguageCodes = new[] {"heb"}};
            var results = downloader.SearchSubtitles(query);

            // make sure there are resuts
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);

            // check first result
            var subtitleFiles = downloader.SaveSubtitle(results[3]);
            Assert.AreNotEqual(null, subtitleFiles);
            Assert.AreNotEqual(0, subtitleFiles.Count);
        }

        [TestMethod]
        public void TestRegularExpression()
        {
            const string strRegex =
                @"<div style=\""[^>]*?>\s*?<a href=\"".*view.php\?id=(?<movie_id>\d+)&amp;q=(?<movie_query>[^\""]*)\""\s*?title=\""(?<movie_hebrew>[^(]*)\((?<movie_year>[^\\)]*)\)\""\>";
            const RegexOptions myRegexOptions = RegexOptions.None;
            var myRegex = new Regex(strRegex, myRegexOptions);
            const string strTargetString =
                @"<div style=""vertical-align:top;padding-left:14px;padding-bottom:12px;""><a href=""view.php?id=11813&amp;q=batman+begins"" title=""באטמן מתחיל (2005)"">";

            Assert.AreNotEqual(0, myRegex.Matches(strTargetString).Count);
        }


        [TestMethod]
        public void TestConfiguration()
        {
//            var conf = SratimDownloaderConfiguration.GetInstance();
//            var sratimCookieContainer = conf.GetSratimCookieContainer();
//            Assert.AreEqual(3, sratimCookieContainer.Count);
        }
    }
}