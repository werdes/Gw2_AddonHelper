using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Common.Model;
using SourceChord.FluentWPF;
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
    public class InstallStateToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InstallState state;

            //String representation may be parsed first
            if (value is string)
            {
                if (Enum.TryParse(value.ToString(), out state))
                {
                    value = state;
                }
            }

            if(value is InstallState)
            {
                state = (InstallState)value;
                switch (state)
                {
                    case InstallState.NotInstalled:
                        return Localization.Localization.InstallStateNotInstalled;
                    case InstallState.InstalledDisabled:
                        return Localization.Localization.InstallStateInstalledDisabled;
                    case InstallState.InstalledEnabled:
                        return Localization.Localization.InstallStateInstalledEnabled;
                    case InstallState.Error:
                        return Localization.Localization.InstallStateError;
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
