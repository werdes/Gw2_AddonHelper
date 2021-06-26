using Gw2_AddonHelper.AddonLib.Model;
using Gw2_AddonHelper.Extensions;
using Gw2_AddonHelper.Model.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
    /// Interaktionslogik für AddonListPane.xaml
    /// </summary>
    public partial class AddonListPane : UiStatePane
    {
        private ILogger<AddonListPane> _log;

        public event EventHandler<UiErrorEventArgs> UiError; 
        public event RoutedEventHandler AboutClick;
        public event RoutedEventHandler AppUpdateClick;
        public event RoutedEventHandler SettingsClick;
        public event RoutedEventHandler AddonsUpdateClick;
        public event EventHandler<AddonEventArgs> AddonOpen;
        public event EventHandler<AddonEventArgs> AddonInstall;
        public event EventHandler<AddonEventArgs> AddonRemove;
        public event EventHandler<AddonEventArgs> AddonEnable;
        public event EventHandler<AddonEventArgs> AddonDisable;

        public AddonListPane()
        {
            _log = App.ServiceProvider.GetService<ILogger<AddonListPane>>();

            InitializeComponent();
            UiState = Enums.UiState.AddonList;
        }

        protected void OnButtonAboutClick(object sender, RoutedEventArgs e) => AboutClick?.Invoke(sender, e);
        protected void OnButtonAppUpdateClick(object sender, RoutedEventArgs e) => AppUpdateClick?.Invoke(sender, e);
        protected void OnButtonSettingsClick(object sender, RoutedEventArgs e) => SettingsClick?.Invoke(sender, e);
        protected void OnButtonUpdateClick(object sender, RoutedEventArgs e) => AddonsUpdateClick?.Invoke(sender, e);
        protected void OnAddonListItemOpen(object sender, AddonEventArgs e) => AddonOpen?.Invoke(sender, e);
        protected void OnAddonListItemInstall(object sender, AddonEventArgs e) => AddonInstall?.Invoke(sender, e);
        protected void OnAddonListItemRemove(object sender, AddonEventArgs e) => AddonRemove?.Invoke(sender, e);
        protected void OnAddonListItemEnable(object sender, AddonEventArgs e) => AddonEnable?.Invoke(sender, e);
        protected void OnAddonListItemDisable(object sender, AddonEventArgs e) => AddonDisable?.Invoke(sender, e);


        /// <summary>
        /// Shortcut to Install State group 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonInstallStateShortcutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                InstallState installState;

                if (e.Source is Button)
                {
                    Button senderButton = (Button)e.Source;
                    string installStateName = senderButton.Tag.ToString();

                    if (Enum.TryParse(installStateName, out installState))
                    {
                        List<GroupItem> items = itemscontrolAddonListAddonItems.GetChildrenOfType<GroupItem>();
                        GroupItem desiredItem = items.Where(x => Enum.Parse<InstallState>(x.Tag.ToString()) == installState).FirstOrDefault();

                        if (desiredItem != null)
                        {
                            ItemsControl itemsControl = (ItemsControl)scrollviewerAddonListAddonItems.Content;
                            var point = desiredItem.TranslatePoint(new Point(), itemsControl);
                            scrollviewerAddonListAddonItems.ScrollToVerticalOffset(point.Y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, $"Finding scroll point");
                UiError?.Invoke(this, new UiErrorEventArgs(ex, Localization.Localization.UncategorizedError));
            }
        }
    }
}
