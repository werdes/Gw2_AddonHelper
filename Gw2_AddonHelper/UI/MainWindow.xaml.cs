using Gw2_AddonHelper.Model.AddonList;
using Gw2_AddonHelper.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Windows;

namespace Gw2_AddonHelper.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IConfiguration config, ILogger<MainWindow> log, IAddonListService addonListManager)
        {
            InitializeComponent();
            log.LogInformation("eh");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IAddonListService addonListService = App.ServiceProvider.GetService<IAddonListService>();
            List<Addon> lstAddons = await addonListService.GetAddonsAsync();

            addonListService.Store();
        }
    }
}
