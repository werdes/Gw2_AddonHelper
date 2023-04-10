using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model.SelfUpdate;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AppUpdaterServices
{
    /// <summary>
    ///  Dummy Updater for having a possible updater if all other methods fail. Will never work, but returns an outdated version 0.0.0.0
    /// </summary>
    internal class LocalFileAppUpdaterService : BaseAppUpdaterService, IAppUpdaterService
    {
        private const int HIERARCHY = 1;
        private string _cachePath;
        private SelfUpdateLocalFile _cacheFile;
        private GitHubClient _gitHubClient;

        public LocalFileAppUpdaterService()
        {
            _cachePath = _config.GetValue<string>("selfUpdate:updaters:localFile:path");
            _gitHubClient = Lib.ServiceProvider.GetService<GitHubClient>();

            Load();
        }

        /// <summary>
        /// Loads the cache file
        /// </summary>
        private void Load()
        {
            try
            {
                if (File.Exists(_cachePath))
                {
                    string localFileJson = File.ReadAllText(_cachePath, Encoding.UTF8);
                    _cacheFile = JsonConvert.DeserializeObject<SelfUpdateLocalFile>(localFileJson);
                }
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, $"Loading local file [{_cachePath}]");
            }
        }

        public int GetHierarchy()
        {
            return HIERARCHY;
        }


        /// <summary>
        /// Returns the latest available version
        /// </summary>
        /// <returns></returns>
        public async Task<(Version, string)> GetLatestVersion()
        {
            if (_cacheFile != null)
            {
                return (new Version(_cacheFile.Version), _cacheFile.Notes);
            }
            return (new Version(0, 0, 0, 0), string.Empty);
        }

        /// <summary>
        /// Returns wether the service implementation can be used for self update
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAvailable()
        {
            if (_cacheFile != null)
            {
                TimeSpan timeSpanMaxAge = _config.GetValue<TimeSpan>("selfUpdate:updaters:localFile:maxAge");
                if (_cacheFile.TimeStamp > DateTime.UtcNow.Subtract(timeSpanMaxAge))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Updates the launcher
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            try
            {
                if (_cacheFile != null)
                {
                    if (GithubRatelimitService.Instance.CanCall())
                    {
                        Uri assetUri = _cacheFile.Uri;
                        Version version = new Version(_cacheFile.Version);

                        if (assetUri != null)
                        {
                            string outputFileName = Path.Combine(_config.GetValue<string>("selfUpdate:updaterAssetDirectory"), $"{version}.zip");
                            await UpdateInternal(version, assetUri, outputFileName);
                        }
                        else
                        {
                            _log.LogWarning($"Asset Uri empty:");
                            _log.LogObject(_cacheFile);
                        }
                    }
                    else
                    {
                        _log.LogWarning("Cannot call github due to ratelimit");
                    }
                }
                else
                {
                    _log.LogWarning("Local file not loaded");
                }
            }
            catch (Exception ex) when (ex is NotFoundException or ArgumentNullException)
            {
                _log.LogWarning($"No version found from local file [{_cachePath}]");
            }
        }

        /// <summary>
        /// Downloads Asset with GithubClient
        /// </summary>
        /// <param name="assetUri"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async Task<byte[]> Download(Uri assetUri)
        {
            byte[] buffer = (await _gitHubClient.Connection.Get<byte[]>(assetUri, TimeSpan.FromSeconds(30))).Body;
            return buffer;
        }

    }
}
