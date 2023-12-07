using System.Globalization;
using System.Windows;
using System.Windows.Data;
/* Unmerged change from project 'StockDevControl (net7.0-windows)'
Before:
using System.Windows;
After:
using System.Windows.Data;
*/


namespace KOAStudio.Core.Converters
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class InvertBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool v)
            {
                flag = v;
            }
            else if (value is bool?)
            {
                bool? flag2 = (bool?)value;
                flag = flag2.HasValue && flag2.Value;
            }

            return !flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }

            return false;
        }
    }
}
