﻿using Gw2_AddonHelper.Model.UI;
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
    /// Interaktionslogik für ErrorScreenPane.xaml
    /// </summary>
    public partial class ErrorPane : UiStatePane
    {
        public event RoutedEventHandler RetryClick;
        public event RoutedEventHandler SettingsClick;
        public event RoutedEventHandler AppUpdateClick;

        public ErrorPane()
        {
            InitializeComponent();
            UiState = Enums.UiState.Error;
        }

        protected void OnButtonRetryClick(object sender, RoutedEventArgs e) => RetryClick?.Invoke(sender, e);
        protected void OnButtonSettingsClick(object sender, RoutedEventArgs e) => SettingsClick?.Invoke(sender, e);
        protected void OnButtonAppUpdateClick(object sender, RoutedEventArgs e) => AppUpdateClick?.Invoke(sender, e);
    }
}
