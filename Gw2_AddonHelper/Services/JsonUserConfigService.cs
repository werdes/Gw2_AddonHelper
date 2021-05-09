using Gw2_AddonHelper.Model.UserConfig;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services
{
    public class JsonUserConfigService : IUserConfigService
    {
        public UserConfig Config { get; set; }
        public UserConfig GetConfig() => Config;


        private IConfiguration _configuration;
        private ILogger _log;

        public JsonUserConfigService(ILogger<JsonUserConfigService> log, IConfiguration configuration)
        {
            _configuration = configuration;
            _log = log;

            Config = new UserConfig();
        }

        /// <summary>
        /// Loads the current user configuration
        /// </summary>
        public void Load()
        {
            string configFile = _configuration["userConfig"];

            try
            {
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile, Encoding.UTF8);
                    UserConfig tempConfig = JsonConvert.DeserializeObject<UserConfig>(json);

                    Config = tempConfig;
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Loading configuration from [{configFile}] failed");
            }
        }

        /// <summary>
        /// Stores the current user configuration
        /// </summary>
        public void Store()
        {
            string configFile = _configuration["userConfig"];
            try
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(configFile, json, Encoding.UTF8);

            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing configuration to [{configFile}] failed");
            }
        }
    }
}
