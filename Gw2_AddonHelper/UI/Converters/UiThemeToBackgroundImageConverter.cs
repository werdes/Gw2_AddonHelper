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

namespace Gw2_AddonHelper.UI.Converters
{
    public class UiThemeToBackgroundImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is UiTheme)
            {
                UiTheme uiTheme = (UiTheme)value;
                Theme theme = ThemeHelper.GetTheme(uiTheme);
                return theme.BackgroundUri;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
