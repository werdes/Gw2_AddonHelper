using Gw2_AddonHelper.AddonLib.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Gw2_AddonHelper.UI.Converters
{
    public class InstallerActionToDescriptionTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is InstallerActionType)
            {
                InstallerActionType installerAction = (InstallerActionType)value;

                switch (installerAction)
                {
                    case InstallerActionType.Install:
                        return Localization.Localization.InstallerDescriptionAddonInstallText;
                    case InstallerActionType.Enable:
                        return Localization.Localization.InstallerDescriptionAddonEnableText;
                    case InstallerActionType.Disable:
                        return Localization.Localization.InstallerDescriptionAddonDisableText;
                    case InstallerActionType.Remove:
                        return Localization.Localization.InstallerDescriptionAddonRemoveText;
                    case InstallerActionType.Update:
                        return Localization.Localization.InstallerDescriptionAddonUpdateText;
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
