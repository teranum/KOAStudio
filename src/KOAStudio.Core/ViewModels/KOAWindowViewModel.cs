using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;

namespace KOAStudio.Core.ViewModels
{
    internal partial class KOAWindowViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        private readonly string _BaseTitle =
            System.Windows.Application.ResourceAssembly.GetName().Name
            + (Environment.Is64BitProcess ? " - 64비트" : " - 32비트");

        public KOAWindowViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;
            _Title = _BaseTitle;

            _MenuCustomizeHeaderText = "Custom";
            _SearchText = "";
            _ResultText = "";
            _StatusText = "준비됨";

            WeakReferenceMessenger.Default.Register<AppStatusChangedMessageType>(this, (r, m) =>
            {
                StatusText = m.Text;
                if (m.ChangedLoginState)
                {
                    MenuLoginCommand.NotifyCanExecuteChanged();
                    MenuLogoutCommand.NotifyCanExecuteChanged();
                    if (_uiRequest.LoginState == OpenApiLoginState.LoginSucceed)
                    {
                        Title = _BaseTitle + (m.IsRealServer ? " (실투자)" : " (모의투자)");
                    }
                    else
                    {
                        Title = _BaseTitle;
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<SetResultTextMessageType>(this, (r, m) =>
            {
                if (m.IsAdd)
                    ResultText += m.Text;
                else
                    ResultText = m.Text;
            });

            WeakReferenceMessenger.Default.Register<SetMenuCustomizeMessageType>(this, (r, m) =>
            {
                MenuCustomizeHeaderText = m.HeaderText;
                var lists = m.Items as List<string>;
                if (lists is not null)
                {
                    MenuCustomizeItems = lists;
                }
            });
        }

        [ObservableProperty]
        private string _Title;

        [ObservableProperty]
        private string _MenuCustomizeHeaderText;

        [ObservableProperty]
        private List<string>? _MenuCustomizeItems;

        [ObservableProperty]
        private string _StatusText;

        [ObservableProperty]
        private string _SearchText;

        [ObservableProperty]
        private string _ResultText;

        [RelayCommand]
        private void Loaded()
        {
            _uiRequest.Initialize();
        }

        [RelayCommand(CanExecute = nameof(CanMenuLogin))]
        private void MenuLogin()
        {
            _uiRequest.ReqApiLogin(true);
        }

        private bool CanMenuLogin()
        {
            OpenApiLoginState state = _uiRequest.LoginState;
            return (state == OpenApiLoginState.None) || (state == OpenApiLoginState.LoginOuted);
        }

        [RelayCommand(CanExecute = nameof(CanMenuLogout))]
        private void MenuLogout()
        {
            _uiRequest.ReqApiLogin(false);
        }

        private bool CanMenuLogout()
        {
            OpenApiLoginState state = _uiRequest.LoginState;
            return state == OpenApiLoginState.LoginSucceed;
        }

        [RelayCommand]
        private void MenuExit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        [RelayCommand]
        private void MenuCustomize(string menuText)
        {
            _uiRequest.MenuCustomizeAction(menuText);
        }
    }
}
