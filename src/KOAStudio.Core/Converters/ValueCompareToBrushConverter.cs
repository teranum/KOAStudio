using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KOAStudio.Core.Converters
{
    public class ValueCompareToBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is double compValue)
            {
                if (compValue > 0)
                    return Brushes.Red;

                if (compValue < 0)
                    return Brushes.Blue;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

