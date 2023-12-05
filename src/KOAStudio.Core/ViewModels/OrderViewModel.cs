using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KOAStudio.Core.Models;

namespace KOAStudio.Core.ViewModels
{
    public partial class OrderViewModel : ObservableObject
    {
        private StockItemInfo? _stockItemInfo;
        public bool EnableUpdateCodeText;
        public OrderViewModel(string modelName, IEnumerable<string> 계좌리스트, Func<OrderViewModel, string, string> extCallProc)
        {

            ModelName = modelName;
            this.계좌리스트 = 계좌리스트;
            ExtCallProc = extCallProc;

            _selected계좌 = string.Empty;
            _종목코드 = string.Empty;
            _종목명 = string.Empty;
            _주문번호 = string.Empty;
        }

        public IEnumerable<string> 계좌리스트 { get; }
        [ObservableProperty] string _selected계좌;
        [ObservableProperty] string _종목코드;
        [ObservableProperty] string _종목명;

        public string ModelName { get; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(주문가격Enabled))]       
        OrderType _매매구분;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(주문가격Enabled))]
        OrderKind _주문종류;
        [ObservableProperty] string _주문번호;
        [ObservableProperty] double _주문가격;
        public bool 주문가격Enabled => 매매구분 == OrderType.정정취소 || 주문종류 == OrderKind.지정가;
        [ObservableProperty] int _주문수량;
        [ObservableProperty] double _현재가;
        public bool 주문확인생략 { get; set; }

        [RelayCommand] void Set현재가() => 주문가격 = 현재가;
        [RelayCommand]
        void UpButton(string kind)
        {
            if (kind.Equals("주문가격"))
            {
                if (_stockItemInfo == null) return;
                주문가격 += _stockItemInfo.틱사이즈;
            }
            else if (kind.Equals("주문수량"))
            {
                주문수량++;
            }
        }
        [RelayCommand]
        void DownButton(string kind)
        {
            if (kind.Equals("주문가격"))
            {
                if (_stockItemInfo == null) return;
                if (주문가격 > _stockItemInfo.틱사이즈) 주문가격 -= _stockItemInfo.틱사이즈;
            }
            else if (kind.Equals("주문수량"))
            {
                if (주문수량 > 1) 주문수량--;
            }
        }
        [RelayCommand]
        void Order(string kind)
        {
            if (kind.Equals("매수주문"))
            {
            }
            else if (kind.Equals("매수주문"))
            {
            }
            else if (kind.Equals("정정주문"))
            {
            }
            else if (kind.Equals("취소주문"))
            {
            }
        }

        partial void On주문종류Changed(OrderKind value)
        {

        }

        public void UpdateCodeText()
        {
            if (EnableUpdateCodeText)
                ExtCallProc?.Invoke(this, "Update");
        }

        Func<OrderViewModel, string, string> ExtCallProc;
    }
}
