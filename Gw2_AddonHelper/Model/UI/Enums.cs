using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.UI
{
    public class Enums
    {
        public enum UiState
        {
            Welcome,
            Loading,
            AddonList,
            Installer,
            InstallerProgress,
            Settings,
            Error,
            Conflicts,
            About,
            AppUpdateAvailable,
            AppUpdateDownloading
        }
    }
}
