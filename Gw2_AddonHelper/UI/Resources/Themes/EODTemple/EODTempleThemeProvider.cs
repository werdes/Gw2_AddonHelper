using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Resources.Themes.EODTemple
{
    internal class EODTempleThemeProvider : IThemeProvider
    {
        private static Lazy<EODTempleThemeProvider> _instance = new Lazy<EODTempleThemeProvider>(() => new EODTempleThemeProvider());
        public static EODTempleThemeProvider Instance { get => _instance.Value; }

        public Color GetAccentColor()
        {
            return Colors.SkyBlue;
        }

        public string GetBackgroundImagePath()
        {
            return @"/UI/Resources/Themes/EODTemple/EODTemple.png";
        }

        public decimal GetBackgroundOpacity()
        {
            return 0.5M;
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
