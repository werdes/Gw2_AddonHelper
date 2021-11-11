using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Common.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gw2_AddonHelper.UI.Converters
{
    public class InstallProgressToDescriptionTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is InstallProgress)
            {
                InstallProgress installProgress = (InstallProgress)value;

                switch (installProgress)
                {
                    case InstallProgress.Waiting:
                        return Localization.Localization.InstallProgressWaitingText;
                    case InstallProgress.InProgress:
                        return Localization.Localization.InstallProgressInProgressText;
                    case InstallProgress.Completed:
                        return Localization.Localization.InstallProgressCompletedText;
                    case InstallProgress.Error:
                        return Localization.Localization.InstallProgressErrorText;
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
