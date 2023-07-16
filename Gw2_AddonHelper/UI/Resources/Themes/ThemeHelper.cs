using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Resources.Themes
{
    public static class ThemeHelper
    {
        public static IThemeProvider GetThemeProvider(UiTheme theme)
        {
            switch (theme)
            {
                case UiTheme.EODJade: return EODJade.EODJadeThemeProvider.Instance;
                case UiTheme.EODTemple: return EODTemple.EODTempleThemeProvider.Instance;
                case UiTheme.POFCrystal: return POFCrystal.POFCrystalThemeProvider.Instance;
                case UiTheme.SOTOCity: return SOTOCity.SOTOCityThemeProvider.Instance;
                default: return null;
            }
        }

        public static Theme GetTheme(UiTheme theme)
        {
            IThemeProvider themeProvider = GetThemeProvider(theme);
            
            if (themeProvider != null)
            {
                return new Theme()
                {
                    AccentColor = themeProvider.GetAccentColor(),
                    BackgroundOpacity = themeProvider.GetBackgroundOpacity(),
                    BackgroundUri = themeProvider.GetBackgroundImagePath(),
                    BackgroundStretch = themeProvider.GetBackgroundStretch(),
                    WindowTintColor = themeProvider.GetWindowTintColor()
                };
            }
            else
            {
                return new Theme()
                {
                    AccentColor = Colors.White,
                    BackgroundOpacity = 1,
                    BackgroundUri = String.Empty,
                    BackgroundStretch = Stretch.Uniform,
                    WindowTintColor  = Colors.Black
                };
            }
        }
    }
}
