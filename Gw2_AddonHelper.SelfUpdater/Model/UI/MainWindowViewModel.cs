using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.SelfUpdater.Model.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int _maxValue;
        private int _value;
        private bool _waiting;

        public MainWindowViewModel()
        {
            _waiting = true;
            _maxValue = 1;
            _value = 0;
        }

        public double Progress => (double)Value / MaxValue;
        public string ProgressText
        {
            get
            {
                if (_waiting) return "Waiting...";
                return $"{(Progress * 100):n2} %";
            }
        }

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                Notify();
                Notify(nameof(Progress));
                Notify(nameof(ProgressText));
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                Notify();
                Notify(nameof(Progress));
                Notify(nameof(ProgressText));
            }
        }

        public bool Waiting
        {
            get => _waiting;
            set
            {
                _waiting = value;
                Notify();
                Notify(nameof(ProgressText));
            }
        }

    }
}
