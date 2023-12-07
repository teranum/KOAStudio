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
            if (value is bool bValue)
            {
                if (parameter is string retVals)
                {
                    var true_false = retVals.Split(',');
                    if (true_false.Length == 2)
                    {
                        var brushConverter = new BrushConverter();
                        return bValue ? brushConverter.ConvertFromString(true_false[1]) : brushConverter.ConvertFromString(true_false[0]);
                    }
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
