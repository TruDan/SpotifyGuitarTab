using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Web;

namespace SpotifyGuitarTab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SpotifyWebAPI Spotify;
        private SpotifyLocalAPI SpotifyLocal;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


        }

        private void InitSpotify()
        {
            Spotify = new SpotifyWebAPI();
            Spotify.UseAuth = true;
            Spotify.UseAutoRetry = true;

            SpotifyLocal = new SpotifyLocalAPI(new SpotifyLocalAPIConfig()
            {
                ProxyConfig = new ProxyConfig()
            });

            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                
            }

            SpotifyLocal.Connect();

            SpotifyLocal.OnTrackChange += SpotifyLocalOnOnTrackChange;
            SpotifyLocal.ListenForEvents = true;
        }

        private void SpotifyLocalOnOnTrackChange(object sender, TrackChangeEventArgs e)
        {
            WebBrowser.SearchGuitarTab(e.NewTrack.TrackResource.Name);
        }
    }
}
