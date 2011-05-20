using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SubsCenterOrg
{
  public sealed class SubsCenterOrgDownloaderConfiguration
  {
    private static volatile SubsCenterOrgDownloaderConfiguration _instance;
    private static readonly object SyncObject = new Object();

    private const string SettingsFilename = "SubtitleDownloaders\\SubsCenterOrg.xml";
    private readonly SortedDictionary<string, string> _seriesNameOverride;

    private SubsCenterOrgDownloaderConfiguration()
    {
      _seriesNameOverride = new SortedDictionary<string, string>();
      
      try
      {
        var xml = new XmlDocument();
        xml.Load(SettingsFilename);

        var seriesNames = xml.SelectNodes("/settings/series");

        if (seriesNames == null)
        {
          return;
        }
        foreach (XmlElement e in seriesNames)
        {
          var key = e.GetAttribute("query").ToLower();
          var value = e.InnerText.ToLower();
          if (!_seriesNameOverride.ContainsKey(key))
          {
            _seriesNameOverride.Add(key, value); 
          }
        }
      }
      catch(FileNotFoundException)
      {
        // no file --> blank list
      }
      catch (XmlException)
      {
        // invalid file --> blank list
      }
    }

    public static SubsCenterOrgDownloaderConfiguration Instance
    {
      get 
      {
         if (_instance == null) 
         {
           lock (SyncObject) 
            {
               if (_instance == null)
                 _instance = new SubsCenterOrgDownloaderConfiguration();
            }
         }
         return _instance;
      }
    }
    
    public string OverrideTitleFromConfiguration(string queryTitle)
    {
      string value;
      _seriesNameOverride.TryGetValue(queryTitle.ToLower(), out value);
      return value ?? queryTitle;
    }
  }
}