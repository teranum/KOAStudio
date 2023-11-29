using CommunityToolkit.Mvvm.ComponentModel;

namespace KOAStudio.Core.ViewModels
{
    internal partial class TabTreeData(int Id, string Title) : ObservableObject
    {
        public int Id { get; } = Id;
        public string Title { get; } = Title;

        public string FilterText { get; set; } = string.Empty;

        [ObservableProperty]
        private List<object>? _items;
    }
}
