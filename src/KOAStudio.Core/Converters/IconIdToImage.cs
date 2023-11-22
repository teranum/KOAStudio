using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KOAStudio.Core.Converters
{
    internal class IconIdToImage : IValueConverter
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
                Images = [];
                var m_Png_dead = new BitmapImage(new Uri("pack://application:,,,/KOAStudio.Core;component/Resources/icons.png"));
                int nCY = 20;
                int nCX = 16;
                int nImgCount = m_Png_dead.PixelWidth / nCX;
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
