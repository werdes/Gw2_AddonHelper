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
    /// Interaktionslogik für LoadingPane.xaml
    /// </summary>
    public partial class AppUpdateAvailablePane : UiStatePane
    {
        public event RoutedEventHandler AppUpdateClick;
        public event RoutedEventHandler SkipClick;

        public AppUpdateAvailablePane()
        {
            InitializeComponent();
            UiState = Enums.UiState.AppUpdateAvailable;
        }

        protected void OnButtonAppUpdateClick(object sender, RoutedEventArgs e) => AppUpdateClick?.Invoke(sender, e);
        protected void OnButtonSkipClick(object sender, RoutedEventArgs e) => SkipClick?.Invoke(sender, e);

    }
}
