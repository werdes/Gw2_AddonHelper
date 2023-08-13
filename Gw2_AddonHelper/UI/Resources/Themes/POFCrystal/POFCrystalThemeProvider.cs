using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Resources.Themes.POFCrystal
{
    internal class POFCrystalThemeProvider : IThemeProvider

    {
        private static Lazy<POFCrystalThemeProvider> _instance = new Lazy<POFCrystalThemeProvider>(() => new POFCrystalThemeProvider());
        public static POFCrystalThemeProvider Instance { get => _instance.Value; }

        public Color GetAccentColor()
        {
            return Color.FromRgb(180, 36, 132) * 1.3F;
        }

        public string GetBackgroundImagePath()
        {
            return @"/UI/Resources/Themes/POFCrystal/POFCrystal.png";
        }

        public decimal GetBackgroundOpacity()
        {
            return 0.5M;
        }

        public Stretch GetBackgroundStretch()
        {
            return Stretch.Uniform;
        }

        public Color GetWindowTintColor()
        {
            return Colors.Black;
        }
    }
}
