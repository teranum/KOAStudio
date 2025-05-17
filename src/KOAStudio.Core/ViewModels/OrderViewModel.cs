using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KOAStudio.Core.Models;
using System.Collections.ObjectModel;

namespace KOAStudio.Core.ViewModels
{
    public partial class OrderViewModel(string title, Func<OrderViewModel, string, Task> extCallProc) : ObservableObject
    {
        public bool EnableUpdateCodeText;
        [ObservableProperty]
        public partial IEnumerable<string>? 계좌리스트 { get; set; }

        [ObservableProperty]
        public partial string Selected계좌 { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string 종목코드 { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string 종목명 { get; set; } = string.Empty;
        public string Title { get; } = title;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(주문가격Enabled))]
        public partial OrderType 매매구분 { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(주문가격Enabled))]
        public partial OrderKind 주문종류 { get; set; }

        public bool 원주문매도수구분;
        [ObservableProperty]
        public partial string 주문번호 { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string 주문가격 { get; set; } = "0";

        public bool 주문가격Enabled => 매매구분 == OrderType.정정취소 || 주문종류 == OrderKind.지정가;
        [ObservableProperty]
        public partial int 주문수량 { get; set; } = 1;

        [ObservableProperty]
        public partial double 현재가 { get; set; }

        [ObservableProperty]
        public partial string CodeText { get; set; } = string.Empty;
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
        async Task ReqAction(string kind) => await _extCallProc(this, kind);

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
                _ = _extCallProc(this, "Update");
        }

        private readonly Func<OrderViewModel, string, Task> _extCallProc = extCallProc;

        public int SelectedTabIndex { get; set; }
        public ObservableCollection<JangoItem> JangoItems { get; } = [];
        public ObservableCollection<MicheItem> MicheItems { get; } = [];

        public JangoItem? SelectedJangoItem { get; set; }
        public MicheItem? SelectedMicheItem { get; set; }
    }
}
