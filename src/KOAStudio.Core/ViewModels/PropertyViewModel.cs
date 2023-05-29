using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using System.Collections.Generic;

namespace KOAStudio.Core.ViewModels
{
    internal partial class PropertyViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        public PropertyViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;
            _Items = new List<PropertyItem>()
            {
                //new PropertyItem("입력값1", "...", "입력값을 설정해 주세요."),
                //new PropertyItem("입력값2", "...", "입력값을 설정해 주세요."),
                //new PropertyItem("입력값3", "...", "입력값을 설정해 주세요."),
            };
            _HeaderText = "요청설정";

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
        private bool _QueryNextEnabled;

        [ObservableProperty]
        private string _HeaderText;

        [ObservableProperty]
        private List<PropertyItem> _Items;
    }
}
