using KOAStudio.Core.Models;
using System.Globalization;
using System.Windows.Data;

namespace KOAStudio.Core.Converters
{
    internal class HasChartRoundIntervalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChartRound chartRound)
            {
                return chartRound == ChartRound.틱 || chartRound == ChartRound.분;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
