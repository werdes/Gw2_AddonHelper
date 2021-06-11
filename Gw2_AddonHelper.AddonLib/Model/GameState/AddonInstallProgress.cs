using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.GameState
{
    public class AddonInstallProgress : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private AddonInstallAction _addonInstallAction;
        private InstallProgress _installProgress;

        public AddonInstallProgress(AddonInstallAction addonInstallAction)
        {
            _addonInstallAction = addonInstallAction;
        }

        public AddonInstallAction AddonInstallAction
        {
            get => _addonInstallAction;
            set
            {
                _addonInstallAction = value;
                Notify();
            }
        }

        public InstallProgress InstallProgress
        {
            get => _installProgress;
            set
            {
                _installProgress = value;
                Notify();
                Notify(nameof(ShowInstallProgressWaiting));
                Notify(nameof(ShowInstallProgressInProgress));
                Notify(nameof(ShowInstallProgressCompleted));
                Notify(nameof(ShowInstallProgressError));
            }
        }

        public bool ShowInstallProgressWaiting => _installProgress == InstallProgress.Waiting;
        public bool ShowInstallProgressInProgress => _installProgress == InstallProgress.InProgress;
        public bool ShowInstallProgressCompleted => _installProgress == InstallProgress.Completed;
        public bool ShowInstallProgressError => _installProgress == InstallProgress.Error;

    }
}
