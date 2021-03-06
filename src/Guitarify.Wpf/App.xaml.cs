﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;

namespace Guitarify.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                         "Guitarify\\Cache"),
                PersistSessionCookies = true,
                RemoteDebuggingPort = 8088
            };

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
    }
}
