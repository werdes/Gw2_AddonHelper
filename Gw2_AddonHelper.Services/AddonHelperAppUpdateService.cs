using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model.SelfUpdate;
using Gw2_AddonHelper.Common.Utility.Github;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services
{
    public class AddonHelperAppUpdateService : IAppUpdaterService
    {
        private const string ZIP_MIME = "application/x-zip-compressed";

        private GitHubClient _gitHubClient;
        private IConfiguration _config;
        private ILogger<AddonHelperAppUpdateService> _log;

        public AddonHelperAppUpdateService()
        {
            _gitHubClient = Lib.ServiceProvider.GetService<GitHubClient>();
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
            _log = Lib.ServiceProvider.GetService<ILogger<AddonHelperAppUpdateService>>();
        }


        /// <summary>
        /// Checks if an update to the application is available
        /// </summary>
        /// <returns></returns>
        public async Task<Version> GetLatestVersion()
        {
            if (GithubRatelimitService.Instance.CanCall())
            {

                Uri repoUrl = _config.GetValue<Uri>("selfUpdate:repoReleaseUrl");
                _log.LogInformation($"Getting latest app version from [{repoUrl}]");
                try
                {
                    Release githubRelease = (await _gitHubClient.Connection.Get<Release>(repoUrl, TimeSpan.FromSeconds(30))).Body;
                    GithubRatelimitService.Instance.RegisterCall();

                    Version version = new Version(githubRelease.TagName);

                    _log.LogInformation($"Latest app version is [{version}]");
                    return version;
                }
                catch (Exception ex) when (ex is NotFoundException or ArgumentNullException)
                {
                    _log.LogWarning($"No version found for [{repoUrl}]");
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the launcher
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            if (GithubRatelimitService.Instance.CanCall())
            {
                Uri repoUrl = _config.GetValue<Uri>("selfUpdate:repoReleaseUrl");
                try
                {
                    _log.LogInformation($"Getting app update from [{repoUrl}]");
                    Release githubRelease = (await _gitHubClient.Connection.Get<Release>(repoUrl, TimeSpan.FromSeconds(30))).Body;
                    GithubRatelimitService.Instance.RegisterCall();

                    Version version = new Version(githubRelease.TagName);
                    Uri assetUri = new Uri(githubRelease.Assets.Where(x => x.ContentType == ZIP_MIME).First().BrowserDownloadUrl);

                    string outputFileName = Path.Combine(_config.GetValue<string>("selfUpdate:updaterAssetDirectory"), $"{version}.zip");
                    outputFileName = Path.GetFullPath(outputFileName);

                    if (!File.Exists(outputFileName))
                    {
                        byte[] buffer = (await _gitHubClient.Connection.Get<byte[]>(assetUri, TimeSpan.FromSeconds(30))).Body;
                        await File.WriteAllBytesAsync(outputFileName, buffer);
                        _log.LogInformation($"Stored update archive with [{buffer.Length}] bytes to [{outputFileName}]");
                    }

                    if (await SelfUpdateUpdater(outputFileName))
                    {
                        int currentProcessId = Process.GetCurrentProcess().Id;
                        string extractionDirectory = Environment.CurrentDirectory;
                        string callerPath = Environment.GetCommandLineArgs().First();

                        //Reset stored version and version check timestamp
                        IUserConfigService userConfigService = Lib.ServiceProvider.GetService<IUserConfigService>();
                        userConfigService.GetConfig().LastSelfUpdateCheck = DateTime.MinValue;
                        userConfigService.GetConfig().LatestVersion = new Version();
                        userConfigService.Store();

                        Process process = new Process();
                        process.StartInfo.ArgumentList.Add(callerPath);
                        process.StartInfo.ArgumentList.Add(currentProcessId.ToString());
                        process.StartInfo.ArgumentList.Add(outputFileName);
                        process.StartInfo.ArgumentList.Add(extractionDirectory);

                        process.StartInfo.WorkingDirectory = _config.GetValue<string>("selfUpdate:updaterDirectory");
                        process.StartInfo.FileName = Path.Combine(_config.GetValue<string>("selfUpdate:updaterDirectory"),
                                                                  _config.GetValue<string>("selfUpdate:executable"));

                        process.Start();

                        _log.LogInformation("Shutdown for application update");
                        _log.LogObject(process.StartInfo);
                    }
                }
                catch (Exception ex) when (ex is NotFoundException or ArgumentNullException)
                {
                    _log.LogWarning($"No version found for [{repoUrl}]");
                }
            }
        }

        /// <summary>
        /// Updates the self updater from archive if available
        /// </summary>
        /// <param name="zipPath"></param>
        /// <returns></returns>
        private async Task<bool> SelfUpdateUpdater(string zipPath)
        {
            bool success = true;
            List<UpdatePath> updatePaths = _config.GetSection("selfUpdate:updatePaths").Get<List<UpdatePath>>();

            try
            {
                _log.LogInformation($"Updating self updater");
                using (FileStream zipStream = File.OpenRead(zipPath))
                {
                    using (ZipArchive zipArchive = new ZipArchive(zipStream))
                    {
                        List<ZipArchiveEntry> zipEntryFiles = zipArchive.Entries.Where(x => x.IsFile()).ToList();
                        foreach (ZipArchiveEntry entry in zipEntryFiles)
                        {
                            string relativeInternalPath = Path.GetDirectoryName(entry.FullName);
                            UpdatePath updatePath = updatePaths.Where(x => x.ZipPath == relativeInternalPath).FirstOrDefault();

                            if (updatePath != null)
                            {
                                // internal path is allowed
                                string outputPath = entry.FullName.Replace(relativeInternalPath, updatePath.FileSystemPath);
                                string outputDirectory = Path.GetDirectoryName(outputPath);
                                if (!Directory.Exists(outputDirectory))
                                {
                                    Directory.CreateDirectory(outputDirectory);
                                }

                                _log.LogInformation($"Extracting self updater to [{outputPath}]");
                                await Task.Run(() => entry.ExtractToFile(outputPath, true));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                _log.LogCritical(ex, "SelfUpdate Updater");
            }

            return success;
        }
    }
}
