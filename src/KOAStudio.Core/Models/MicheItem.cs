namespace KOAStudio.Core.Models
{
    public class MicheItem(string 종목코드, string 종목명, string 주문번호, string 원주문번호, bool 매도수구분, int 주문수량, int 미체결수량, double 주문가격, string 통화코드, string 주문시각)
    {
        public string 종목코드 { get; } = 종목코드;
        public string 종목명 { get; } = 종목명;
        public string 주문번호 { get; } = 주문번호;
        public string 원주문번호 { get; set; } = 원주문번호;
        /// <summary>
        /// true: 매도, false: 매수
        /// </summary>
        public bool 매도수구분 { get; } = 매도수구분;

        public int 주문수량 { get; set; } = 주문수량;
        public int 미체결수량 { get; set; } = 미체결수량;
        public double 주문가격 { get; set; } = 주문가격;
        public string 통화코드 { get; } = 통화코드;
        public string 주문시각 { get; set; } = 주문시각;
    }
}
