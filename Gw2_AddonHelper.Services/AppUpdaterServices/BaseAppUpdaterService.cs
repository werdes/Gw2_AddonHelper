using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model.SelfUpdate;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AppUpdaterServices
{
    public abstract class BaseAppUpdaterService
    {
        protected IConfiguration _config;
        protected ILogger<GithubAppUpdateService> _log;
        protected IUserConfigService _userConfig;
        private HttpClient _httpClient;
        private IProgress<double> _downloadProgress;

        public event EventHandler<AppUpdateDownloadEventArgs> UpdateProgress;

        public BaseAppUpdaterService()
        {
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
            _userConfig = Lib.ServiceProvider.GetService<IUserConfigService>();
            _log = Lib.ServiceProvider.GetService<ILogger<GithubAppUpdateService>>();

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
            _httpClient.Timeout = TimeSpan.FromMinutes(5);

            _downloadProgress = new Progress<double>(x => UpdateProgress?.Invoke(this, new AppUpdateDownloadEventArgs(x)));
        }

        protected async Task<byte[]> Download(Uri assetUri)
        {
            using(MemoryStream downloadStream = new MemoryStream())
            {
                await _httpClient.DownloadAsync(assetUri, downloadStream, _downloadProgress);
                return downloadStream.ToArray();
            }
        }

        protected async Task StoreFile(Uri uri, string version, string notes)
        {
            string filePath = _config.GetValue<string>("selfUpdate:updaters:localFile:path");
            SelfUpdateLocalFile localFile = new SelfUpdateLocalFile()
            {
                Uri = uri,
                Version = version,
                Notes = notes,
                TimeStamp = DateTime.UtcNow
            };

            string localFileJson = JsonConvert.SerializeObject(localFile, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, localFileJson, Encoding.UTF8);
        }

        /// <summary>
        /// Calls the download, unpacks and updates first the Self-Updater and then calls it
        /// </summary>
        /// <param name="version"></param>
        /// <param name="assetUri"></param>
        /// <param name="outputFileName"></param>
        /// <returns></returns>
        protected async Task UpdateInternal(Version version, Uri assetUri, string outputFileName)
        {

            outputFileName = Path.GetFullPath(outputFileName);

            if (!File.Exists(outputFileName))
            {
                byte[] buffer = await Download(assetUri);

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
            catch (Exception ex)
            {
                success = false;
                _log.LogCritical(ex, "SelfUpdate Updater");
            }

            return success;
        }
    }
}
