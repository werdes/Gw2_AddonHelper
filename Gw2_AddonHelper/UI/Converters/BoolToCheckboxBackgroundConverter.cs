using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Gw2_AddonHelper.UI.Converters
{
    public class BoolToCheckboxBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                Color colorGreen = Colors.YellowGreen;
                colorGreen.A = 80;

                Color colorRed = Colors.Red;
                colorRed.A = 80;

                SolidColorBrush brush = new SolidColorBrush(((bool)value) ? colorGreen : colorRed);
                return brush;
            }
            return Brushes.Transparent; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
