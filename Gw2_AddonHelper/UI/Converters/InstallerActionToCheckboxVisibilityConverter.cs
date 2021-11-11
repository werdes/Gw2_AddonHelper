using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Common.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Gw2_AddonHelper.UI.Converters
{
    public class InstallerActionToCheckboxVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InstallerActionType)
            {
                InstallerActionType installerAction = (InstallerActionType)value;

                switch (installerAction)
                {
                    case InstallerActionType.Install:
                    case InstallerActionType.Remove:
                    case InstallerActionType.Enable:
                    case InstallerActionType.Disable:
                        return false;
                    case InstallerActionType.Update:
                        return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
