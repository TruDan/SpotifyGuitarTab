using System;
using System.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;

namespace Guitarify.Wpf.Util
{
    public class UltimateGuitarScraper : IGuitarTabDataProvider
    {
        private static Regex SearchSanitizeRegex = new Regex(@"(\s*(?:(?:(?:\(|\[).+(?:\)|\]))|(?:-.+)))",
                                                             RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static string SearchUrl = "https://www.ultimate-guitar.com/search.php?band_name=%artist%&song_name=%track%";

        private static string SearchScrapeXPath = @"//script[text()[contains(., 'window.UGAPP.store.page')]]/text()";

        public IGuitarTabSearchResult[] Search(string artistName, string trackName)
        {
            var url = GetSearchUrl(artistName, trackName);

            var web = new HtmlWeb();
            var doc = web.Load(url);

            var list = doc.DocumentNode.SelectSingleNode(SearchScrapeXPath)
                          .InnerText
                          .Replace("window.UGAPP.store.page =", "")
                          .Trim()
                          .TrimEnd(';');

            Console.WriteLine(list);

            var json = JsonConvert.DeserializeObject<UGAPPStorePage>(list).Data;
            Console.WriteLine(JsonConvert.SerializeObject(json));
            return json.Results.Select(r => (IGuitarTabSearchResult)new UGResultEntry(r)).ToArray();
        }

        private string GetSearchUrl(string artistName, string trackName)
        {
            artistName = WebUtility.UrlEncode(SanitiseSearchString(artistName));
            trackName  = WebUtility.UrlEncode(SanitiseSearchString(trackName));

            var url = SearchUrl.Replace("%artist%", artistName).Replace("%track%", trackName);
            return url;
        }
        private string SanitiseSearchString(string searchQuery)
        {
            searchQuery = SearchSanitizeRegex.Replace(searchQuery, "");
            return searchQuery.Trim();
        }

        public class UGResultEntry : IGuitarTabSearchResult
        {
            public string ArtistName { get; }
            public string TrackName { get; }
            public string Version { get; }

            public double Rating { get; }

            public int RatingCount { get; }
            public string Type { get; }

            public string Url { get; }

            internal UGResultEntry(UGSearchData.ResultEntry entry)
            {
                ArtistName = entry.ArtistName;
                TrackName = entry.SongName;
                Rating = entry.Rating / 5d;
                RatingCount = entry.Votes;
                Url = entry.TabUrl;
                Version = entry.Version > 1 ? $"(ver {entry.Version})" : "";
                Type = entry.Type;
            } 
        }
    }

    public struct UGAPPStorePage
    {
        public UGSearchData Data { get; set; }
    }

    public struct UGSearchData
    {

        public PaginationEntry Pagination { get; set; }
        public ResultEntry[] Results { get; set; }

        public struct PaginationEntry
        {
            public int Total { get; set; }
            public int Current { get; set; }
        }

        public struct ResultEntry
        {
            public int Id { get; set; }
            [JsonProperty("song_name")]
            public string SongName { get; set; }
            [JsonProperty("artist_name")]
            public string ArtistName { get; set; }
            public string Type { get; set; }
            [JsonProperty("type_name")]
            public string TypeName { get; set; }
            public string Date { get; set; }
            public double Rating { get; set; }
            [JsonProperty("tab_url")]
            public string TabUrl { get; set; }
            public int Votes { get; set; }
            public int Version { get; set; }
        }
    }
}
