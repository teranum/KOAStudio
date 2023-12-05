using KOAStudio.Core.Models;
using System.Globalization;
using System.Windows.Data;

namespace KOAStudio.Core.Converters
{
    public class StokItemInfoPriceConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double price)
            {
                var item_info = values[1] as StockItemInfo;
                if (item_info != null)
                {
                    return item_info.GetPriceString(price);
                }
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
