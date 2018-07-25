using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Guitarify.Wpf.Services;
using Guitarify.Wpf.ViewModels.Commands;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

namespace Guitarify.Wpf.ViewModels
{
    public class SpotifyPlayerViewModel : BaseViewModel
    {
        private Track _currentTrack;
        private DelegateCommand _playCommand, _skipCommand, _prevCommand;
        private bool _isPlaying;
        private TimeSpan _playPosition;


        private StatusResponse _spotifyStatus;
        private ImageSource _albumArtImage;
        private TimeSpan _trackLength;

        public Track CurrentTrack
        {
            get => _currentTrack;
            set
            {
                if (Equals(value, _currentTrack)) return;
                _currentTrack = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TrackId));
                OnPropertyChanged(nameof(TrackName));
                OnPropertyChanged(nameof(ArtistName));
                OnPropertyChanged(nameof(AlbumName));

                UpdateAlbumArt();
                TrackLength = TimeSpan.FromSeconds(_currentTrack.Length);
            }
        }

        public string TrackId => CurrentTrack?.TrackResource?.Uri;
        public string TrackName => CurrentTrack?.TrackResource?.Name;

        public string ArtistName => CurrentTrack?.ArtistResource?.Name;
        public string AlbumName => CurrentTrack?.AlbumResource?.Name;

        public ImageSource AlbumArtImage
        {
            get => _albumArtImage;
            set
            {
                if (Equals(value, _albumArtImage)) return;
                _albumArtImage = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan PlayPosition
        {
            get => _playPosition;
            set
            {
                if (value.Equals(_playPosition)) return;
                _playPosition = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan TrackLength
        {
            get => _trackLength;
            set
            {
                if (value.Equals(_trackLength)) return;
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

        public DelegateCommand PlayCommand
        {
            get
            {
                return _playCommand ?? (_playCommand = new DelegateCommand(() =>
                                           {
                                               var spotify = SpotifyService.Spotify;

                                               if (IsPlaying)
                                               {
                                                   spotify.Pause();
                                               }
                                               else
                                               {
                                                   spotify.Play();
                                               }
                                           }, _spotifyStatus.PlayEnabled));
            }
        }
        public DelegateCommand PrevCommand
        {
            get
            {
                return _prevCommand ??
                       (_prevCommand = new DelegateCommand(() => SpotifyService.Spotify.Previous(), _spotifyStatus.PrevEnabled));
            }
        }
        public DelegateCommand SkipCommand
        {
            get
            {
                return _skipCommand ??
                       (_skipCommand = new DelegateCommand(() => SpotifyService.Spotify.Skip(), _spotifyStatus.NextEnabled));
            }
        }

        public void UpdateStatus()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdateStatus);
                return;
            }

            var status = SpotifyService.Spotify.GetStatus();
            
            _spotifyStatus = status;
            PlayPosition = TimeSpan.FromSeconds(status.PlayingPosition);
            IsPlaying = status.Playing;

            PlayCommand.Enabled = status.PlayEnabled;
            PrevCommand.Enabled = status.PrevEnabled;
            SkipCommand.Enabled = status.NextEnabled;

            OnPropertyChanged(nameof(PlayCommand));
            OnPropertyChanged(nameof(PrevCommand));
            OnPropertyChanged(nameof(SkipCommand));

            CurrentTrack = status.Track;
        }

        private async void UpdateAlbumArt()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdateAlbumArt);
                return;
            }

            if (CurrentTrack == null)
            {
                AlbumArtImage = null;
                return;
            }

            var bitmap = await CurrentTrack.GetAlbumArtAsByteArrayAsync(AlbumArtSize.Size160);
            using (MemoryStream memory = new MemoryStream(bitmap))
            {
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                AlbumArtImage = bitmapimage;
            }
        }
    }
}
