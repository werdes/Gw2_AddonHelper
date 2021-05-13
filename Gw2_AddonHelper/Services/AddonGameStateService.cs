using Gw2_AddonHelper.Custom.YamlDotNet;
using Gw2_AddonHelper.Extensions;
using Gw2_AddonHelper.Model;
using Gw2_AddonHelper.Model.AddonList;
using Gw2_AddonHelper.Model.AddonList.Github;
using Gw2_AddonHelper.Model.GameState;
using Gw2_AddonHelper.Model.UserConfig;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gw2_AddonHelper.Services
{
    public class AddonGameStateService : IAddonGameStateService
    {

        private ILogger _log;
        private IConfiguration _config;
        private IUserConfigService _userConfigService;

        public AddonGameStateService(ILogger<GithubAddonListService> log, IConfiguration configuration, IUserConfigService userConfigService)
        {
            _log = log;
            _config = configuration;
            _userConfigService = userConfigService;
        }

        public AddonInstallation GetAddonInstallation(Addon addon)
        {
            UserConfig userConfig = _userConfigService.GetConfig();
            AddonInstallation addonInstallation = new AddonInstallation(addon);
            try
            {
                addonInstallation.InstallationDirectory = Path.Combine(userConfig.GameLocation.GetDirectory(),
                                                                       _config.GetValue<string>("installation:gamePathAddonsFolder"),
                                                                       addon.AddonId);

                if(Directory.Exists(addonInstallation.InstallationDirectory))
                {

                }
                else
                {
                    addonInstallation.InstallState = InstallState.NotInstalled;
                }
            }
            catch (Exception ex)
            {
                addonInstallation.InstallState = InstallState.Error;
                _log.LogError(ex, $"Determination of install state for [{addon.AddonName}] failed");
            }
            return addonInstallation;
        }
    }
}
