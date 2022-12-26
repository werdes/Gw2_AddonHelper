﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using Gw2_AddonHelper.Services.Interfaces;
using Octokit;
using System.Threading.Tasks;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Services.AppUpdaterServices;
using Gw2_AddonHelper.Services.UserConfigServices;
using Gw2_AddonHelper.Services.AddonGameStateServices;
using Gw2_AddonHelper.Services.AddonSourceServices;

namespace Gw2_AddonHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        private ILogger<App> _log;

        /// <summary>
        /// Constructor
        ///  > Initialize IOC
        /// </summary>
        public App()
        {
            try
            {
                ServiceCollection services = new ServiceCollection();
                ConfigureServices(services);

                ServiceProvider = services.BuildServiceProvider();
                AddonLib.Lib.ServiceProvider = ServiceProvider;
                Services.Lib.ServiceProvider = ServiceProvider;
                Common.Lib.ServiceProvider = ServiceProvider;

                _log = ServiceProvider.GetService<ILogger<App>>();

                ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Critical error occured", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Startup Event
        ///  > Show Main Window
        ///  > Check App Environment Directories
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            IConfiguration config = ServiceProvider.GetService<IConfiguration>();
            IUserConfigService userConfigService = ServiceProvider.GetService<IUserConfigService>();

            userConfigService.Load();

            InitializeEnvironment(config);
            UI.MainWindow mainWindow = ServiceProvider.GetService<UI.MainWindow>();
            mainWindow.Show();
        }


        /// <summary>
        /// Check and create necessary Directories
        /// </summary>
        private void InitializeEnvironment(IConfiguration config)
        {
            List<string> lstDirs = new List<string>();
            lstDirs.Add(Path.GetDirectoryName(config["userConfig"]));
            lstDirs.Add(Path.GetDirectoryName(config["githubRatelimitFile"]));
            lstDirs.Add(Path.GetDirectoryName(config["selfUpdate:updaterDirectory"]));
            lstDirs.Add(Path.GetDirectoryName(config["selfUpdate:updaterAssetDirectory"]));
            lstDirs.Add(Path.GetDirectoryName(config["addonSourceServices:localFile:addonsPath"]));
            lstDirs.Add(Path.GetDirectoryName(config["addonSourceServices:localFile:versionsPath"]));

            try
            {
                foreach (string dir in lstDirs)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Initialization of Environment failed");
            }
        }


        /// <summary>
        /// Configures IOC
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddLog4Net("log.config");
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            IConfiguration configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", false)
               .Build();
            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue(
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()));

            services.AddSingleton(configuration);
            services.AddSingleton(gitHubClient);
            services.AddSingleton<IAddonGameStateService, AddonGameStateService>();
            services.AddSingleton<IUserConfigService, JsonUserConfigService>();

            services.AddTransient<UI.MainWindow>();
        }
    }
}
