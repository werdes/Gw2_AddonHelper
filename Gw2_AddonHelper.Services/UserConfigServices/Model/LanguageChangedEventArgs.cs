using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.UserConfigServices.Model
{
    public class LanguageChangedEventArgs : EventArgs
    {
        private CultureInfo _culture;

        public CultureInfo Culture
        {
            get => _culture;
            set => _culture = value;
        }

        public LanguageChangedEventArgs(CultureInfo cultureInfo)
            => _culture = cultureInfo;
    }
}
