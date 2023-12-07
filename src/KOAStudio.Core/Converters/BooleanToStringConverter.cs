using System.Globalization;
using System.Windows.Data;

namespace KOAStudio.Core.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
            {
                if (parameter is string retVals)
                {
                    var true_false = retVals.Split(',');
                    if (true_false.Length == 2)
                    {
                        return bValue ? true_false[1] : true_false[0];
                    }
                }
            }
            return "Invalid";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
