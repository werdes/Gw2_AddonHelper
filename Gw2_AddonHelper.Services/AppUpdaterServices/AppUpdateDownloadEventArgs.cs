using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AppUpdaterServices
{
    public class AppUpdateDownloadEventArgs : EventArgs
    {
        public double Progress { get; set; }

        public AppUpdateDownloadEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}
