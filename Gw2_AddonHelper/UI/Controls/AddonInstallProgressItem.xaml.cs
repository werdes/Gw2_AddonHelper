using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.AddonLib.Model.GameState;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gw2_AddonHelper.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für AddonInstallProgressItem.xaml
    /// </summary>
    public partial class AddonInstallProgressItem : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty AddonInstallProgressProperty =
            DependencyProperty.RegisterAttached(nameof(AddonInstallProgress), typeof(AddonInstallProgress), typeof(AddonInstallProgressItem), new PropertyMetadata(null));

        public AddonInstallProgress AddonInstallProgress
        {
            get => (AddonInstallProgress)GetValue(AddonInstallProgressProperty);
            set => SetValue(AddonInstallProgressProperty, value);
        }

        public AddonInstallProgressItem()
        {
            InitializeComponent();
        }
    }
}
