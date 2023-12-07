using System.Globalization;
using System.Windows.Data;

namespace KOAStudio.Core.Converters
{
    internal class EqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolval && boolval)
                return parameter;
            return Binding.DoNothing;
        }
    }
}
