using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gw2_AddonHelper.UI.Localization
{
    public class LocalizationProvider
    {
        private static ObjectDataProvider _provider;
        public static ObjectDataProvider Provider
        {
            get
            {
                if (_provider == null)
                    _provider = (ObjectDataProvider)App.Current.FindResource("Localization");
                return _provider;
            }
        }

        /// <summary>
        /// Returns a list of available languages
        /// </summary>
        /// <returns></returns>
        public static List<CultureInfo> GetAvailableCultures()
        {
            ResourceManager resourceManager = new ResourceManager(typeof(Localization));
            CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            List<CultureInfo> availableCultures = new List<CultureInfo>();

            availableCultures = allCultures.Where(x => resourceManager.GetResourceSet(x, true, false) != null).ToList();
            return availableCultures;
        }

        /// <summary>
        /// Provides a new instance of a Localization resource for the object data provider
        /// </summary>
        /// <returns></returns>
        public Localization GetInstance()
        {
            return new Localization();
        }

        /// <summary>
        /// Changes the resource culture and refreshes the data provider
        /// </summary>
        /// <param name="culture"></param>
        public static void ChangeCulture(CultureInfo culture)
        {
            Localization.Culture = culture;
            Provider.Refresh();
        }
    }
}
