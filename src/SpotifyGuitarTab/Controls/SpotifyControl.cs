using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using Track = SpotifyAPI.Local.Models.Track;

namespace SpotifyGuitarTab
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SpotifyControl.Wpf.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SpotifyControl.Wpf.Controls;assembly=SpotifyControl.Wpf.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:SpotifyControl/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_PlayButton",     Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_SkipPrevButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_SkipNextButton", Type = typeof(ButtonBase))]
    public class SpotifyControl : Control, IDisposable
    {
        private const string PlayButtonTemplateName     = "PART_PlayButton";
        private const string SkipPrevButtonTemplateName = "PART_SkipPrevButton";
        private const string SkipNextButtonTemplateName = "PART_SkipNextButton";

        private readonly SpotifyControlDataContext _dataContext;
        private readonly SpotifyLocalAPI _spotifyLocal;

        private ButtonBase _playButton, _skipPrevButton, _skipNextButton;

        public event EventHandler<TrackChangeEventArgs> TrackChanged;

        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register("Track", typeof(Track), typeof(SpotifyControl), new PropertyMetadata(default(Track)));

        public Track Track
        {
            get { return (Track) GetValue(TrackProperty); }
            private set { SetValue(TrackProperty, value); }
        }

        static SpotifyControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpotifyControl), new FrameworkPropertyMetadata(typeof(SpotifyControl)));
        }

        public SpotifyControl()
        {
            _spotifyLocal = new SpotifyLocalAPI(new SpotifyLocalAPIConfig()
            {
                ProxyConfig = new ProxyConfig()
            });

            _spotifyLocal.OnPlayStateChange += SpotifyLocalOnOnPlayStateChange;
            _spotifyLocal.OnTrackChange     += SpotifyLocalOnOnTrackChange;
            _spotifyLocal.OnTrackTimeChange += SpotifyLocalOnOnTrackTimeChange;

            _dataContext = new SpotifyControlDataContext();
            DataContext = _dataContext;
        }
        
        private void SpotifyLocalOnOnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SpotifyLocalOnOnTrackTimeChange(sender, e));
                return;
            }

            _dataContext.UpdatePlayPosition(e.TrackTime);
        }

        private void SpotifyLocalOnOnTrackChange(object sender, TrackChangeEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SpotifyLocalOnOnTrackChange(sender, e));
                return;
            }
            
            Track = e.NewTrack;
            _dataContext.UpdateTrack(e.NewTrack);

            TrackChanged?.Invoke(sender, e);
        }

        private void SpotifyLocalOnOnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SpotifyLocalOnOnPlayStateChange(sender, e));
                return;
            }

            _dataContext.IsPlaying = e.Playing;
        }

        public void Update()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(Update);
                return;
            }

            var status = _spotifyLocal.GetStatus();
            if (status == null) return;

            _dataContext.UpdateDataContext(status);
        }

        private void TryConnect()
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
                Update();
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContext = _dataContext;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_playButton != null)
                _playButton.Click -= PlayButtonOnClick;
            if (_skipPrevButton != null)
                _skipPrevButton.Click -= SkipPrevButtonOnClick;
            if (_skipNextButton != null)
                _skipNextButton.Click -= SkipNextButtonOnClick;

            _playButton     = GetTemplateChild(PlayButtonTemplateName) as ButtonBase;
            _skipPrevButton = GetTemplateChild(SkipPrevButtonTemplateName) as ButtonBase;
            _skipNextButton = GetTemplateChild(SkipNextButtonTemplateName) as ButtonBase;

            _playButton.Click += PlayButtonOnClick;
            _skipPrevButton.Click += SkipPrevButtonOnClick;
            _skipNextButton.Click += SkipNextButtonOnClick;

            DataContext = _dataContext;
        }

        private void SkipPrevButtonOnClick(object sender, RoutedEventArgs e)
        {
            _spotifyLocal.Previous();
        }

        private void SkipNextButtonOnClick(object sender, RoutedEventArgs e)
        {
            _spotifyLocal.Skip();
        }

        private async void PlayButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (_dataContext.IsPlaying)
            {
                await _spotifyLocal.Pause();
            }
            else
            {
                await _spotifyLocal.Play();
            }
        }


        public override void BeginInit()
        {
            base.BeginInit();
            
            TryConnect();
        }
        
        public void Dispose()
        {
            _spotifyLocal?.Dispose();
        }


    }

    public class SpotifyControlDataContext : INotifyPropertyChanged
    {
        #region Private Backing Fields
        private string _trackName = string.Empty;
        private string _artistName = string.Empty;
        private string _albumName = string.Empty;
        private ImageSource _albumArtImage = null;
        private TimeSpan _playPosition = TimeSpan.Zero;
        private TimeSpan _trackLength = TimeSpan.Zero;
        private bool _isPlaying = false;
        private bool _canPlay = false;
        private bool _canSkipPrev = false;
        private bool _canSkipNext = false;
        #endregion

        #region Properties
        public string TrackName
        {
            get => _trackName;
            set
            {
                if (value == _trackName) return;
                _trackName = value;
                OnPropertyChanged();
            }
        }
        public string ArtistName
        {
            get => _artistName;
            set
            {
                if (value == _artistName) return;
                _artistName = value;
                OnPropertyChanged();
            }
        }
        public string AlbumName
        {
            get => _albumName;
            set
            {
                if (value == _albumName) return;
                _albumName = value;
                OnPropertyChanged();
            }
        }
        public ImageSource AlbumArtImage
        {
            get => _albumArtImage;
            set
            {
                if (value == _albumArtImage) return;
                _albumArtImage = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan PlayPosition
        {
            get => _playPosition;
            set
            {
                if (value == _playPosition) return;
                _playPosition = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan TrackLength
        {
            get => _trackLength;
            set
            {
                if (value == _trackLength) return;
                _trackLength = value;
                OnPropertyChanged();
            }
        }
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (value == _isPlaying) return;
                _isPlaying = value;
                OnPropertyChanged();
            }
        }
        public bool CanPlay
        {
            get => _canPlay;
            set
            {
                if (value == _canPlay) return;
                _canPlay = value;
                OnPropertyChanged();
            }
        }
        public bool CanSkipPrev
        {
            get => _canSkipPrev;
            set
            {
                if (value == _canSkipPrev) return;
                _canSkipPrev = value;
                OnPropertyChanged();
            }
        }
        public bool CanSkipNext
        {
            get => _canSkipNext;
            set
            {
                if (value == _canSkipNext) return;
                _canSkipNext = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public SpotifyControlDataContext()
        {
        }

        #region Status Updating
        

        public void UpdateDataContext(StatusResponse status)
        {
            if (status == null) return;

            IsPlaying = status.Playing;
            CanPlay = status.PlayEnabled;
            CanSkipPrev = status.PrevEnabled;
            CanSkipNext = status.NextEnabled;

            if(status.Track != null)
                UpdateTrack(status.Track);

            UpdatePlayPosition(status.PlayingPosition);
        }

        public async void UpdateTrack(Track track)
        {
            AlbumName   = track.AlbumResource.Name;
            ArtistName  = track.ArtistResource.Name;
            TrackName   = track.TrackResource.Name;
            TrackLength = TimeSpan.FromSeconds(track.Length);

            UpdateAlbumArt(track);
        }

        public void UpdatePlayPosition(double playingPosition)
        {
            PlayPosition = TimeSpan.FromSeconds(playingPosition);
        }

        public async void UpdateAlbumArt(Track track)
        {
            var bitmap = await track.GetAlbumArtAsByteArrayAsync(AlbumArtSize.Size160);
            using (MemoryStream memory = new MemoryStream(bitmap))
            {
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption  = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                AlbumArtImage = bitmapimage;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
