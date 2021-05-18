using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Github
{
    public class GithubRatelimitService
    {
        private const int MAX_CALLS_PER_HOUR = 60;

        private Queue<DateTime> _callTimes = new Queue<DateTime>();
        private ILogger _log;
        private IConfiguration _config;

        private static Lazy<GithubRatelimitService> _instance = new Lazy<GithubRatelimitService>(() => new GithubRatelimitService());
        public static GithubRatelimitService Instance
        {
            get => _instance.Value;
        }

        private GithubRatelimitService()
        {
            _log = App.ServiceProvider.GetService<ILogger<GithubRatelimitService>>();
            _config = App.ServiceProvider.GetService<IConfiguration>();
            _callTimes = new Queue<DateTime>();

            Load();
            Cleanup();
        }

        /// <summary>
        /// Cleans up expired call times
        /// </summary>
        public void Cleanup()
        {
            DateTime cutOffTime = DateTime.UtcNow.AddHours(-1D);
            while (_callTimes.Count > 0 && _callTimes.Peek() < cutOffTime)
            {
                _callTimes.Dequeue();
            }
        }

        /// <summary>
        /// Returns wether a call can be made
        /// </summary>
        /// <returns></returns>
        public bool CanCall()
        {
            Cleanup();
            return _callTimes.Count < MAX_CALLS_PER_HOUR;
        }

        /// <summary>
        /// Registers a call
        /// </summary>
        public void RegisterCall()
        {
            _callTimes.Enqueue(DateTime.UtcNow);
            Store();
        }

        /// <summary>
        /// Loads the addon list from json file
        /// </summary>
        public void Load()
        {
            string callTimesFile = _config.GetValue<string>("githubRatelimitFile");

            try
            {
                if (File.Exists(callTimesFile))
                {
                    string json = File.ReadAllText(callTimesFile, Encoding.UTF8);
                    Queue<DateTime> tempList = JsonConvert.DeserializeObject<Queue<DateTime>>(json);

                    _callTimes = tempList;
                }
                else
                {
                    _callTimes = new Queue<DateTime>();
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading call times from [{callTimesFile}] failed");
            }
        }

        /// <summary>
        /// Stores the addon list as JSON
        /// </summary>
        public void Store()
        {
            string callTimesFile = _config.GetValue<string>("githubRatelimitFile");

            try
            {
                string json = JsonConvert.SerializeObject(_callTimes, Formatting.Indented);
                File.WriteAllText(callTimesFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing call times to [{callTimesFile}] failed");
            }
        }
    }
}
