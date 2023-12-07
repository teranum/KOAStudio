namespace KOAStudio.Core.Models
{
    public class JangoItem(string 종목코드, string 종목명, bool 매도수구분, int 보유수량, double 평균단가, double 현재가, double 평가손익, string 통화코드)
    {
        public string 종목코드 { get; } = 종목코드;
        public string 종목명 { get; } = 종목명;

        /// <summary>
        /// true: 매도, false: 매수
        /// </summary>
        public bool 매도수구분 { get; set; } = 매도수구분;
        public int 보유수량 { get; set; } = 보유수량;
        public double 평균단가 { get; set; } = 평균단가;
        public double 현재가 { get; set; } = 현재가;
        public double 평가손익 { get; protected set; } = 평가손익;
        public string 통화코드 { get; } = 통화코드;
    }
}
