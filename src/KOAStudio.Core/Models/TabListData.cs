using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace KOAStudio.Core.ViewModels
{
    internal partial class TabListData(string Title) : ObservableObject
    {
        public string Title { get; } = Title;

        [ObservableProperty]
        private int _ballImage;

        public ObservableCollection<string> Items { get; } = [];
    }
}
