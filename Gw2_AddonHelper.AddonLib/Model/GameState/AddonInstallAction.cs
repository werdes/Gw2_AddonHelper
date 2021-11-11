using Gw2_AddonHelper.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.GameState
{
    public class AddonInstallAction : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private AddonContainer _addonContainer;
        private InstallerActionType _installerActionType;

        public AddonInstallAction(AddonContainer addonContainer, InstallerActionType installerActionType)
        {
            _addonContainer = addonContainer;
            _installerActionType = installerActionType;
        }

        public AddonContainer AddonContainer
        {
            get => _addonContainer;
            set
            {
                _addonContainer = value;
                Notify();
            }
        }

        public InstallerActionType InstallActionType
        {
            get => _installerActionType;
            set
            {
                _installerActionType = value;
                Notify();
            }
        }
    }
}
