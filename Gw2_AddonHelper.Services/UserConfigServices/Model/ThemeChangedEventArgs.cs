using Gw2_AddonHelper.Common.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.UserConfigServices.Model
{
    public class ThemeChangedEventArgs : EventArgs
    {
        private UiTheme _theme;

        public UiTheme Theme
        {
            get => _theme;
            set => _theme = value;
        }

        public ThemeChangedEventArgs(UiTheme theme)
            => _theme = theme;
    }
}
