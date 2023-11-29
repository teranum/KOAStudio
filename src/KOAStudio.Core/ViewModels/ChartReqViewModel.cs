using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KOAStudio.Core.Models;
using System.Windows;

namespace KOAStudio.Core.ViewModels
{
    public partial class ChartReqViewModel : ObservableObject
    {
        public ChartReqViewModel(string TagName)
        {
            Title = TagName;
            TitleBarVisibility = Title.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

            _selectedChartRound = ChartRound.일;
            _selectedChartInterval = string.Empty;

            _selectedChartInterval = "1";
            _selectedDataCount = "100";

            _resultText = string.Empty;
            _codeText = string.Empty;
            _selected종목 = string.Empty;
        }
        public Action<ChartReqViewModel, string>? ChartReqCommand;

        public Func<ChartReqViewModel, string>? MakeChartReqCode;
        public string Title { get; }
        public Visibility TitleBarVisibility { get; }

        [ObservableProperty] string _selected종목;
        [ObservableProperty] ChartRound _selectedChartRound;
        [ObservableProperty] string _selectedChartInterval;
        [ObservableProperty] string _selectedDataCount;

        [RelayCommand]
        void Action(string action_name) => ChartReqCommand?.Invoke(this, action_name);

        public bool NextEnabled { get; set; }

        [ObservableProperty] int _nRqId;
        [ObservableProperty] int _ReceivedDataCount;
        [ObservableProperty] DateTime _receivedTime;
        [ObservableProperty] string _resultText;
        [ObservableProperty] string _codeText;


        partial void OnSelectedChartRoundChanged(ChartRound value) => UpdateCodeText();

        partial void OnSelectedChartIntervalChanged(string value) => UpdateCodeText();
        partial void OnSelectedDataCountChanged(string value) => UpdateCodeText();
        partial void OnSelected종목Changed(string value) => UpdateCodeText();

        public bool EnableUpdateCodeText;
        private void UpdateCodeText()
        {
            if (EnableUpdateCodeText == false) return;
            if (MakeChartReqCode != null)
                CodeText = MakeChartReqCode(this);
        }
    }
}
