using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Guitarify.Wpf.ViewModels;
using SpotifyAPI;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Guitarify.Wpf.Services
{
    public static class SpotifyService
    {
        private const Scope SpotifyScopes = Scope.UserReadPrivate | Scope.Streaming | Scope.UserModifyPlaybackState | Scope.UserLibraryRead;

        private static SpotifyPlayerViewModel _viewModel;
        public static SpotifyPlayerViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = new SpotifyPlayerViewModel()); }
        }

        public static SpotifyWebAPI Spotify => _spotify;

        private static WebAPIFactory _spotifyApiFactory;
        private static SpotifyWebAPI _spotify;
        private static readonly string _clientId;
        private static SpotifyWatcher _watcher;

        static SpotifyService()
        {
            _clientId = ConfigurationManager.AppSettings["SpotifyAPI.ClientID"];

            _spotifyApiFactory = new WebAPIFactory("http://localhost/", 8000, _clientId, SpotifyScopes, Timeout.InfiniteTimeSpan, new ProxyConfig());

            //_spotify = new SpotifyWebAPI(new ProxyConfig());



            //_spotify.OnPlayStateChange += SpotifyLocalOnOnPlayStateChange;
            //_spotify.OnTrackChange += SpotifyLocalOnOnTrackChange;
            //_spotify.OnTrackTimeChange += SpotifyLocalOnOnTrackTimeChange;
            //_spotify.OnVolumeChange += SpotifyLocalOnOnVolumeChange;

            TryConnect();
        }

        private static async void TryConnect()
        {
            try
            {
                _spotify = await _spotifyApiFactory.GetWebApi(true);
                _watcher = new SpotifyWatcher(_spotify);

                //_watcher.PlayStateChanged += SpotifyLocalOnOnPlayStateChange;
                //_watcher.TrackChanged += SpotifyLocalOnOnTrackChange;
                //_watcher.TrackTimeChanged += SpotifyLocalOnOnTrackTimeChange;
                //_watcher.VolumeChanged += SpotifyLocalOnOnVolumeChange;

                _watcher.Tick += SpotifyLocalOnTick;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error authenticating to Spotify API");
                throw;
            }

            if (_spotify == null)
                return;
        }

        public static void Play() => _spotify.ResumePlayback();
        public static void Pause() => _spotify.PausePlayback();

        public static void Previous() => _spotify.SkipPlaybackToPrevious();
        public static void Next() => _spotify.SkipPlaybackToNext();

        private static void SpotifyLocalOnOnVolumeChange(object sender, VolumeChangeEventArgs e)
        {

        }

        private static void SpotifyLocalOnOnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            _viewModel.DispatchIfNessecary(() => { _viewModel.PlayPosition = e.TrackTime; });
        }

        private static void SpotifyLocalOnOnTrackChange(object sender, TrackChangeEventArgs e)
        {
            _viewModel.DispatchIfNessecary(() => { _viewModel.CurrentTrack = e.NewTrack; });
        }

        private static void SpotifyLocalOnOnPlayStateChange(object sender, PlayStateChangeEventArgs e)
        {
            _viewModel.DispatchIfNessecary(() =>
            {
                _viewModel.UpdateStatus(_watcher.Context);
            });
        }
        private static void SpotifyLocalOnTick(object sender, EventArgs e)
        {
            _viewModel.DispatchIfNessecary(() =>
            {
                _viewModel.UpdateStatus(_watcher.Context);
            });
        }

        class SpotifyWatcher : IDisposable
        {
            public event EventHandler<VolumeChangeEventArgs> VolumeChanged;
            public event EventHandler<TrackTimeChangeEventArgs> TrackTimeChanged;
            public event EventHandler<TrackChangeEventArgs> TrackChanged;
            public event EventHandler<PlayStateChangeEventArgs> PlayStateChanged;

            public event EventHandler Tick;

            private SpotifyWebAPI _api;
            private Timer _timer;

            public PlaybackContext Context => _currentContext;
            private PlaybackContext _currentContext, _previousContext;

            public SpotifyWatcher(SpotifyWebAPI api, int tickInterval = 500)
            {
                _api = api;
                _timer = new Timer(o => DoTick(), null, tickInterval, tickInterval);
            }

            private void DoTick()
            {
                _previousContext = _currentContext;
                try
                {
                    _currentContext = _api.GetPlayback();
                }
                catch (SpotifyWebApiException ex)
                {
                    MessageBox.Show(ex.Message);
                    Thread.Sleep(5000);
                    return;
                }

                try
                {
                    if (_currentContext == null) return;

                    // Check volume change
                    if (CompareContext(c => c.Device?.VolumePercent, out var oldVolume, out var newVolume))
                    {
                        // Volume Changed
                        VolumeChanged?.Invoke(this,
                                              new VolumeChangeEventArgs(oldVolume.GetValueOrDefault(),
                                                                        newVolume.GetValueOrDefault()));
                    }

                    if (CompareContext(c => c.Item, out var oldTrack, out var newTrack))
                    {
                        // Track Changed
                        TrackChanged?.Invoke(this, new TrackChangeEventArgs(oldTrack, newTrack));
                    }

                    if (CompareContext(c => c.IsPlaying))
                    {
                        // Playstate Changed
                        PlayStateChanged?.Invoke(this, new PlayStateChangeEventArgs(_currentContext.IsPlaying));
                    }

                    if (CompareContext(c => c.ProgressMs, out var oldProgressMs, out var newProgressMs))
                    {
                        // Track Time Changed
                        TrackTimeChanged?.Invoke(this,
                                                 new TrackTimeChangeEventArgs(TimeSpan
                                                                                  .FromMilliseconds(newProgressMs)));
                    }

                    Tick?.Invoke(this, new EventArgs());
                }
                catch { }
            }

            private bool CompareContext(Func<PlaybackContext, object> objectFunc)
            {
                return CompareContext(objectFunc, out var oldValue, out var newValue);
            }
            private bool CompareContext<T>(Func<PlaybackContext, T> objectFunc, out T oldValue, out T newValue)
            {
                oldValue = _previousContext != null ? objectFunc(_previousContext) : default(T);
                newValue = _currentContext != null ? objectFunc(_currentContext) : default(T);

                return (!object.Equals(oldValue, newValue));
            }

            public void Dispose()
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
            }
        }
    }

    public class VolumeChangeEventArgs : EventArgs
    {
        public double OldVolume { get; }
        public double NewVolume { get; }

        internal VolumeChangeEventArgs(double oldVolume, double newVolume)
        {
            OldVolume = oldVolume;
            NewVolume = newVolume;
        }
    }

    public class TrackChangeEventArgs : EventArgs
    {
        public FullTrack OldTrack { get; }
        public FullTrack NewTrack { get; }

        internal TrackChangeEventArgs(FullTrack oldTrack, FullTrack newTrack)
        {
            OldTrack = oldTrack;
            NewTrack = newTrack;
        }
    }

    public class PlayStateChangeEventArgs : EventArgs
    {
        public bool Playing { get; }

        internal PlayStateChangeEventArgs(bool playing)
        {
            Playing = playing;
        }
    }

    public class TrackTimeChangeEventArgs : EventArgs
    {
        public TimeSpan TrackTime { get; }

        internal TrackTimeChangeEventArgs(TimeSpan trackTime)
        {
            TrackTime = trackTime;
        }
    }
}
