using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Model.UI;
using Gw2_AddonHelper.UI.Resources.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Converters
{
    internal class UiThemeToWindowTintColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UiTheme)
            {
                UiTheme uiTheme = (UiTheme)value;
                Theme theme = ThemeHelper.GetTheme(uiTheme);
                return new SolidColorBrush(theme.WindowTintColor);
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
