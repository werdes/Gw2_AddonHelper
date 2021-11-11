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
    public class InstallerActionToHeaderTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is InstallerActionType)
            {
                InstallerActionType installerAction = (InstallerActionType)value;

                switch (installerAction)
                {
                    case InstallerActionType.Install:
                        return Localization.Localization.InstallerHeaderAddonInstallText;
                    case InstallerActionType.Enable:
                        return Localization.Localization.InstallerHeaderAddonEnableText;
                    case InstallerActionType.Disable:
                        return Localization.Localization.InstallerHeaderAddonDisableText;
                    case InstallerActionType.Remove:
                        return Localization.Localization.InstallerHeaderAddonRemoveText;
                    case InstallerActionType.Update:
                        return Localization.Localization.InstallerHeaderAddonUpdateText;
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
