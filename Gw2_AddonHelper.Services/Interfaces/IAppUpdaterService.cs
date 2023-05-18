using Gw2_AddonHelper.Services.AppUpdaterServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAppUpdaterService
    {
        public event EventHandler<AppUpdateDownloadEventArgs> UpdateProgress; 
        public Task<(Version, string)> GetLatestVersion();
        public Task Update();
        public Task<bool> IsAvailable();
        int GetHierarchy();
    }
}
