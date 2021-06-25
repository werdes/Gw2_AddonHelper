using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.Interfaces
{
    public interface IAppUpdaterService
    {
        public Task<Version> GetLatestVersion();
        public Task Update();
    }
}
