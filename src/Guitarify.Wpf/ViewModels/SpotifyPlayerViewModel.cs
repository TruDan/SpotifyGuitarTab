using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Guitarify.Wpf.Properties;
using Guitarify.Wpf.Services;
using Guitarify.Wpf.ViewModels.Commands;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Web.Models;

namespace Guitarify.Wpf.ViewModels
{
    public class SpotifyPlayerViewModel : BaseViewModel
    {
        private FullTrack _currentTrack;
        private DelegateCommand _playCommand, _skipCommand, _prevCommand;
        private bool _isPlaying;
        private TimeSpan _playPosition;


        private PlaybackContext _spotifyStatus;
        private ImageSource _albumArtImage;
        private TimeSpan _trackLength;

        public FullTrack CurrentTrack
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
                TrackLength = TimeSpan.FromMilliseconds(_currentTrack.DurationMs);
            }
        }

        public string TrackId => CurrentTrack?.Id;
        public string TrackName => CurrentTrack?.Name;

        public string ArtistName => string.Join(", ", CurrentTrack?.Artists?.Select(a => a.Name) ?? new List<string>());
        public string AlbumName => CurrentTrack?.Album?.Name;

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
                                                   //spotify.Pause();
                                               }
                                               else
                                               {
                                                   //spotify.Play();
                                               }
                                           }, _spotifyStatus != null && _spotifyStatus.Device.IsActive && !_spotifyStatus.Device.IsRestricted));
            }
        }
        public DelegateCommand PrevCommand
        {
            get
            {
                return _prevCommand ??
                       (_prevCommand = new DelegateCommand(() => { }//SpotifyService.Spotify.Previous()
                                                         , _spotifyStatus != null && _spotifyStatus.Device.IsActive && !_spotifyStatus.Device.IsRestricted));
            }
        }
        public DelegateCommand SkipCommand
        {
            get
            {
                return _skipCommand ??
                       (_skipCommand = new DelegateCommand(() => { }//SpotifyService.Spotify.Skip()
                                                         , _spotifyStatus != null && _spotifyStatus.Device.IsActive && !_spotifyStatus.Device.IsRestricted));
            }
        }

        public void UpdateStatus(PlaybackContext status)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateStatus(status));
                return;
            }

            _spotifyStatus = status;
            PlayPosition = TimeSpan.FromMilliseconds(status.ProgressMs);
            IsPlaying = status.IsPlaying;

            PlayCommand.Enabled = status.Device.IsActive && !status.Device.IsRestricted;
            PrevCommand.Enabled = status.Device.IsActive && !status.Device.IsRestricted;
            SkipCommand.Enabled = status.Device.IsActive && !status.Device.IsRestricted;

            OnPropertyChanged(nameof(PlayCommand));
            OnPropertyChanged(nameof(PrevCommand));
            OnPropertyChanged(nameof(SkipCommand));

            CurrentTrack = status.Item;
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

            var image = CurrentTrack.Album.Images.FirstOrDefault();
            if (image != null)
            {

                var bitmap = await new WebClient().DownloadDataTaskAsync(image?.Url);
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
            else
            {
                AlbumArtImage = Bitmap2BitmapImage(Resources.NoAlbumArt);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval;

            try
            {
                retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                                                                            hBitmap,
                                                                            IntPtr.Zero,
                                                                            Int32Rect.Empty,
                                                                            BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }
    }
}
