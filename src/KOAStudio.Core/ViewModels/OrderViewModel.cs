using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KOAStudio.Core.Models;
using System.Collections.ObjectModel;

namespace KOAStudio.Core.ViewModels
{
    public partial class OrderViewModel(string title, Action<OrderViewModel, string> extCallProc) : ObservableObject
    {
        public bool EnableUpdateCodeText;
        [ObservableProperty] IEnumerable<string>? _계좌리스트;
        [ObservableProperty] string _selected계좌 = string.Empty;
        [ObservableProperty] string _종목코드 = string.Empty;
        [ObservableProperty] string _종목명 = string.Empty;

        public string Title { get; } = title;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(주문가격Enabled))]
        OrderType _매매구분;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(주문가격Enabled))]
        OrderKind _주문종류;
        public bool 원주문매도수구분;
        [ObservableProperty] string _주문번호 = string.Empty;
        [ObservableProperty] string _주문가격 = "0";
        public bool 주문가격Enabled => 매매구분 == OrderType.정정취소 || 주문종류 == OrderKind.지정가;
        [ObservableProperty] int _주문수량 = 1;
        [ObservableProperty] double _현재가;
        [ObservableProperty] string _codeText = string.Empty;
        public bool 주문확인생략 { get; set; }

        [RelayCommand]
        void UpButton(string kind)
        {
            if (kind.Equals("주문수량"))
            {
                주문수량++;
            }
        }
        [RelayCommand]
        void DownButton(string kind)
        {
            if (kind.Equals("주문수량"))
            {
                if (주문수량 > 1) 주문수량--;
            }
        }
        [RelayCommand]
        void ReqAction(string kind) => _extCallProc?.Invoke(this, kind);

        partial void OnSelected계좌Changed(string value) => UpdateCodeText();
        partial void On종목코드Changed(string value) => UpdateCodeText();
        partial void On매매구분Changed(OrderType value) => UpdateCodeText();
        partial void On주문종류Changed(OrderKind value) => UpdateCodeText();
        partial void On주문번호Changed(string value) => UpdateCodeText();
        partial void On주문가격Changed(string value) => UpdateCodeText();
        partial void On주문수량Changed(int value) => UpdateCodeText();

        public void UpdateCodeText()
        {
            if (EnableUpdateCodeText)
                _extCallProc?.Invoke(this, "Update");
        }

        private readonly Action<OrderViewModel, string> _extCallProc = extCallProc;

        public int SelectedTabIndex { get; set; }
        public ObservableCollection<JangoItem> JangoItems { get; } = [];
        public ObservableCollection<MicheItem> MicheItems { get; } = [];

        public JangoItem? SelectedJangoItem { get; set; }
        public MicheItem? SelectedMicheItem { get; set; }
    }
}
