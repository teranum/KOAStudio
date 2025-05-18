using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KOAStudio.Core.Models;
using System.Windows;

namespace KOAStudio.Core.ViewModels
{
    public partial class CharDataReqViewModel : ObservableObject
    {
        public enum KIND
        {
            업종,
            주식,
            선물,
            옵션,
        }
        public CharDataReqViewModel(KIND Kind, string Title)
        {
            this.Kind = Kind;
            this.Title = Title;
            TitleBarVisibility = this.Title.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            SelectedChartRound = ChartRound.일;
            SelectedChartInterval_분 = "1";
            SelectedChartInterval_틱 = "100";
            ResultText = string.Empty;
            CodeText = string.Empty;
            NextText = string.Empty;
            Selected종목 = string.Empty;
            조회일자 = DateTime.Now;
            Is수정주가 = null;
        }

        public Func<CharDataReqViewModel, string, Task>? ExtProcedure;

        public KIND Kind { get; }
        public string Title { get; }
        public Visibility TitleBarVisibility { get; }

        [ObservableProperty]
        public partial string Selected종목 { get; set; }

        [ObservableProperty]
        public partial DateTime 조회일자 { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedChartInterval))]
        public partial ChartRound SelectedChartRound { get; set; }

        public string SelectedChartInterval
        {
            get => SelectedChartRound switch
            {
                ChartRound.분 => SelectedChartInterval_분,
                ChartRound.틱 => SelectedChartInterval_틱,
                _ => "1",
            };
            set
            {
                switch (SelectedChartRound)
                {
                    case ChartRound.일:
                    case ChartRound.주:
                    case ChartRound.월:
                        break;
                    case ChartRound.분:
                        SelectedChartInterval_분 = value;
                        UpdateCodeText();
                        break;
                    case ChartRound.틱:
                        SelectedChartInterval_틱 = value;
                        UpdateCodeText();
                        break;
                    default:
                        break;
                }
            }
        }
        public string SelectedChartInterval_분;
        public string SelectedChartInterval_틱;

        [RelayCommand]
        async Task Action(string action_name)
        {
            if (ExtProcedure != null)
            {
                await ExtProcedure.Invoke(this, action_name);
            }
        }

        [ObservableProperty]
        public partial bool NextEnabled { get; set; }

        [ObservableProperty]
        public partial int ReceivedDataCount { get; set; }

        [ObservableProperty]
        public partial DateTime RequestTime { get; set; }

        [ObservableProperty]
        public partial double Elapsed_ms { get; set; }

        [ObservableProperty]
        public partial string ResultText { get; set; }

        [ObservableProperty]
        public partial string CodeText { get; set; }

        [ObservableProperty]
        public partial string NextText { get; set; }

        [ObservableProperty]
        public partial bool? Is수정주가 { get; set; }

        partial void OnSelectedChartRoundChanged(ChartRound value) => UpdateCodeText();
        partial void OnSelected종목Changed(string value) => UpdateCodeText();
        partial void On조회일자Changed(DateTime value) => UpdateCodeText();
        partial void OnIs수정주가Changed(bool? value) => UpdateCodeText();

        public bool EnableUpdateCodeText;
        public void UpdateCodeText()
        {
            if (!EnableUpdateCodeText) return;
            _ = ExtProcedure?.Invoke(this, string.Empty);
        }
    }
}
