using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Guitarify.Wpf.ViewModels;
using JetBrains.Annotations;
using SpotifyAPI;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using Track = SpotifyAPI.Local.Models.Track;

namespace Guitarify.Wpf.Controls
{
    [TemplatePart(Name = "PART_PlayButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_SkipPrevButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_SkipNextButton", Type = typeof(ButtonBase))]
    public class SpotifyPlayer : Control
    {
        private const string PlayButtonTemplateName = "PART_PlayButton";
        private const string SkipPrevButtonTemplateName = "PART_SkipPrevButton";
        private const string SkipNextButtonTemplateName = "PART_SkipNextButton";

        private readonly SpotifyPlayerViewModel _dataContext;
        
        static SpotifyPlayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpotifyPlayer), new FrameworkPropertyMetadata(typeof(SpotifyPlayer)));
        }

        public SpotifyPlayer()
        {
            _dataContext = new SpotifyPlayerViewModel();
            DataContext = _dataContext;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContext = _dataContext;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DataContext = _dataContext;
        }

        public override void BeginInit()
        {
            base.BeginInit();
        }
    }
}
