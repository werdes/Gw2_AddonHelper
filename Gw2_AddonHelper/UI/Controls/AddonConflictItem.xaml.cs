using Gw2_AddonHelper.AddonLib.Model.GameState;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Gw2_AddonHelper.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für AddonConflictItem.xaml
    /// </summary>
    public partial class AddonConflictItem : UserControl
    {
        public static readonly DependencyProperty AddonConflictProperty =
            DependencyProperty.RegisterAttached(nameof(AddonConflict), typeof(AddonConflict), typeof(AddonConflictItem), new PropertyMetadata(null));

        public AddonConflict AddonConflict
        {
            get => (AddonConflict)GetValue(AddonConflictProperty);
            set => SetValue(AddonConflictProperty, value);
        }

        public AddonConflictItem()
        {
            InitializeComponent();
        }
    }
}
