using Gw2_AddonHelper.UI.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für MultiLanguageLabel.xaml
    /// </summary>
    public partial class MultiLanguageLabel : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public const string DEFAULT_LANGUAGE_CODE = "eng";

        private IConfiguration _config;
        private string _englishVersion;
        private List<string> _otherVersions;

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.RegisterAttached(nameof(Key), typeof(string), typeof(MultiLanguageLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty MainOrientationProperty =
            DependencyProperty.RegisterAttached(nameof(MainOrientation), typeof(Orientation), typeof(MultiLanguageLabel), new PropertyMetadata(Orientation.Horizontal));
        public static readonly DependencyProperty OtherVersionsOrientationProperty =
            DependencyProperty.RegisterAttached(nameof(OtherVersionsOrientation), typeof(Orientation), typeof(MultiLanguageLabel), new PropertyMetadata(Orientation.Horizontal));
        public static readonly DependencyProperty OtherVersionsPaddingProperty =
           DependencyProperty.RegisterAttached(nameof(OtherVersionsPadding), typeof(Thickness), typeof(MultiLanguageLabel), new PropertyMetadata(new Thickness(0, 0, 0, 0)));
        public static readonly DependencyProperty OtherVersionsMarginProperty =
           DependencyProperty.RegisterAttached(nameof(OtherVersionsMargin), typeof(Thickness), typeof(MultiLanguageLabel), new PropertyMetadata(new Thickness(0, 0, 0, 0)));
        public static readonly DependencyProperty ReducedFontSizeFactorProperty =
           DependencyProperty.RegisterAttached(nameof(ReducedFontSizeFactor), typeof(double), typeof(MultiLanguageLabel), new PropertyMetadata(0.75D));



        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public Orientation MainOrientation
        {
            get => (Orientation)GetValue(MainOrientationProperty);
            set => SetValue(MainOrientationProperty, value);
        }

        public Orientation OtherVersionsOrientation
        {
            get => (Orientation)GetValue(OtherVersionsOrientationProperty);
            set => SetValue(OtherVersionsOrientationProperty, value);
        }

        public Thickness OtherVersionsPadding
        {
            get => (Thickness)GetValue(OtherVersionsPaddingProperty);
            set => SetValue(OtherVersionsPaddingProperty, value);
        }

        public Thickness OtherVersionsMargin
        {
            get => (Thickness)GetValue(OtherVersionsMarginProperty);
            set => SetValue(OtherVersionsMarginProperty, value);
        }

        public double ReducedFontSizeFactor
        {
            get => (double)GetValue(ReducedFontSizeFactorProperty);
            set => SetValue(ReducedFontSizeFactorProperty, value);
        }


        public List<string> OtherVersions
        {
            get => _otherVersions;
            set
            {
                _otherVersions = value;
                Notify();
            }
        }


        public string EnglishVersion
        {
            get => _englishVersion;
            set
            {
                _englishVersion = value;
                Notify();
            }
        }

        public double ReducedFontSize { get => (FontSize * ReducedFontSizeFactor); }

        public MultiLanguageLabel()
        {
            InitializeComponent();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _config = App.ServiceProvider.GetService<IConfiguration>();
            }
            catch { }

            bool showInvariantCulture = _config?.GetValue<bool>("showInvariantCulture") ?? false;
            Dictionary<string, string> languageVersions = LocalizationProvider.GetLocalized(Key, showInvariantCulture);

            EnglishVersion = languageVersions[DEFAULT_LANGUAGE_CODE];
            OtherVersions = languageVersions.Where(x => x.Key != DEFAULT_LANGUAGE_CODE).Select(x => x.Value).ToList();
        }
    }
}
