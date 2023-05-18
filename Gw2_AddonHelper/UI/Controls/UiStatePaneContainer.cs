using Gw2_AddonHelper.Model.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Gw2_AddonHelper.UI.Controls
{
    [ContentProperty(nameof(Panes))]
    public class UiStatePaneContainer : ContentControl
    {
        public static readonly DependencyProperty PanesProperty = DependencyProperty.RegisterAttached(
            nameof(Panes), typeof(List<UiStatePane>), typeof(UiStatePaneContainer), new PropertyMetadata(new List<UiStatePane>()));

        public static readonly DependencyProperty UiStateProperty =
            DependencyProperty.RegisterAttached(nameof(UiState), typeof(Enums.UiState), typeof(UiStatePaneContainer), new PropertyMetadata(Enums.UiState.Loading));

        /// <summary>
        /// Binding to UiState from Viewmodel
        /// > Change monitoring will handle Pane switching
        /// </summary>
        public Enums.UiState UiState
        {
            get => (Enums.UiState)GetValue(UiStateProperty);
            set => SetValue(UiStateProperty, value);
        }

        public List<UiStatePane> Panes
        {
            get => (List<UiStatePane>)GetValue(PanesProperty);
            set => SetValue(PanesProperty, value);
        }

        public UiStatePaneContainer()
        {
            DependencyPropertyDescriptor panesDescriptor = DependencyPropertyDescriptor.FromProperty(UiStateProperty, typeof(UiStatePaneContainer));
            panesDescriptor.AddValueChanged(this, new EventHandler(OnUiStateChanged));
        }

        /// <summary>
        /// Handler for the DProp AppUpdateDownloadValue Changed event, sets visibility of the appropriate pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUiStateChanged(object sender, EventArgs e)
        {
            foreach(UiStatePane pane in Panes)
            {
                bool visible = pane.UiState == this.UiState;
                pane.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
