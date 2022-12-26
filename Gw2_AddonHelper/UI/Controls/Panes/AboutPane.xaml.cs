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
    /// Interaktionslogik für AboutPane.xaml
    /// </summary>
    public partial class AboutPane : UiStatePane
    {
        public event RoutedEventHandler BackClick;
        public event MouseButtonEventHandler LegalNoticeIconsClick;
        public event MouseButtonEventHandler BugreportsClick;

        public AboutPane()
        {
            InitializeComponent();
            UiState = Enums.UiState.About;
        }

        protected void OnButtonAboutBackClick(object sender, RoutedEventArgs e) => BackClick?.Invoke(sender, e);
        protected void OnTextBlockLegalNoticeIconsMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => LegalNoticeIconsClick?.Invoke(sender, e);
        protected void OnTextBlockBugreportsMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => BugreportsClick?.Invoke(sender, e);

    }
}
