using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KOAStudio.Core.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToNamedBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? retVals = parameter as string;
            if (retVals != null)
            {
                var true_false = retVals.Split(',');
                if (true_false.Length == 2)
                {
                    var brushConverter = new BrushConverter();
                    return (bool)value ? brushConverter.ConvertFromString(true_false[1]) : brushConverter.ConvertFromString(true_false[0]);
                }
            }
            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
