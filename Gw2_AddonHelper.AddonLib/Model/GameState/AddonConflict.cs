using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.GameState
{
    public class AddonConflict : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private AddonContainer _installedAddon;
        private AddonContainer _newAddon;

        public AddonConflict(AddonContainer installedAddon, AddonContainer newAddon)
        {
            _installedAddon = installedAddon;
            _newAddon = newAddon;
        }

        public AddonContainer NewAddon
        {
            get => _newAddon;
            set
            {
                _newAddon = value;
                Notify();
            }
        }

        public AddonContainer InstalledAddon
        {
            get => _installedAddon;
            set
            {
                _installedAddon = value; ;
                Notify();
            }
        }
    }
}
