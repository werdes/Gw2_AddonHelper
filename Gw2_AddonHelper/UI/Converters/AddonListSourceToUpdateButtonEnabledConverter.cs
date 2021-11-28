using Gw2_AddonHelper.AddonLib.Model.GameState;
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
    public class AddonListSourceToUpdateButtonEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is AddonListSource &&
                values[1] is IEnumerable<AddonContainer>)
            {
                AddonListSource addonListSource = (AddonListSource)values[0];
                IEnumerable<AddonContainer> addonContainers = (IEnumerable<AddonContainer>)values[1];

                if (addonListSource == AddonListSource.GitHub)
                {
                    return true;
                }
                else if (addonListSource == AddonListSource.RepositoryMirror)
                {
                    return addonContainers.Any(x => x.QuickUpdateAvailable);                    
                }
            }

            return Localization.Localization.ButtonCheckUpdatesText;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
