using Gw2_AddonHelper.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Gw2_AddonHelper.UI.Controls.Panes
{
    /// <summary>
    /// Interaktionslogik für InstallerPane.xaml
    /// </summary>
    public partial class InstallerPane : UiStatePane
    {
        public event RoutedEventHandler CancelClick;
        public event RoutedEventHandler InstallClick;

        protected void OnButtonInstallerCancelClick(object sender, RoutedEventArgs e) => CancelClick?.Invoke(sender, e);
        protected void OnButtonInstallerInstallClick(object sender, RoutedEventArgs e) => InstallClick?.Invoke(sender, e);
        public InstallerPane()
        {
            InitializeComponent();
            UiState = Enums.UiState.Installer;
        }
    }
}
