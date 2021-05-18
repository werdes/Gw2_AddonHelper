using Gw2_AddonHelper.Custom.YamlDotNet;
using Gw2_AddonHelper.Extensions;
using Gw2_AddonHelper.Model;
using Gw2_AddonHelper.Model.AddonList;
using Gw2_AddonHelper.Model.AddonList.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Gw2_AddonHelper.Utility.Github;
using Microsoft.Extensions.Configuration;
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

        public GithubAddonListService(ILogger<GithubAddonListService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;

            _addonList = new GithubAddonList();
            _githubClient = new GitHubClient(new ProductHeaderValue(
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()));

            Load();
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
            DateTime checkThreshold = DateTime.UtcNow.AddMinutes(-1 * _config.GetValue<int>("githubAddonList:refreshCooldown"));

            //Check Refresh cooldown, Ratelimit 
            if ((lastCheck <= checkThreshold || !File.Exists(storedFile)) &&
                GithubRatelimitService.Instance.CanCall())
            {

                //Check if a commit was made since last update -> commit sha changed
                string currentSha = await GetCurrentRepositoryCommitSha();
                if (currentSha != _addonList.CommitSha)
                {
                    Uri zipBallUrl = new Uri(_config.GetValue<string>("githubAddonList:repositoryZipBallUrl"));
                    
                    try
                    {
                        List<YamlAddonDescription> fileContents = await GetAddonDescriptionsFromZipBall(zipBallUrl);

                        foreach (YamlAddonDescription addonDescription in fileContents)
                        {
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

                                    Addon addon = yamlDeserializer.Deserialize<Addon>(addonDescriptionYamlContent);
                                    addon.AddonId = addonDescription.FileName.Split('/')[1];
                                    addon.VersioningType = GetVersioningType(addon);
                                    addon.LoaderKey = GetLoaderKey(addon);

                                    addons.Add(addon);
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
                        Addon addonLoader = _config.GetSection("addonLoader").Get<Addon>();

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
            return loaderKey;
        }

        /// <summary>
        /// Returns the versioning type of the given addon
        /// </summary>
        /// <param name="addon"></param>
        /// <returns></returns>
        private VersioningType GetVersioningType(Addon addon)
        {
            if (addon.HostType == HostType.Github) return VersioningType.GithubCommitSha;
            if (addon.HostType == HostType.Standalone && addon.VersionUrl != null) return VersioningType.HostFileMd5;
            if (addon.HostType == HostType.Standalone && addon.AdditionalFlags.Contains(AddonFlag.SelfUpdating)) return VersioningType.SelfUpdating;
            return VersioningType.Unknown;
        }

        /// <summary>
        /// Downloads the repository zip ball, extracts the yaml configuration files
        /// </summary>
        /// <param name="zipBallUrl"></param>
        /// <returns></returns>
        private async Task<List<YamlAddonDescription>> GetAddonDescriptionsFromZipBall(Uri zipBallUrl)
        {
            List<YamlAddonDescription> fileContents = new List<YamlAddonDescription>();
            byte[] buffer = (await _githubClient.Connection.Get<byte[]>(zipBallUrl, TimeSpan.FromSeconds(30))).Body;   //await _webClient.DownloadDataTaskAsync(zipBallUrl);
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
        public void Load()
        {
            string addonFile = _config.GetValue<string>("githubAddonList:filePath");

            try
            {
                string json = File.ReadAllText(addonFile, Encoding.UTF8);
                GithubAddonList tempList = JsonConvert.DeserializeObject<GithubAddonList>(json);

                _addonList = tempList;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading configuration from [{addonFile}] failed");
            }
        }

        /// <summary>
        /// Stores the addon list as JSON
        /// </summary>
        public void Store()
        {
            string addonFile = _config.GetValue<string>("githubAddonList:filePath");

            try
            {
                string json = JsonConvert.SerializeObject(_addonList, Formatting.Indented);
                File.WriteAllText(addonFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing configuration to [{addonFile}] failed");
            }
        }

        /// <summary>
        /// Gets the current SHA of the approved addons repository
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetCurrentRepositoryCommitSha()
        {

            Branch branch = await _githubClient.Repository.Branch.Get(
                _config.GetValue<string>("githubAddonList:repositoryOwner"),
                _config.GetValue<string>("githubAddonList:repositoryName"),
                _config.GetValue<string>("githubAddonList:repositoryBranch"));
            GithubRatelimitService.Instance.RegisterCall();

            _userConfigService.GetConfig().LastGithubCheck = DateTime.UtcNow;
            _userConfigService.Store();

            return branch.Commit.Sha;
        }
    }
}
