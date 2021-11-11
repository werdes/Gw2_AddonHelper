using Gw2_AddonHelper.AddonLib.Model.SelfUpdate;
using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.SelfUpdater.Model.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gw2_AddonHelper.SelfUpdater.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel _viewModel;
        private IConfiguration _config;
        private ILogger<MainWindow> _log;


        public MainWindow(IConfiguration config, ILogger<MainWindow> log)
        {
            InitializeComponent();

            _log = log;
            _config = config;

            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        /// <summary>
        /// Loaded handler
        /// > will basically run the whole process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            string callerPath = null;
            int callerProcessId = -1;
            string archivePath = null;
            string targetDirectory = null;

            try
            {
                string[] args = Environment.GetCommandLineArgs();

                callerPath = args.TryGet(1);

                if (!string.IsNullOrEmpty(callerPath))
                {
                    string callerProcess = args.TryGet(2);
                    if (!string.IsNullOrEmpty(callerProcess) && int.TryParse(callerProcess, out callerProcessId))
                    {
                        await WaitForProcessClosed(callerProcessId);

                        archivePath = args.TryGet(3);
                        targetDirectory = args.TryGet(4);

                        if (!string.IsNullOrEmpty(archivePath) &&
                           !string.IsNullOrEmpty(targetDirectory) &&
                           File.Exists(archivePath) &&
                           Directory.Exists(targetDirectory))
                        {
                            _viewModel.Waiting = false;

                            //Delay to show the window
                            await Task.Delay(1000);
                            if (await Update(archivePath, targetDirectory))
                            {
                                string archiveMoveLocation = Path.Combine(_config.GetValue<string>("moveOldAssetsTo"),
                                                                          Path.GetFileName(archivePath));
                                File.Move(archivePath, archiveMoveLocation, true);
                            }
                        }
                        else _log.LogCritical("Invalid target directory or archive");
                    }
                    else _log.LogCritical("Invalid caller process id");
                }
                else _log.LogCritical("No caller path provided");
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, nameof(OnWindowLoaded));
            }

            await ReturnToCaller(callerPath);

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Restarts the application
        /// </summary>
        /// <param name="callerPath"></param>
        private async Task ReturnToCaller(string callerPath)
        {
            if (!string.IsNullOrEmpty(callerPath))
            {
                _log.LogInformation($"Return to caller [{callerPath}]");

                //Delay to show the result window
                await Task.Delay(1000);


                Process process = new Process();
                process.StartInfo.FileName = callerPath;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(callerPath);

                process.Start();
            }
        }


        /// <summary>
        /// Updates from archive
        /// </summary>
        /// <param name="zipPath"></param>
        /// <returns></returns>
        private async Task<bool> Update(string archivePath, string targetDirectory)
        {
            bool success = true;
            List<UpdatePath> updatePaths = _config.GetSection("updatePaths").Get<List<UpdatePath>>();

            try
            {
                using (FileStream zipStream = File.OpenRead(archivePath))
                {
                    using (ZipArchive zipArchive = new ZipArchive(zipStream))
                    {
                        List<ZipArchiveEntry> zipEntryFiles = zipArchive.Entries.Where(x => x.IsFile()).ToList();
                        _viewModel.MaxValue = zipEntryFiles.Count;

                        foreach (ZipArchiveEntry entry in zipEntryFiles)
                        {
                            string relativeInternalPath = Path.GetDirectoryName(entry.FullName);
                            UpdatePath updatePath = updatePaths.Where(x => x.ZipPath == relativeInternalPath).FirstOrDefault();

                            if (updatePath != null)
                            {
                                List<string> outputDirectoryParts = new List<string>();
                                outputDirectoryParts.Add(targetDirectory);
                                outputDirectoryParts.Add(relativeInternalPath);

                                outputDirectoryParts = outputDirectoryParts.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                                outputDirectoryParts.Add(entry.Name);

                                // internal path is allowed
                                string outputPath = Path.Combine(outputDirectoryParts.ToArray());
                                string outputDirectory = Path.GetDirectoryName(outputPath);
                                if(!Directory.Exists(outputDirectory))
                                {
                                    Directory.CreateDirectory(outputDirectory);
                                }

                                _log.LogDebug($"Updating [{outputPath}]");
                                await Task.Run(() => entry.ExtractToFile(outputPath, true));
                                _viewModel.Value++;
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

        /// <summary>
        /// Waits for a certain process to finish
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        private async Task WaitForProcessClosed(int processId)
        {
            Process process = Process.GetProcessById(processId);
            while(!process.HasExited)
            {
                await Task.Delay(250);
            }
        }
    }
}
