using Gw2_AddonHelper.Common.Extensions;
using Gw2_AddonHelper.Common.Model;
using Gw2_AddonHelper.Services.UserConfigServices.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaktionslogik für ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ObservableCollection<UiTheme> _allThemes;

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;


        //public static readonly DependencyProperty SelectedThemeProperty =
        //    DependencyProperty.RegisterAttached(nameof(SelectedTheme), typeof(UiTheme), typeof(ThemeSelector), new PropertyMetadata(UiTheme.Undefined));
        
        //public UiTheme SelectedTheme
        //{
        //    get => (UiTheme)GetValue(SelectedThemeProperty);
        //    set => SetValue(SelectedThemeProperty, value);
        //}


        public ObservableCollection<UiTheme> AllThemes
        {
            get => _allThemes;
            set
            {
                _allThemes = value;
                Notify();
            }
        }

        public ThemeSelector()
        {
            InitializeComponent();
            _allThemes = new ObservableCollection<UiTheme>();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            AllThemes.Clear();

            UiTheme[] uiThemes = Enum.GetValues<UiTheme>().Where(x => x != UiTheme.Undefined).ToArray();
            AllThemes.AddRange(uiThemes);
        }

        private void OnThemeClicked(object sender, MouseButtonEventArgs e) => ThemeChanged?.Invoke(this, new ThemeChangedEventArgs((UiTheme)((Image)sender).Tag));
    }
}
