using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices
{
    public class LocalFileAddonSourceService : BaseAddonSourceService, IAddonSourceService
    {
        private const int HIERARCHY = 0;
        protected IUserConfigService _userConfigService;

        protected AddonListContainer _addonListContainer;
        protected VersionContainer _versionContainer;

        public LocalFileAddonSourceService()
        {
            _log = Lib.ServiceProvider.GetService<ILogger<LocalFileAddonSourceService>>();
            _userConfigService = Lib.ServiceProvider.GetService<IUserConfigService>();
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
        }

        /// <summary>
        /// Loads the service information
        /// </summary>
        /// <returns></returns>
        protected async Task InitAddonsFromService()
        {
            string addonFile = _config.GetValue<string>("addonSourceServices:localFile:addonsPath");

            try
            {
                string json = await File.ReadAllTextAsync(addonFile, Encoding.UTF8);
                AddonListContainer tempList = JsonConvert.DeserializeObject<AddonListContainer>(json);

                _addonListContainer = tempList;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading addon list from [{addonFile}] failed");
            }
        }

        /// <summary>
        /// Loads the service information
        /// </summary>
        /// <returns></returns>
        protected async Task InitVersionsFromService()
        {
            string versionsFile = _config.GetValue<string>("addonSourceServices:localFile:versionsPath");

            try
            {
                string json = await File.ReadAllTextAsync(versionsFile, Encoding.UTF8);
                VersionContainer tempList = JsonConvert.DeserializeObject<VersionContainer>(json);

                _versionContainer = tempList;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading addon list from [{versionsFile}] failed");
            }
        }

        /// <summary>
        /// Returns the list of addons from the source
        /// </summary>
        /// <returns></returns>
        public async Task<List<Addon>> GetAddonsAsync()
        {
            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
            }

            return _addonListContainer?.Addons;
        }

        /// <summary>
        /// Returns the given hierarchy
        /// </summary>
        /// <returns></returns>
        public virtual int GetHierarchy()
        {
            return HIERARCHY;
        }

        /// <summary>
        /// Returns the timestamp of the addon source (here last crawl time)
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetListTimestamp()
        {
            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
            }

            return _addonListContainer?.CrawlTime ?? DateTime.MinValue;
        }

        public async Task<string> GetVersion()
        {
            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
            }
            return _addonListContainer?.RepositoryVersion;
        }

        /// <summary>
        /// Returns the version of the addon source
        /// </summary>
        /// <returns></returns>
        public async Task<VersionContainer> GetVersions()
        {
            if (_versionContainer == null)
            {
                await InitVersionsFromService();
            }

            return _versionContainer;
        }

        /// <summary>
        /// Tells wether the source is available
        /// here: - local files exist
        ///       - local files are not timed out (configurable)
        ///       - local files contain addons
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> IsAvailable()
        {
            TimeSpan maxAge = _config.GetValue<TimeSpan>("addonSourceServices:localFile:maxAge");

            string addonsFile = _config.GetValue<string>("addonSourceServices:localFile:addonsPath");
            string versionsFile = _config.GetValue<string>("addonSourceServices:localFile:versionsPath");
            FileInfo fileInfoAddons = null;
            FileInfo fileInfoVersions = null;

            if (File.Exists(addonsFile) &&
                File.Exists(versionsFile))
            {

                fileInfoAddons = new FileInfo(addonsFile);
                fileInfoVersions = new FileInfo(versionsFile);

                if (_addonListContainer == null)
                {
                    await InitAddonsFromService();
                }
                if (_versionContainer == null)
                {
                    await InitVersionsFromService();
                }
            }

            bool sourceDependentChecks = true;
            if (_addonListContainer != null)
            {
                switch (_addonListContainer.Source)
                {
                    case AddonListSource.Undefined:
                        sourceDependentChecks = false;
                        break;
                    case AddonListSource.GitHub:
                        sourceDependentChecks = GithubRatelimitService.Instance.CanCall();
                        break;
                }
            }


            return fileInfoAddons != null &&
                   fileInfoVersions != null &&
                   fileInfoAddons.LastWriteTime >= DateTime.Now.Subtract(maxAge) &&
                   fileInfoAddons.LastWriteTime >= DateTime.Now.Subtract(maxAge) &&
                   _addonListContainer != null &&
                   _addonListContainer.Addons != null &&
                   _addonListContainer.Addons.Count > 0 &&
                   _versionContainer != null &&
                   _versionContainer.Versions != null &&
                   sourceDependentChecks;

        }

        public async Task<AddonListSource> GetSource()
        {
            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
            }
            return _addonListContainer.Source;
        }
    }
}
