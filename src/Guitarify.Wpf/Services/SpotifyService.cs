using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Guitarify.Wpf.ViewModels;
using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

namespace Guitarify.Wpf.Services
{
    public static class SpotifyService
    {
        private static SpotifyPlayerViewModel _viewModel;
        public static SpotifyPlayerViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = new SpotifyPlayerViewModel()); }
        }

        public static SpotifyLocalAPI Spotify => _spotifyLocal;

        private static SpotifyLocalAPI _spotifyLocal;

        static SpotifyService()
        {
            _spotifyLocal = new SpotifyLocalAPI(new SpotifyLocalAPIConfig()
            {
                ProxyConfig = new ProxyConfig()
            });

            _spotifyLocal.OnPlayStateChange += SpotifyLocalOnOnPlayStateChange;
            _spotifyLocal.OnTrackChange += SpotifyLocalOnOnTrackChange;
            _spotifyLocal.OnTrackTimeChange += SpotifyLocalOnOnTrackTimeChange;
            _spotifyLocal.OnVolumeChange += SpotifyLocalOnOnVolumeChange;

            TryConnect();
        }

        private static async void TryConnect()
        {
            if (!SpotifyLocalAPI.IsSpotifyInstalled())
            {
                MessageBox.Show(@"Spotify is not installed!");
                return;
            }

            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                if (MessageBox.Show(@"Spotify is not running! Launch now?", @"Spotify", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SpotifyLocalAPI.RunSpotify();
                }

                return;
            }

            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                if (MessageBox.Show(@"Spotify web helper is not running! Launch now?", @"Spotify", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SpotifyLocalAPI.RunSpotifyWebHelper();
                }

                return;
            }

            if (_spotifyLocal.Connect())
            {
                ViewModel.UpdateStatus();
                _spotifyLocal.ListenForEvents = true;
            }
            else
            {
                if (MessageBox.Show(@"Could not connect to local spotify client. Retry?", @"Spotify", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    TryConnect();
                }
            }
        }

        private static void SpotifyLocalOnOnVolumeChange(object sender, VolumeChangeEventArgs e)
        {

        }

        private static void SpotifyLocalOnOnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            _viewModel.DispatchIfNessecary(() => { _viewModel.PlayPosition = TimeSpan.FromSeconds(e.TrackTime); });
        }

        private static void SpotifyLocalOnOnTrackChange(object sender, TrackChangeEventArgs e)
        {
            _viewModel.DispatchIfNessecary(() => { _viewModel.CurrentTrack = e.NewTrack; });
        }

        private static void SpotifyLocalOnOnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            _viewModel.DispatchIfNessecary(() =>
            {
                _viewModel.UpdateStatus();
            });
        }
    }
}
