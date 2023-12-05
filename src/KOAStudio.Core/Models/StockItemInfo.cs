#pragma warning disable MA0096,MA0097
using CommunityToolkit.Mvvm.ComponentModel;

namespace KOAStudio.Core.Models
{
    public partial class StockItemInfo : ObservableObject, IComparable<StockItemInfo>
    {
        public string 종목코드 { get; }
        public string 종목명 { get; }
        public double 틱사이즈 { get; }
        public double 계약크기 { get; } // 1포인트당 달러 수익/손실 가격

        public string 통화 { get; }

        public StockItemInfo(string 종목코드, string 종목명, double 틱사이즈, double 계약크기, int 소수점개수, string 통화)
        {
            this.종목코드 = 종목코드;
            this.종목명 = 종목명;
            this.틱사이즈 = 틱사이즈;
            this.계약크기 = 계약크기;
            this.통화 = 통화;

            소수점비율 = Math.Pow(10, -소수점개수);
            _fmtPrice = "{0:N" + 소수점개수 + "}";
        }
        public double 소수점비율 { get; }
        public double 손익계산(int 계약수, double 상승분)
        {
            return 상승분 * 계약크기 * 계약수;
        }

        public override string ToString() => $"[{종목코드}]  {종목명}";

        public int CompareTo(StockItemInfo? other)
        {
            if (other == null)
            // ...and y is null, x is greater.
            {
                return 1;
            }
            else
            {
                // ...and y is not null, compare the
                // lengths of the two strings.
                //
                //int retval = 종목코드.Length.CompareTo(other.종목코드.Length);

                //if (retval != 0)
                //{
                //    // If the strings are not of equal length,
                //    // the longer string is greater.
                //    //
                //    return retval;
                //}
                //else
                {
                    // If the strings are of equal length,
                    // sort them with ordinary string comparison.
                    //
                    return 종목코드.CompareTo(other.종목코드);
                }
            }
        }


        // Dynamic infomations

        [ObservableProperty]
        double _현재가;

        public double 시가, 고가, 저가, 전일가, 누적거래량;
        public double 체결시간, 체결량;
        //public ChegyolType 체결구분;

        private string _fmtPrice;
        public string GetPriceString(double price)
        {
            return string.Format(_fmtPrice, price);
        }
    }
}
