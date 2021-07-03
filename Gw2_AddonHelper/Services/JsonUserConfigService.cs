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
        private UserConfig _userConfig;
        public UserConfig GetConfig() => _userConfig;


        private IConfiguration _configuration;
        private ILogger _log;

        public JsonUserConfigService(ILogger<JsonUserConfigService> log, IConfiguration configuration)
        {
            _userConfig = new UserConfig();
            _configuration = configuration;
            _log = log;
        }

        /// <summary>
        /// Loads the current user configuration
        /// </summary>
        public void Load()
        {
            string configFile = _configuration["userConfig"];

            try
            {
                _log.LogInformation($"Loading user config from [{configFile}]");
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile, Encoding.UTF8);
                    UserConfig tempConfig = JsonConvert.DeserializeObject<UserConfig>(json);

                    _userConfig.Load(tempConfig);
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
                _log.LogInformation($"Storing user config to [{configFile}]");
                string json = JsonConvert.SerializeObject(_userConfig, Formatting.Indented);
                File.WriteAllText(configFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Storing configuration to [{configFile}] failed");
            }
        }
    }
}
