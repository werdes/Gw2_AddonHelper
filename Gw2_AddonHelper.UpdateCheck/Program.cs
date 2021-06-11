using Gw2_AddonHelper.AddonLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.IO;

namespace Gw2_AddonHelper.UpdateCheck
{
    class Program
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            Lib.ServiceProvider = ServiceProvider;
            new Gw2AddonHelperUpdateCheck();
        }


        /// <summary>
        /// Configures IOC
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureServices(IServiceCollection services)
        {
            string env = Environment.GetEnvironmentVariable("Environment");
            services.AddLogging(builder =>
            {
                builder.AddLog4Net($"log.{env}.config");
            });

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{env}.json", false)
                .Build();

            GitHubClient githubClient = new GitHubClient(new ProductHeaderValue(
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()));

            string tokenEnvKey = configuration.GetValue<string>("oauthKeyEnvironmentVariableKey");
            string token = Environment.GetEnvironmentVariable(tokenEnvKey);
            githubClient.Credentials = new Credentials(token);

            services.AddSingleton(configuration);
            services.AddSingleton(githubClient);
        }
    }
}
