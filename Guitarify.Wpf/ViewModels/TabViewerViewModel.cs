using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CefSharp;
using Guitarify.Wpf.Util;
using Guitarify.Wpf.ViewModels.Commands;
using SpotifyAPI.Local.Models;

namespace Guitarify.Wpf.ViewModels
{
    public class TabViewerViewModel : BaseViewModel
    {
        private FavouriteTabEntry _favouriteTabEntry;
        private bool _hasFavouriteTab;
        private string _trackId;
        private string _trackTabUrl;
        private Track _track;

        public bool HasFavouriteTab => FavouriteTabEntry != null;
        public FavouriteTabEntry FavouriteTabEntry
        {
            get { return _favouriteTabEntry; }
            set
            {
                _favouriteTabEntry = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFavouriteTab));
                OnPropertyChanged(nameof(TrackTabUrl));
            }
        }

        public Track Track
        {
            get => _track;
            set
            {
                if (Equals(value, _track)) return;
                _track = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(TrackId));

                if (FavouriteTabStore.TryGetEntry(TrackId, out var entry))
                {
                    FavouriteTabEntry = entry;
                }
                else
                {
                    FavouriteTabEntry = null;
                }
            }
        }

        public string TrackId => Track?.TrackResource.Uri;
        public string TrackTabUrl => FavouriteTabEntry?.TabUrl ?? GetTrackUrl(Track);

        public ICommand ToggleFavouriteTabCommand => new ParameterizedDelegateCommand((currentBrowserAddress) =>
        {

            if (FavouriteTabStore.TryGetEntry(TrackId, out var entry))
            {
                FavouriteTabStore.RemoveEntry(TrackId);
                FavouriteTabEntry = null;
            }
            else
            {
                FavouriteTabEntry = FavouriteTabStore.AddEntry(TrackId, currentBrowserAddress.ToString());
            }
        });

        public ICommand OpenDevToolsCommand => new DelegateCommand(() =>
        {
            _browser.ShowDevTools();
        });

        private IWebBrowser _browser;

        public TabViewerViewModel(IWebBrowser browser)
        {
            _browser = browser;
        }


        private static Regex SearchSanitizeRegex = new Regex(@"(\s*(?:(?:(?:\(|\[).+(?:\)|\]))|(?:-.+)))",
                                                             RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static string SearchUrl   = "https://www.ultimate-guitar.com/search.php?band_name=%artist%&song_name=%track%";

        private string GetTrackUrl(Track track)
        {
            if (track == null) return null;

            var artistName = WebUtility.UrlEncode(SanitiseSearchString(track.ArtistResource.Name));
            var trackName  = WebUtility.UrlEncode(SanitiseSearchString(track.TrackResource.Name));
            
            var url = SearchUrl.Replace("%artist%", artistName).Replace("%track%", trackName);
            return url;
        }
        private string SanitiseSearchString(string searchQuery)
        {
            searchQuery = SearchSanitizeRegex.Replace(searchQuery, "");
            return searchQuery.Trim();
        }
    }
}
