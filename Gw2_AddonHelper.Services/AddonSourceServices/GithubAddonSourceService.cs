using Gw2_AddonHelper.Common.Custom.YamlDotNet;
using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Common.Model.AddonList;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Services.AddonSourceServices.Model.Github;
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

namespace Gw2_AddonHelper.Services.AddonSourceServices
{
    public class GithubAddonSourceService : BaseAddonSourceService, IAddonSourceService
    {
        private const int HIERARCHY = 2;

        private IUserConfigService _userConfigService;

        private AddonListContainer _addonListContainer;
        private VersionContainer _versionContainer;
        private GitHubClient _githubClient = null;
        private Branch _branch = null;
        private WebClient _webClient;

        public GithubAddonSourceService()
        {
            _log = Lib.ServiceProvider.GetService<ILogger<GithubAddonSourceService>>();
            _userConfigService = Lib.ServiceProvider.GetService<IUserConfigService>();
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
            _githubClient = Lib.ServiceProvider.GetService<GitHubClient>();

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
                IDeserializer yamlDeserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .WithTypeConverter(new YamlStringEnumConverter())
                    .Build();
                if (GithubRatelimitService.Instance.CanCall())
                {
                    _log.LogInformation($"Loading addon list from github");

                    Uri zipBallUrl = new Uri(_config.GetValue<string>("addonSourceServices:github:repositoryZipBallUrl"));
                    _log.LogInformation($"Loading repo from [{zipBallUrl}]");

                    await GithubRatelimitService.Instance.Wait();
                    List<YamlAddonDescription> fileContents = await GetAddonDescriptionsFromZipBall(zipBallUrl);
                    GithubRatelimitService.Instance.RegisterCall();

                    _addonListContainer = new AddonListContainer();
                    _addonListContainer.RepositoryVersion = await GetCurrentRepositoryCommitSha();
                    _addonListContainer.Source = AddonListSource.GitHub;

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

                                _addonListContainer.Addons.Add(addon);
                                _log.LogInformation($"Addon added: [{addon.AddonId}]");
                            }
                        }
                        catch (YamlException ex)
                        {
                            _log.LogError(ex, $"Deserialization of [{addonDescription.FileName}] failed");
                        }
                    }


                    // add Addon Loader
                    Addon addonLoader = _config.GetSection("defaultValues:addonLoader").Get<Addon>();
                    _addonListContainer.Addons.ForEach(x => x.RequiredAddons.Insert(0, addonLoader.AddonId));
                    _addonListContainer.Addons.Insert(0, addonLoader);

                    await StoreAddons();
                }
            }
            catch (RateLimitExceededException ex)
            {
                // Saturate the Ratelimit (Exception from Ratelimit)
                _log.LogError(ex, $"Github Ratelimit exceeded, saturating limiter");
                GithubRatelimitService.Instance.Saturate();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Retrieving addons from github failed");
            }
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
        /// Gets the current SHA of the approved addons repository
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetCurrentRepositoryCommitSha()
        {
            await LoadBranch();
            return _branch.Commit.Sha;
        }

        /// <summary>
        /// Loads Branch information
        /// </summary>
        /// <returns></returns>
        private async Task LoadBranch()
        {
            await GithubRatelimitService.Instance.Wait();
            _branch = await _githubClient.Repository.Branch.Get(
                _config.GetValue<string>("addonSourceServices:github:repositoryOwner"),
                _config.GetValue<string>("addonSourceServices:github:repositoryName"),
                _config.GetValue<string>("addonSourceServices:github:repositoryBranch"));
            GithubRatelimitService.Instance.RegisterCall();
        }

        /// <summary>
        /// Loads the service versions information
        /// </summary>
        /// <returns></returns>
        private async Task InitVersionsFromService()
        {
            try
            {
                _versionContainer = new VersionContainer();
                _versionContainer.CrawlTime = DateTime.Now;
                _versionContainer.Source = AddonListSource.GitHub;

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
            return await Task.Run(() => GithubRatelimitService.Instance.CanCall());
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
            if (_versionContainer == null)
            {
                await InitVersionsFromService();
            }

            return _versionContainer;
        }

        public async Task<AddonListSource> GetSource()
        {
            return AddonListSource.GitHub;
        }
    }
}
