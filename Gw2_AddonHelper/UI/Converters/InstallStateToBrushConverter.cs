using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Common.Model;
using SourceChord.FluentWPF;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Converters
{
    public class InstallStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is InstallState)
            {
                InstallState state = (InstallState)value;
                if(state == InstallState.InstalledEnabled)
                {
                    return Brushes.LimeGreen;
                }
                if(state == InstallState.InstalledDisabled)
                {
                    return Brushes.Yellow;
                }
            }

            return AccentColors.ImmersiveSystemAccentDark3Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
