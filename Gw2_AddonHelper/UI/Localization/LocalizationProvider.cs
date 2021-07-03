using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public const string INVARIANT_CULTURE_CODE = "ivl";
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
        public static List<CultureInfo> GetAvailableCultures(bool includeInvariantCulture)
        {
            ResourceManager resourceManager = new ResourceManager(typeof(Localization));
            CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            List<CultureInfo> availableCultures = new List<CultureInfo>();

            availableCultures = allCultures.Where(x => resourceManager.GetResourceSet(x, true, false) != null).ToList();

            if(!includeInvariantCulture)
            {
                availableCultures.RemoveAll(x => x.ThreeLetterISOLanguageName == INVARIANT_CULTURE_CODE);
            }

            return availableCultures;
        }

        /// <summary>
        /// Returns a dictionary of a resources' representation in all available cultures
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetLocalized(string name, bool includeInvariantCulture)
        {
            Dictionary<string, string> languageVersions = new Dictionary<string, string>();
            ResourceManager resourceManager = new ResourceManager(typeof(Localization));

            List<CultureInfo> cultures = GetAvailableCultures(includeInvariantCulture);
            foreach (CultureInfo cultureInfo in cultures)
            {
                string cultureVersion = resourceManager.GetString(name, cultureInfo);
                languageVersions.Add(cultureInfo.ThreeLetterISOLanguageName, cultureVersion);
            }

            return languageVersions;
        }

        /// <summary>
        /// Provides a new instance of a Localization resource for the object data provider
        /// </summary>
        /// <returns></returns>S
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
