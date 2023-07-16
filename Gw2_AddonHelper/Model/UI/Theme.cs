using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gw2_AddonHelper.Model.UI
{
    public class Theme : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _backgroundUri;
        private decimal _backgroundOpacity;
        private Stretch _backgroundStretch;
        private Color _accentColor;
        private Color _windowTintColor;


        public string BackgroundUri
        {
            get => _backgroundUri;
            set
            {
                _backgroundUri = value;
                Notify();
            }
        }


        public decimal BackgroundOpacity
        {
            get => _backgroundOpacity;
            set
            {
                _backgroundOpacity = value;
                Notify();
            }
        }
        public Stretch BackgroundStretch
        {
            get => _backgroundStretch;
            set
            {
                _backgroundStretch = value;
                Notify();
            }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                Notify();
            }
        }

        public Color WindowTintColor
        {
            get => _windowTintColor;
            set
            {
                _windowTintColor = value;
                Notify();
            }
        }

    }
}
