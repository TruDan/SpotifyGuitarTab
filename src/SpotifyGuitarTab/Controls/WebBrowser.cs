using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp.Wpf;

namespace SpotifyGuitarTab
{
    public class WebBrowser : UserControl, IDisposable
    {
        public ChromiumWebBrowser Chromium;
        public bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());


        public string HomeUrl = "https://www.ultimate-guitar.com/";
        public string SearchUrl = "https://www.ultimate-guitar.com/search.php?search_type=title&value={%query%}";

        public WebBrowser()
        {
            if (!IsDesignMode) BrowserInit();
        }

        private void BrowserInit()
        {
            Content = Chromium = new ChromiumWebBrowser();
            Chromium.Address = HomeUrl;
        }

        public void SearchGuitarTab(string trackName)
        {
            var url = SearchUrl.Replace("{%query%}", WebUtility.UrlEncode(trackName));

            Chromium.Address = url;
        }

        public void Dispose()
        {
            Chromium?.Dispose();
        }
    }
}
