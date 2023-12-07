using KOAStudio.Core.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KOAStudio.Core.Converters
{
    public class OrderTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderType orderType)
            {
                return orderType switch
                {
                    OrderType.매수 => Brushes.PaleVioletRed,
                    OrderType.매도 => Brushes.LightSkyBlue,
                    OrderType.정정취소 => Brushes.LightGoldenrodYellow,
                    _ => throw new NotSupportedException(),
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
