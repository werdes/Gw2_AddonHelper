using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Resources.Themes.EODJade
{
    internal class EODJadeThemeProvider : IThemeProvider
    {
        private static Lazy<EODJadeThemeProvider> _instance = new Lazy<EODJadeThemeProvider>(() => new EODJadeThemeProvider());
        public static EODJadeThemeProvider Instance { get => _instance.Value; }

        public Color GetAccentColor()
        {
            return Colors.ForestGreen;
        }

        public string GetBackgroundImagePath()
        {
            return @"/UI/Resources/Themes/EODJade/EODJade.png";
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
