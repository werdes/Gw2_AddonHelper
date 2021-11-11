using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Common.Model;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Converters
{
    public class InstallStateToBatchEnableButtonVisibilityConverter : IValueConverter
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
                    case InstallState.Error:
                    case InstallState.NotInstalled:
                    case InstallState.InstalledEnabled:
                        return Visibility.Collapsed;
                    case InstallState.InstalledDisabled:
                        return Visibility.Visible;
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
