using Gw2_AddonHelper.Model.UserConfig;
using Gw2_AddonHelper.Services;
using Microsoft.Extensions.Configuration;
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
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            AddonLib.Lib.ServiceProvider = ServiceProvider;

            _log = ServiceProvider.GetService<ILogger<App>>();

            ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
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
            lstDirs.Add(Path.GetDirectoryName(config["addonsFile"]));
            lstDirs.Add(Path.GetDirectoryName(config["userConfig"]));
            lstDirs.Add(Path.GetDirectoryName(config["githubRatelimitFile"]));
            lstDirs.Add(Path.GetDirectoryName(config["githubAddonList:filePath"]));
            lstDirs.Add(Path.GetDirectoryName(config["selfUpdate:updaterDirectory"]));
            lstDirs.Add(Path.GetDirectoryName(config["selfUpdate:updaterAssetDirectory"]));

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
            services.AddSingleton<IAddonListService, GithubAddonListService>();
            services.AddSingleton<IAddonGameStateService, AddonGameStateService>();
            services.AddSingleton<IUserConfigService, JsonUserConfigService>();
            services.AddSingleton<IAppUpdaterService, AddonHelperAppUpdateService>();

            services.AddTransient<UI.MainWindow>();
        }
    }
}
