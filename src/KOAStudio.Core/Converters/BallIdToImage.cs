using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SI_DevCenter.Converters
{
    internal class BallIdToImage : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return GetTreeIcon((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static List<BitmapSource>? Images;
        public static BitmapSource? GetTreeIcon(int nIndex)
        {
            if (Images is null)
            {
                Images = new List<BitmapSource>();
                var m_Png_dead = new BitmapImage(new Uri("pack://application:,,,/KOAStudio.Core;component/Resources/Balls_3.png"));
                int nCY = m_Png_dead.PixelHeight;
                int nImgCount = m_Png_dead.PixelWidth / nCY;
                int nCX = nCY;
                for (int i = 0; i < nImgCount; i++)
                {
                    Images.Add(new CroppedBitmap(m_Png_dead, new System.Windows.Int32Rect(i * nCX, 0, nCX, nCY)));
                }
            }
            if (nIndex >= 0 && nIndex < Images.Count)
                return Images[nIndex];
            return null;
        }
    }

}
