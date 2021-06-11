using Gw2_AddonHelper.AddonLib.Extensions;
using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.AddonList;
using Gw2_AddonHelper.AddonLib.Model.AddonList.Github;
using Gw2_AddonHelper.AddonLib.Utility.Addon.Downloader;
using Gw2_AddonHelper.UpdateCheck.Custom.YamlDotNet;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gw2_AddonHelper.UpdateCheck
{
    public class Gw2AddonHelperUpdateCheck
    {
        private ILogger _log;
        private IConfiguration _config;

        public Gw2AddonHelperUpdateCheck()
        {
            _log = Program.ServiceProvider.GetService<ILogger<Gw2AddonHelperUpdateCheck>>();
            _config = Program.ServiceProvider.GetService<IConfiguration>();

            Task.WaitAll(Run());
        }

        /// <summary>
        /// Runs the stuff
        /// </summary>
        /// <returns></returns>
        private async Task Run()
        {
            GithubAddonList addonList = Load();
            while (true)
            {
                DateTime end = DateTime.Now + _config.GetValue<TimeSpan>("delay");

                try
                {
                    VersionContainer versionContainer = new VersionContainer();
                    string currentSha = await GetCurrentRepositoryCommitSha();
                    if(currentSha != addonList.CommitSha)
                    {
                        addonList = GetAddons(currentSha);
                        Store(addonList);
                    }

                    foreach (Addon addon in addonList.Addons)
                    {
                        IAddonDownloader downloader = AddonDownloaderFactory.GetDownloader(addon);
                        versionContainer.Versions.Add(addon.AddonId, await downloader.GetLatestVersion());
                    }

                    string[] files = _config.GetSection("outputPath").Get<string[]>();
                    foreach (string outputFile in files)
                    {
                        _log.LogInformation($"Writing output [{outputFile}]");
                        File.WriteAllText(outputFile, JsonConvert.SerializeObject(versionContainer), Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogCritical(ex, nameof(Run));
                }

                double sleepMs = Math.Max(0, (end - DateTime.Now).TotalMilliseconds);
                _log.LogInformation($"Sleeping for [{sleepMs}] ms");

                Thread.Sleep((int)sleepMs);
            }
        }

        /// <summary>
        /// Returns a list of yaml based addons from a Github Repository
        /// </summary>
        /// <returns></returns>
        private GithubAddonList GetAddons(string currentSha)
        {
            List<Addon> addons = new List<Addon>();
            GithubAddonList addonList = new GithubAddonList();

            IDeserializer yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithTypeConverter(new YamlStringEnumConverter())
                .Build();

            Uri zipBallUrl = new Uri(_config.GetValue<string>("githubAddonList:repositoryZipBallUrl"));

            try
            {
                List<YamlAddonDescription> fileContents = GetAddonDescriptionsFromZipBall(zipBallUrl);

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
                    catch (Exception ex)
                    {
                        _log.LogError(ex, $"Deserialization of [{addonDescription.FileName}] failed");
                    }
                }

                addonList.CommitSha = currentSha;
                addonList.RetrievedAt = DateTime.UtcNow;
                addonList.Addons.Clear();
                addonList.Addons.AddRange(addons);

                // Addon Loader
                Addon addonLoader = _config.GetSection("addonLoader").Get<Addon>();

                addonList.Addons.ForEach(x => x.RequiredAddons.Insert(0, addonLoader.AddonId));
                addonList.Addons.Insert(0, addonLoader);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Downloading or reading addon list from [{zipBallUrl}] failed");
            }

            return addonList;
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
            if (!string.IsNullOrWhiteSpace(addon.PluginName))
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
        private List<YamlAddonDescription> GetAddonDescriptionsFromZipBall(Uri zipBallUrl)
        {
            List<YamlAddonDescription> fileContents = new List<YamlAddonDescription>();
            GitHubClient gitHubClient = Program.ServiceProvider.GetService<GitHubClient>();

            byte[] buffer = null;

            IApiResponse<byte[]> response = gitHubClient.Connection.Get<byte[]>(zipBallUrl, TimeSpan.FromSeconds(30)).Result;
            buffer = response.Body;

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
        public GithubAddonList Load()
        {
            string addonFile = _config.GetValue<string>("githubAddonList:filePath");
            GithubAddonList addonList = new GithubAddonList();

            try
            {
                string json = File.ReadAllText(addonFile, Encoding.UTF8);
                GithubAddonList tempList = JsonConvert.DeserializeObject<GithubAddonList>(json);

                addonList = tempList;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading addons from [{addonFile}] failed");
            }

            return addonList;
        }

        /// <summary>
        /// Stores the addon list as JSON
        /// </summary>
        public void Store(GithubAddonList addonList)
        {
            string addonFile = _config.GetValue<string>("githubAddonList:filePath");

            try
            {
                string json = JsonConvert.SerializeObject(addonList, Formatting.Indented);
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
            GitHubClient gitHubClient = Program.ServiceProvider.GetService<GitHubClient>();

            Branch branch = await gitHubClient.Repository.Branch.Get(
                _config.GetValue<string>("githubAddonList:repositoryOwner"),
                _config.GetValue<string>("githubAddonList:repositoryName"),
                _config.GetValue<string>("githubAddonList:repositoryBranch"));
            return branch.Commit.Sha;
        }
    }
}
