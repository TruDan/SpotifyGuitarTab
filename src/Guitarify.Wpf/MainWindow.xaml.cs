using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using CefSharp;
using Guitarify.Wpf.Handlers;
using Guitarify.Wpf.Services;
using Guitarify.Wpf.Util;
using Guitarify.Wpf.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

namespace Guitarify.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private TabViewerViewModel _viewModel;
        public MainWindow()
        {
            SpotifyService.ViewModel.PropertyChanged += SpotifyViewModelOnPropertyChanged;

            InitializeComponent();

            _viewModel  = new TabViewerViewModel(ChromiumBrowser);
            DataContext = _viewModel;

            ChromiumBrowser.FrameLoadEnd += ChromiumBrowserOnFrameLoadEnd;
            Loaded += OnLoaded;

            SearchResultsList.ItemsSource = _viewModel.SearchResults;
            CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(SearchResultsList.ItemsSource);
            var groupDescription = new PropertyGroupDescription("ArtistName");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void ChromiumBrowserOnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain) return;
            
            e.Browser.MainFrame.ExecuteJavaScriptAsync(Properties.Resources.jQuery_slim);
            e.Browser.MainFrame
                .ExecuteJavaScriptAsync(@"
                    $(document).ready(function(){
                        $('main').children().eq(1).css({
                            'position'  : 'absolute',
                            'top'       : '0',
                            'left'      : '0',
                            'right'     : '0',
                            'z-index'   : '9999',
                            'background': '#000',
                            'width'     : 'auto',
                        });
                    });");

        }

        private void SpotifyViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SpotifyService.ViewModel.CurrentTrack))
            {
                UpdateTrack();
            }
        }

        private void UpdateTrack()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdateTrack);
                return;
            }

            _viewModel.Track = SpotifyService.ViewModel.CurrentTrack;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ChromiumBrowser.RequestHandler = new AdBlockRequestHandler();
            UpdateTrack();
            //ChromiumBrowser.ShowDevTools();
        }

        private void SearchResultsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var currentItem = (e.AddedItems[0]) as IGuitarTabSearchResult;
            if (currentItem == null) return;

            ChromiumBrowser.Load(currentItem.Url);
        }
    }
}
