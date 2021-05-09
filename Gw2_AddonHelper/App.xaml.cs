﻿using Gw2_AddonHelper.Model.UserConfig;
using Gw2_AddonHelper.Services;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;

namespace Gw2_AddonHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        private ILogger<App> _log;

        /// <summary>
        /// Constructor
        ///  > Initialize IOC
        /// </summary>
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            _log = ServiceProvider.GetService<ILogger<App>>();
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
            UserConfig userConfig = userConfigService.GetConfig();


            InitializeEnvironment(config);
            Gw2_AddonHelper.Properties.Localization.Culture = new CultureInfo(userConfig.Language);

            UI.MainWindow mainWindow = ServiceProvider.GetService<UI.MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        /// Check and create necessary Directories
        /// </summary>
        private void InitializeEnvironment(IConfiguration config)
        {
            List<string> lstDirs = new List<string>();
            lstDirs.Add(Path.GetDirectoryName(config["addonsFile"]));
            lstDirs.Add(Path.GetDirectoryName(config["userConfig"]));

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
            });

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton<IAddonListService, GithubAddonListService>();
            services.AddSingleton<IUserConfigService, JsonUserConfigService>();


            services.AddTransient<UI.MainWindow>();

        }
    }
}