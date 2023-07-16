using Gw2_AddonHelper.Model.UI;
using Gw2_AddonHelper.Services.UserConfigServices.Model;
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
    /// Interaktionslogik für SettingsPane.xaml
    /// </summary>
    public partial class SettingsPane : UiStatePane
    {
        public event RoutedEventHandler SaveClick;
        public event RoutedEventHandler CancelClick;
        public event RoutedEventHandler FindExecutableClick;
        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        public SettingsPane()
        {
            InitializeComponent();
            UiState = Enums.UiState.Settings;
        }

        protected void OnButtonSettingsSaveClick(object sender, RoutedEventArgs e) => SaveClick?.Invoke(sender, e);
        protected void OnButtonSettingsCancelClick(object sender, RoutedEventArgs e) => CancelClick?.Invoke(sender, e);
        protected void OnButtonSettingsFindExecutableClick(object sender, RoutedEventArgs e) => FindExecutableClick?.Invoke(sender, e);
        private void OnThemeSelectorThemeChanged(object sender, ThemeChangedEventArgs e) => ThemeChanged?.Invoke(sender, e);
    }
}
