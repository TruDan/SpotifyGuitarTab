using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace Guitarify.Wpf.Converters
{
    public class BooleanToFavouriteIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolVal = (bool) value;
            return boolVal ? PackIconMaterialKind.Star : PackIconMaterialKind.StarOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconVal = (PackIconMaterialKind)value;
            return iconVal == PackIconMaterialKind.Star;
        }
    }
}
