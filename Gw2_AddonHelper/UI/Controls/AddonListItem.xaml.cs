using Gw2_AddonHelper.AddonLib.Model.GameState;
using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Model.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Gw2_AddonHelper.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für AddonListItem.xaml
    /// </summary>
    public partial class AddonListItem : UserControl, INotifyPropertyChanged
    {
        private ILogger<AddonListItem> _log;


        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty AddonContainerProperty =
            DependencyProperty.RegisterAttached(nameof(AddonContainer), typeof(AddonContainer), typeof(AddonListItem), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowActionsProperty =
            DependencyProperty.RegisterAttached(nameof(ShowActions), typeof(bool), typeof(AddonListItem), new PropertyMetadata(false));
        public static readonly DependencyProperty ShowCheckboxProperty =
            DependencyProperty.RegisterAttached(nameof(ShowCheckbox), typeof(bool), typeof(AddonListItem), new PropertyMetadata(false));
        
        public event EventHandler<AddonEventArgs> Open;
        public event EventHandler<AddonEventArgs> Install;
        public event EventHandler<AddonEventArgs> Remove;
        public event EventHandler<AddonEventArgs> Enable;
        public event EventHandler<AddonEventArgs> Disable;

        public AddonContainer AddonContainer
        {
            get => (AddonContainer)GetValue(AddonContainerProperty);
            set => SetValue(AddonContainerProperty, value);
        }

        public bool ShowActions
        {
            get => (bool)GetValue(ShowActionsProperty);
            set => SetValue(ShowActionsProperty, value);
        }

        public bool ShowCheckbox
        {
            get => (bool)GetValue(ShowCheckboxProperty);
            set => SetValue(ShowCheckboxProperty, value);
        }

        public AddonListItem()
        {
            InitializeComponent();

            _log = App.ServiceProvider.GetService<ILogger<AddonListItem>>();
        }

        /// <summary>
        /// On "Open"-Button click -> Fire "Open" event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonOpenClick(object sender, RoutedEventArgs e)
        {
            Open?.Invoke(this, new AddonEventArgs(AddonContainer));
        }

        /// <summary>
        /// On "Install"-Button click -> Fire "Install" event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonInstallClick(object sender, RoutedEventArgs e)
        {
            Install?.Invoke(this, new AddonEventArgs(AddonContainer));
        }

        /// <summary>
        /// On "Remove"-Button click -> Fire "Remove" event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonRemoveClick(object sender, RoutedEventArgs e)
        {
            Remove?.Invoke(this, new AddonEventArgs(AddonContainer));
        }

        /// <summary>
        /// On "Enable"-Button click -> Fire "Enable" event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonEnableClick(object sender, RoutedEventArgs e)
        {
            Enable?.Invoke(this, new AddonEventArgs(AddonContainer));
        }

        /// <summary>
        /// On "Disable"-Button click -> Fire "Disable" event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonDisableClick(object sender, RoutedEventArgs e)
        {
            Disable?.Invoke(this, new AddonEventArgs(AddonContainer));
        }

        /// <summary>
        /// Toggle the "checkbox"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonCheckClick(object sender, RoutedEventArgs e)
        {
            AddonContainer.Checked = !AddonContainer.Checked;
        }

        /// <summary>
        /// Open the addon website
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImageDeveloperLinkMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                AddonContainer?.Addon?.Website?.OpenWeb();
            }
            catch(Exception ex)
            {
                _log.LogError(ex, $"Opening link for [{AddonContainer.Addon.AddonId}]");
            }
        } 
    }
}
