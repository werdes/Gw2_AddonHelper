using Gw2_AddonHelper.AddonLib.Model.AddonList.Github;
using Gw2_AddonHelper.Common.Custom.YamlDotNet;
using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gw2_AddonHelper.Services
{
    public class GithubAddonListService : IAddonListService
    {
        private ILogger _log;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;

        private GitHubClient _githubClient = null;
        private GithubAddonList _addonList = null;
        private VersionContainer _versionContainer;
        private Branch _branch = null;
        private WebClient _webClient;

        public GithubAddonListService(ILogger<GithubAddonListService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;
            _githubClient = Lib.ServiceProvider.GetService<GitHubClient>();
            _addonList = new GithubAddonList();

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
        }

        /// <summary>
        /// Returns a list of yaml based addons from a Github Repository
        /// </summary>
        /// <returns></returns>
        public async Task<List<Addon>> GetAddonsAsync()
        {
            List<Addon> addons = new List<Addon>();
            string storedFile = _config.GetValue<string>("githubAddonList:filePath");

            IDeserializer yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithTypeConverter(new YamlStringEnumConverter())
                .Build();

            DateTime lastCheck = _userConfigService.GetConfig().LastGithubCheck;
            DateTime checkThreshold = DateTime.UtcNow.Add(-1 * _config.GetValue<TimeSpan>("githubAddonList:refreshCooldown"));

            //Check Refresh cooldown, Ratelimit 
            if ((lastCheck <= checkThreshold || !File.Exists(storedFile)) &&
                GithubRatelimitService.Instance.CanCall())
            {
                _log.LogInformation($"Loading addon list from github");

                //Check if a commit was made since last update -> commit sha changed
                string currentSha = await GetCurrentRepositoryCommitSha();

                _log.LogInformation($"Current repo version is [{currentSha}]");
                if (currentSha != _addonList.CommitSha)
                {
                    Uri zipBallUrl = new Uri(_config.GetValue<string>("githubAddonList:repositoryZipBallUrl"));
                    _log.LogInformation($"Loading repo from [{zipBallUrl}]");

                    try
                    {
                        List<YamlAddonDescription> fileContents = await GetAddonDescriptionsFromZipBall(zipBallUrl);

                        foreach (YamlAddonDescription addonDescription in fileContents)
                        {

                            _log.LogInformation($"Reading addon [{addonDescription.FileName}]");
                            try
                            {
                                // more than 2 segments in addon description file path -> ignore the template file located in the repo root
                                if (addonDescription.FileName.Split('/').Length > 2)
                                {
                                    //Fixes for bad YAML
                                    string addonDescriptionYamlContent = addonDescription.Content;

                                    // 1) Parser doesn't properly recognize escaped double quotes
                                    //     => remove escaped quotes
                                    //     => remove line break with regex
                                    addonDescriptionYamlContent = addonDescriptionYamlContent.Replace("\\\"", string.Empty);
                                    addonDescriptionYamlContent = Regex.Replace(addonDescriptionYamlContent, "\"[^\"]*(?:\"\"[^\"]*)*\"",
                                                                                match => match.Value.Replace("\n", "").Replace("\r", ""));
                                    addonDescriptionYamlContent = addonDescriptionYamlContent.Replace("\\n", "");

                                    Addon addon = yamlDeserializer.Deserialize<Addon>(addonDescriptionYamlContent);
                                    addon.AddonId = addonDescription.FileName.Split('/')[1];
                                    addon.LoaderKey = GetLoaderKey(addon);

                                    addons.Add(addon);
                                    _log.LogInformation($"Addon added: [{addon.AddonId}]");
                                }
                            }
                            catch (YamlException ex)
                            {
                                _log.LogError(ex, $"Deserialization of [{addonDescription.FileName}] failed");
                            }
                        }

                        _addonList.CommitSha = currentSha;
                        _addonList.RetrievedAt = DateTime.UtcNow;
                        _addonList.Addons.Clear();
                        _addonList.Addons.AddRange(addons);

                        // Addon Loader
                        Addon addonLoader = _config.GetSection("defaultValues:addonLoader").Get<Addon>();

                        _addonList.Addons.ForEach(x => x.RequiredAddons.Insert(0, addonLoader.AddonId));
                        _addonList.Addons.Insert(0, addonLoader);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, $"Downloading or reading addon list from [{zipBallUrl}] failed");
                    }
                }
            }

            return _addonList.Addons.ToList();
        }

        /// <summary>
        /// Returns the key which the addon loader plugin uses
        /// </summary>
        /// <param name="addon"></param>
        /// <returns></returns>
        private string GetLoaderKey(Addon addon)
        {
            string loaderKey = addon.AddonId;
            string loaderPrefix = _config.GetValue<string>("installation:binary:prefix");
            if(!string.IsNullOrWhiteSpace(addon.PluginName))
            {
                loaderKey = addon.PluginName.Replace(loaderPrefix, string.Empty);
            }

            string customConfigKey = $"customLoaderKeys:{addon.AddonId}";
            string customLoaderKey = _config.GetValue<string>(customConfigKey);
            if (!string.IsNullOrEmpty(customLoaderKey))
            {
                loaderKey = customLoaderKey;
            }


            _log.LogDebug($"Addon key for [{addon.AddonId}]: [{loaderKey}]");

            return loaderKey;
        }

        /// <summary>
        /// Downloads the repository zip ball, extracts the yaml configuration files
        /// </summary>
        /// <param name="zipBallUrl"></param>
        /// <returns></returns>
        private async Task<List<YamlAddonDescription>> GetAddonDescriptionsFromZipBall(Uri zipBallUrl)
        {
            List<YamlAddonDescription> fileContents = new List<YamlAddonDescription>();
            byte[] buffer = (await _githubClient.Connection.Get<byte[]>(zipBallUrl, TimeSpan.FromSeconds(30))).Body;
            using (MemoryStream zipStream = new MemoryStream(buffer))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipStream))
                {
                    IEnumerable<ZipArchiveEntry> yamlEntries = zipArchive.Entries.Where(x => x.Name.ToLower().EndsWith(".yaml"));
                    foreach (ZipArchiveEntry entry in yamlEntries)
                    {
                        using (Stream fileStream = entry.Open())
                        {
                            using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
                            {
                                string yamlContent = reader.ReadToEnd();
                                fileContents.Add(new YamlAddonDescription()
                                {
                                    Content = yamlContent,
                                    FileName = entry.FullName
                                });
                            }
                        }
                    }
                }
            }
            return fileContents;
        }

        /// <summary>
        /// Loads the addon list from json file
        /// </summary>
        public async Task Load()
        {
            string addonFile = _config.GetValue<string>("githubAddonList:filePath");

            try
            {
                string json = await File.ReadAllTextAsync(addonFile, Encoding.UTF8);
                GithubAddonList tempList = JsonConvert.DeserializeObject<GithubAddonList>(json);

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
            string addonFile = _config.GetValue<string>("githubAddonList:filePath");

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

        /// <summary>
        /// Gets the current SHA of the approved addons repository
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetCurrentRepositoryCommitSha()
        {
            await LoadBranch();
            return _branch.Commit.Sha;
        }

        /// <summary>
        /// Returns the version of the given list
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetListVersion()
        {
            return await GetCurrentRepositoryCommitSha();
        }

        /// <summary>
        /// Loads Branch information
        /// </summary>
        /// <returns></returns>
        private async Task LoadBranch()
        {
            await GithubRatelimitService.Instance.Wait();
            _branch = await _githubClient.Repository.Branch.Get(
                _config.GetValue<string>("githubAddonList:repositoryOwner"),
                _config.GetValue<string>("githubAddonList:repositoryName"),
                _config.GetValue<string>("githubAddonList:repositoryBranch"));
            _userConfigService.GetConfig().LastGithubCheck = DateTime.UtcNow;
            _userConfigService.Store();
            GithubRatelimitService.Instance.RegisterCall();
        }

        /// <summary>
        /// Returns the timestamp of the list
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetListTimestamp()
        {
            if(_branch == null)
            {
                await LoadBranch();
            }

            return _branch.Commit.Repository.UpdatedAt.UtcDateTime;
        }

        /// <summary>
        /// Loads a list of current versions for quick update check on list creation
        /// </summary>
        /// <returns></returns>
        public async Task<VersionContainer> GetVersions()
        {
            if (_versionContainer == null)
                await LoadVersions();

            return _versionContainer;
        }

        /// <summary>
        /// Loads versions
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> LoadVersions()
        {
            try
            {
                //Uri updateUri = new Uri(_config.GetValue<string>("githubAddonList:quickUpdateCheck:url"));
                //string json = await _webClient.DownloadStringTaskAsync(updateUri);

                //_versionContainer = JsonConvert.DeserializeObject<VersionContainer>(json);
                //DateTime minCrawlTime = DateTime.UtcNow - _config.GetValue<TimeSpan>("githubAddonList:quickUpdateCheck:maxAge");

                //if (_versionContainer.CrawlTime < minCrawlTime)
                //{
                //    _versionContainer.Versions = new Dictionary<string, string>();
                //}
                //return _versionContainer.CrawlTime;

                _versionContainer = new VersionContainer();
                return DateTime.Now;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, nameof(GetVersions));
            }
            return DateTime.MinValue;
        }

        public AddonListSource GetListSource()
        {
            return AddonListSource.GitHub;
        }
    }
}
