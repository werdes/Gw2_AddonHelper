using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services
{
    public class RepositoryMirrorAddonListService : IAddonListService
    {
        private ILogger _log;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;
        private AddonListContainer _addonList;
        private WebClient _webClient;

        public RepositoryMirrorAddonListService(ILogger<RepositoryMirrorAddonListService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;
            _addonList = new AddonListContainer();

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());

            Load();
        }

        /// <summary>
        /// Returns a list of yaml based addons from a Github Repository
        /// </summary>
        /// <returns></returns>
        public async Task<List<Addon>> GetAddonsAsync()
        {
            try
            {
                Uri updateUri = new Uri(_config.GetValue<string>("repositoryMirrorAddonList:url"));
                string json = await _webClient.DownloadStringTaskAsync(updateUri);

                _addonList = JsonConvert.DeserializeObject<AddonListContainer>(json);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Retrieving addons from repo mirror failed");
            }

            return _addonList.Addons;
        }

        /// <summary>
        /// Loads the addon list from json file
        /// </summary>
        public async Task Load()
        {
            string addonFile = _config.GetValue<string>("repositoryMirrorAddonList:filePath");

            try
            {
                string json = await File.ReadAllTextAsync(addonFile, Encoding.UTF8);
                AddonListContainer tempList = JsonConvert.DeserializeObject<AddonListContainer>(json);

                _addonList = tempList;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading addon list from [{addonFile}] failed");
            }
        }

        /// <summary>
        /// Stores the addon list as JSON
        /// </summary>
        public async Task Store()
        {
            string addonFile = _config.GetValue<string>("repositoryMirrorAddonList:filePath");

            try
            {
                string json = JsonConvert.SerializeObject(_addonList, Formatting.Indented);
                await File.WriteAllTextAsync(addonFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing addon list to [{addonFile}] failed");
            }
        }

        public async Task<string> GetListVersion()
        {
            return _addonList.RepositoryVersion;
        }

        public async Task<DateTime> GetListTimestamp()
        {
            return _addonList.CrawlTime;
        }

        /// <summary>
        /// Loads a list of current versions for quick update check on list creation
        /// </summary>
        /// <returns></returns>
        public async Task<VersionContainer> GetVersions()
        {
            VersionContainer container = new VersionContainer();
            try
            {
                Uri updateUri = new Uri(_config.GetValue<string>("repositoryMirrorAddonList:updateUrl"));
                string json = await _webClient.DownloadStringTaskAsync(updateUri);

                container = JsonConvert.DeserializeObject<VersionContainer>(json);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, nameof(GetVersions));
            }
            return container;
        }
    }
}
