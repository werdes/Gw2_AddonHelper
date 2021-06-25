using Gw2_AddonHelper.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Gw2_AddonHelper.UI.Controls
{
    public class UiStatePane : ContentControl
    {
        public static readonly DependencyProperty UiStateProperty =
            DependencyProperty.RegisterAttached(nameof(UiState), typeof(Enums.UiState), typeof(UiStatePane), new PropertyMetadata(Enums.UiState.Loading));

        public Enums.UiState UiState
        {
            get => (Enums.UiState)GetValue(UiStateProperty);
            set => SetValue(UiStateProperty, value);
        }
    }
}
