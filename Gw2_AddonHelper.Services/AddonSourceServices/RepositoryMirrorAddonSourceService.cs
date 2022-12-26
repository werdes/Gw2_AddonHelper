using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices
{
    public class RepositoryMirrorAddonSourceService : BaseAddonSourceService, IAddonSourceService
    {
        private const int HIERARCHY = 1;

        private IUserConfigService _userConfigService;

        private AddonListContainer _addonListContainer;
        private VersionContainer _versionContainer;
        private WebClient _webClient;

        public RepositoryMirrorAddonSourceService()
        {
            _log = Lib.ServiceProvider.GetService<ILogger<RepositoryMirrorAddonSourceService>>();
            _userConfigService = Lib.ServiceProvider.GetService<IUserConfigService>();
            _config = Lib.ServiceProvider.GetService<IConfiguration>();

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
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
        /// Loads the service information
        /// </summary>
        /// <returns></returns>
        private async Task InitAddonsFromService()
        {
            try
            {
                Uri updateUri = new Uri(_config.GetValue<string>("addonSourceServices:repositoryMirror:addonsUrl"));

                string json = await _webClient.DownloadStringTaskAsync(updateUri);

                _addonListContainer = JsonConvert.DeserializeObject<AddonListContainer>(json);
                _addonListContainer.Source = AddonListSource.RepositoryMirror;

                foreach(Addon addon in _addonListContainer.Addons)
                {
                    addon.LoaderKey = GetLoaderKey(addon);
                }

                await StoreAddons();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Retrieving addons from repo mirror failed");
            }
        }

        /// <summary>
        /// Loads the service versions information
        /// </summary>
        /// <returns></returns>
        private async Task InitVersionsFromService()
        {
            try
            {
                Uri updateUri = new Uri(_config.GetValue<string>("addonSourceServices:repositoryMirror:versionsUrl"));

                string json = await _webClient.DownloadStringTaskAsync(updateUri);

                _versionContainer = JsonConvert.DeserializeObject<VersionContainer>(json);
                _versionContainer.Source = AddonListSource.RepositoryMirror;

                await StoreVersions();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Retrieving addons from repo mirror failed");
            }
        }

        /// <summary>
        /// Returns the given hierarchy
        /// </summary>
        /// <returns></returns>
        public int GetHierarchy()
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

        /// <summary>
        /// Returns the version of the addon source
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVersion()
        {
            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
            }
            return _addonListContainer?.RepositoryVersion;
        }

        /// <summary>
        /// Tells wether the source is available
        /// here: - source can be called
        ///       - source returns addons
        ///       - not timed out (configurable)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAvailable()
        {
            TimeSpan maxAge = _config.GetValue<TimeSpan>("addonSourceServices:repositoryMirror:maxAge");

            if (_addonListContainer == null)
            {
                await InitAddonsFromService();
                await InitVersionsFromService();
            }

            return _addonListContainer != null &&
                   _addonListContainer.Addons != null &&
                   _addonListContainer.Addons.Count > 0 &&
                   _addonListContainer.CrawlTime >= DateTime.UtcNow.Subtract(maxAge) &&
                   _versionContainer != null &&
                   _versionContainer.Versions != null &&
                   _versionContainer.Versions.Count > 0;
        }

        /// <summary>
        /// Stores the addon list as JSON for the local file service
        /// </summary>
        public async Task StoreAddons()
        {
            string addonFile = _config.GetValue<string>("addonSourceServices:localFile:addonsPath");

            try
            {
                string json = JsonConvert.SerializeObject(_addonListContainer, Formatting.Indented);
                await File.WriteAllTextAsync(addonFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing addon list to [{addonFile}] failed");
            }
        }

        /// <summary>
        /// Stores the version list as JSON for the local file service
        /// </summary>
        public async Task StoreVersions()
        {
            string versionsFilePath = _config.GetValue<string>("addonSourceServices:localFile:versionsPath");

            try
            {
                string json = JsonConvert.SerializeObject(_versionContainer, Formatting.Indented);
                await File.WriteAllTextAsync(versionsFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing version list to [{versionsFilePath}] failed");
            }
        }

        /// <summary>
        /// Returns the individual addon versions
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<VersionContainer> GetVersions()
        {
            if(_versionContainer == null)
            {
                await InitVersionsFromService();
            }

            return _versionContainer;
        }

        public async Task<AddonListSource> GetSource()
        {
            return AddonListSource.RepositoryMirror;
        }
    }
}
