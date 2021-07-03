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
    /// Interaktionslogik für WelcomePane.xaml
    /// </summary>
    public partial class WelcomePane : UiStatePane
    {
        public event RoutedEventHandler FindExecutableClick;
        public event RoutedEventHandler ContinueClick;

        public WelcomePane()
        {
            InitializeComponent();
            UiState = Enums.UiState.Welcome;
        }

        protected void OnButtonSettingsFindExecutableClick(object sender, RoutedEventArgs e) => FindExecutableClick?.Invoke(sender, e);
        protected void OnButtonContinueClick(object sender, RoutedEventArgs e) => ContinueClick?.Invoke(sender, e);
    }
}
