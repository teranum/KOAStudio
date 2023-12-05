using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KOAStudio.Core.Converters
{
    public class RadioBooleanToNamedBrushConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string? retVals = parameter as string;
            if (retVals != null)
            {
                var true_false = retVals.Split(',');
                if (true_false.Length == values.Length)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] is true)
                            return new BrushConverter().ConvertFromString(true_false[i]);
                    }
                }
            }
            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
