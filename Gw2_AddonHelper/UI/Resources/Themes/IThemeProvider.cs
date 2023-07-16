using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Resources.Themes
{
    public interface IThemeProvider
    {
        string GetBackgroundImagePath();
        decimal GetBackgroundOpacity();
        Color GetAccentColor();
        Color GetWindowTintColor();
        Stretch GetBackgroundStretch();
    }
}
