using Gw2_AddonHelper.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gw2_AddonHelper.SelfUpdater
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

            ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;


            string[] args = Environment.GetCommandLineArgs();

            _log.LogInformation($"Selfupdater started in [{Environment.CurrentDirectory}] with:");
            _log.LogObject(args);

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
            lstDirs.Add(Path.GetDirectoryName(config.GetValue<string>("moveOldAssetsTo")));

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
            

            services.AddSingleton(configuration);
            services.AddTransient<UI.MainWindow>();
        }
    }
}
