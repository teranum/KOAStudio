using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KOAStudio.Core.Converters
{
    internal class NullCheckVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
