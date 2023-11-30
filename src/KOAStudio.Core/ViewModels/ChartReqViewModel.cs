using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KOAStudio.Core.Models;
using System.Windows;

namespace KOAStudio.Core.ViewModels
{
    public partial class ChartReqViewModel : ObservableObject
    {
        public enum KIND
        {
            업종,
            주식,
            선물,
            옵션,
        }
        public ChartReqViewModel(KIND Kind, string Title)
        {
            this.Kind = Kind;
            this.Title = Title;
            TitleBarVisibility = this.Title.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

            _selectedChartRound = ChartRound.일;
            _selectedChartInterval = string.Empty;

            _selectedChartInterval = "1";
            _selectedDataCount = "100";

            _resultText = string.Empty;
            _codeText = string.Empty;
            _nextText = string.Empty;
            _selected종목 = string.Empty;

            조회일자 = DateTime.Now;
            _is수정주가 = null;
        }

        public Func<ChartReqViewModel, string, string>? ExtProcedure;

        public KIND Kind { get; }
        public string Title { get; }
        public Visibility TitleBarVisibility { get; }

        [ObservableProperty] string _selected종목;
        [ObservableProperty] DateTime _조회일자;
        [ObservableProperty] ChartRound _selectedChartRound;
        [ObservableProperty] string _selectedChartInterval;
        [ObservableProperty] string _selectedDataCount;

        [RelayCommand]
        void Action(string action_name)
        {
            if (ExtProcedure != null)
            {
                string result = ExtProcedure.Invoke(this, action_name);
                CodeText += result;
            }
        }

        [ObservableProperty] bool _nextEnabled;

        [ObservableProperty] int _nRqId;
        [ObservableProperty] int _receivedDataCount;
        [ObservableProperty] DateTime _receivedTime;
        [ObservableProperty] string _resultText;
        [ObservableProperty] string _codeText;
        [ObservableProperty] string _nextText;
        [ObservableProperty] bool? _is수정주가;


        partial void OnSelectedChartRoundChanged(ChartRound value) => UpdateCodeText();
        partial void OnSelectedChartIntervalChanged(string value) => UpdateCodeText();
        partial void OnSelectedDataCountChanged(string value) => UpdateCodeText();
        partial void OnSelected종목Changed(string value) => UpdateCodeText();
        partial void On조회일자Changed(DateTime value) => UpdateCodeText();
        partial void OnIs수정주가Changed(bool? value) => UpdateCodeText();

        public bool EnableUpdateCodeText;
        public void UpdateCodeText()
        {
            if (!EnableUpdateCodeText) return;
            if (ExtProcedure != null)
                CodeText = ExtProcedure(this, string.Empty);
        }
    }
}
