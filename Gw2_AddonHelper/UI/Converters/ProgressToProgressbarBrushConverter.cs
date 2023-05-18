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
    public class ProgressToProgressbarBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double)
            {
                double progress = (double)value;
                Color color = Colors.YellowGreen;
                color.A = 50;

                GradientStopCollection gradientStops = new GradientStopCollection();
                gradientStops.Add(new GradientStop(color, 0));
                gradientStops.Add(new GradientStop(color, progress));
                gradientStops.Add(new GradientStop(Colors.Transparent, progress));
                gradientStops.Add(new GradientStop(Colors.Transparent, 1));

                Brush brush = new LinearGradientBrush(gradientStops, 0);
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
