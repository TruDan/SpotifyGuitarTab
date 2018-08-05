using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CefSharp;
using Guitarify.Wpf.Util;
using Guitarify.Wpf.ViewModels.Commands;
using SpotifyAPI.Web.Models;

namespace Guitarify.Wpf.ViewModels
{
    public class TabViewerViewModel : BaseViewModel
    {
        private FavouriteTabEntry _favouriteTabEntry;
        private bool _hasFavouriteTab;
        private string _trackId;
        private string _trackTabUrl;
        private FullTrack _track;

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

        public FullTrack Track
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

                // search
                SearchResults.Clear();
                var results = _guitarTabProvider.Search(_track.Artists[0].Name, _track.Name);

                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }

                OnPropertyChanged(nameof(SearchResults));
            }
        }

        public string TrackId => Track?.Uri;
        public string TrackTabUrl => FavouriteTabEntry?.TabUrl;

        private IGuitarTabDataProvider _guitarTabProvider { get; } = new UltimateGuitarScraper();

        public ObservableCollection<IGuitarTabSearchResult> SearchResults { get; } = new ObservableCollection<IGuitarTabSearchResult>();

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

        public ICommand OpenSearchResult => new ParameterizedDelegateCommand((searchResult) =>
        {
            _browser.Load(((IGuitarTabSearchResult)searchResult).Url);
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
    }
}
