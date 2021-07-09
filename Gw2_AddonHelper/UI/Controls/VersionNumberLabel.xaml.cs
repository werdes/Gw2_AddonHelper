using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Gw2_AddonHelper.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für VersionNumberLabel.xaml
    /// </summary>
    public partial class VersionNumberLabel : UserControl
    {
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.RegisterAttached(nameof(Version), typeof(Version), typeof(AddonListItem), new PropertyMetadata(new Version()));

        public Version Version
        {
            get => (Version)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public VersionNumberLabel()
        {
            InitializeComponent();
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void OnCopyToClipboardMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(Version.ToString());
        }
    }
}
