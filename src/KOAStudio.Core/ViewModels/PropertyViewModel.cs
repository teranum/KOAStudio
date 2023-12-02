using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;

namespace KOAStudio.Core.ViewModels
{
    internal partial class PropertyViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        public PropertyViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;
            _items = [];
            _headerText = "요청설정";

            WeakReferenceMessenger.Default.Register<QueryNextEnabledMessageType>(this, (r, m) =>
            {
                QueryNextEnabled = m.IsEnable;
            });

            WeakReferenceMessenger.Default.Register<SetPropertiesMessageType>(this, (r, m) =>
            {
                QueryNextEnabled = false;
                HeaderText = m.Text ?? string.Empty;
                Items.Clear();
                var listItems = m.Items as List<PropertyItem>;
                if (listItems is not null)
                    Items = listItems;
            });
        }

        [RelayCommand]
        void Query(bool bNext)
        {
            QueryNextEnabled = false;
            _uiRequest.QueryApiAction(HeaderText, Items, bNext);
        }

        [ObservableProperty]
        private bool _queryNextEnabled;

        [ObservableProperty]
        private string _headerText;

        [ObservableProperty]
        private List<PropertyItem> _items;
    }
}
