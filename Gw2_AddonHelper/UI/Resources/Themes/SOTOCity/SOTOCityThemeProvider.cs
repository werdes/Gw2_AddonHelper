using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Resources.Themes.SOTOCity
{
    public class SOTOCityThemeProvider : IThemeProvider
    {
        private static Lazy<SOTOCityThemeProvider> _instance = new Lazy<SOTOCityThemeProvider>(() => new SOTOCityThemeProvider());
        public static SOTOCityThemeProvider Instance { get => _instance.Value; }

        public Color GetAccentColor()
        {
            return Colors.Gold;
        }

        public string GetBackgroundImagePath()
        {
            return @"/UI/Resources/Themes/SOTOCity/SOTOCity.png";
        }

        public decimal GetBackgroundOpacity()
        {
            return 0.75M;
        }

        public Stretch GetBackgroundStretch()
        {
            return Stretch.UniformToFill;
        }

        public Color GetWindowTintColor()
        {
            return Colors.Black;
        }
    }
}
