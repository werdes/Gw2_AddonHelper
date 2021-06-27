using Gw2_AddonHelper.AddonLib.Extensions;
using Gw2_AddonHelper.AddonLib.Model.SelfUpdate;
using Gw2_AddonHelper.AddonLib.Utility.Github;
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
using System.Text;
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
            _gitHubClient = App.ServiceProvider.GetService<GitHubClient>();
            _config = App.ServiceProvider.GetService<IConfiguration>();
            _log = App.ServiceProvider.GetService<ILogger<AddonHelperAppUpdateService>>();
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
                try
                {
                    Release githubRelease = (await _gitHubClient.Connection.Get<Release>(repoUrl, TimeSpan.FromSeconds(30))).Body;
                    GithubRatelimitService.Instance.RegisterCall();

                    Version version = new Version(githubRelease.TagName);
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
                    }

                    if (await SelfUpdateUpdater(outputFileName))
                    {
                        int currentProcessId = Process.GetCurrentProcess().Id;
                        string extractionDirectory = Environment.CurrentDirectory;
                        string callerPath = Environment.GetCommandLineArgs().First();

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
                        System.Windows.Application.Current.Shutdown();
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
