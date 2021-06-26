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
    /// Interaktionslogik für InstallerProgressPane.xaml
    /// </summary>
    public partial class InstallerProgressPane : UiStatePane
    {
        public event RoutedEventHandler CancelClick;

        public InstallerProgressPane()
        {
            InitializeComponent();
            UiState = Enums.UiState.InstallerProgress;
        }

        protected void OnButtonInstallerCancelClick(object sender, RoutedEventArgs e) => CancelClick?.Invoke(sender, e);
    }
}
