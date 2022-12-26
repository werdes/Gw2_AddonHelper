using Gw2_AddonHelper.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AppUpdaterServices
{
    /// <summary>
    ///  Dummy Updater for having a possible updater if all other methods fail. Will never work, but returns an outdated version 0.0.0.0
    /// </summary>
    internal class DummyAppUpdaterService : IAppUpdaterService
    {
        public int GetHierarchy()
        {
            return int.MaxValue;
        }

        public async Task<Version> GetLatestVersion()
        {
            return new Version(0, 0, 0, 0);
        }

        public async Task<bool> IsAvailable()
        {
            return true;
        }

        public Task Update()
        {
            throw new NotImplementedException();
        }
    }
}
