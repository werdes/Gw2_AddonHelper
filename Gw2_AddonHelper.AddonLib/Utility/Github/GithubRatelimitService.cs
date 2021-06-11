using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gw2_AddonHelper.AddonLib.Utility.Github
{
    public class GithubRatelimitService
    {
        private int _maxCallsPerHour = 0;
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
            _log = Lib.ServiceProvider.GetService<ILogger<GithubRatelimitService>>();
            _config = Lib.ServiceProvider.GetService<IConfiguration>();
            _callTimes = new Queue<DateTime>();
            _maxCallsPerHour = _config.GetValue<int>("githubRatelimit");


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
            return _callTimes.Count < _maxCallsPerHour;
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
