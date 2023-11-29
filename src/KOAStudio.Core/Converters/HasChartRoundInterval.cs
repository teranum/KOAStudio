using KOAStudio.Core.Models;
using System.Globalization;
using System.Windows.Data;

namespace KOAStudio.Core.Converters
{
    internal class HasChartRoundInterval : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChartRound chartRound)
            {
                return chartRound == ChartRound.일 || chartRound == ChartRound.주 || chartRound == ChartRound.월;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
