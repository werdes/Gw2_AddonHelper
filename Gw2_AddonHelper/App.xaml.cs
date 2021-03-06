using Gw2_AddonHelper.Model.UserConfig;
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
using System.Threading.Tasks;
using Gw2_AddonHelper.Common.Model;

namespace Gw2_AddonHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        public static IAddonListService AddonListService { get; private set; }

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
            Services.Lib.ServiceProvider = ServiceProvider;
            Common.Lib.ServiceProvider = ServiceProvider;

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
        /// Predetermines which type of list provider to use
        /// </summary>
        /// <returns></returns>
        public static async Task<(AddonListSource, DateTime)> DetermineListProvider()
        {
            IConfiguration configuration = ServiceProvider.GetService<IConfiguration>();
            DateTime minCrawlTime = DateTime.UtcNow - configuration.GetValue<TimeSpan>("repositoryMirrorAddonList:maxAge");
            AddonListSource addonListSource = AddonListSource.RepositoryMirror;

            //Try first with repo mirror service
            AddonListService = ServiceProvider.GetService<Services.RepositoryMirrorAddonListService>();
            DateTime crawlTime = await AddonListService.LoadVersions();

            if (crawlTime < minCrawlTime)
            {
                AddonListService = ServiceProvider.GetService<Services.GithubAddonListService>();
                crawlTime = await AddonListService.LoadVersions();
                addonListSource = AddonListSource.GitHub;
            }

            await AddonListService.Load();
            return (addonListSource, crawlTime);
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
            services.AddSingleton<IAddonGameStateService, Services.AddonGameStateService>();
            services.AddSingleton<IUserConfigService, Services.JsonUserConfigService>();
            services.AddSingleton<IAppUpdaterService, Services.AddonHelperAppUpdateService>();
            services.AddSingleton<Services.GithubAddonListService>();
            services.AddSingleton<Services.RepositoryMirrorAddonListService>();

            services.AddTransient<UI.MainWindow>();
        }
    }
}
