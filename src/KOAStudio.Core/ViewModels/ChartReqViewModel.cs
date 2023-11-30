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
        }

        public Func<ChartReqViewModel, string, string>? ExtProcedure;

        public KIND Kind { get; }
        public string Title { get; }
        public Visibility TitleBarVisibility { get; }

        [ObservableProperty] string _selected종목;
        [ObservableProperty] ChartRound _selectedChartRound;
        [ObservableProperty] string _selectedChartInterval;
        [ObservableProperty] string _selectedDataCount;

        [RelayCommand]
        void Action(string action_name) => ExtProcedure?.Invoke(this, action_name);

        public bool NextEnabled { get; set; }

        [ObservableProperty] int _nRqId;
        [ObservableProperty] int _ReceivedDataCount;
        [ObservableProperty] DateTime _receivedTime;
        [ObservableProperty] string _resultText;
        [ObservableProperty] string _codeText;
        [ObservableProperty] string _nextText;


        partial void OnSelectedChartRoundChanged(ChartRound value) => UpdateCodeText();

        partial void OnSelectedChartIntervalChanged(string value) => UpdateCodeText();
        partial void OnSelectedDataCountChanged(string value) => UpdateCodeText();
        partial void OnSelected종목Changed(string value) => UpdateCodeText();

        public bool EnableUpdateCodeText;

        private void UpdateCodeText()
        {
            if (EnableUpdateCodeText == false) return;
            if (ExtProcedure != null)
                CodeText = ExtProcedure(this, string.Empty);
        }
    }
}
