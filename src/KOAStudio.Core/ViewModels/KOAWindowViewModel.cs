using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using System.Windows.Controls;

namespace KOAStudio.Core.ViewModels
{
    internal partial class KOAWindowViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        private readonly string _baseTitle =
            System.Windows.Application.ResourceAssembly.GetName().Name
            + (Environment.Is64BitProcess ? " - 64비트" : " - 32비트");

        public KOAWindowViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;
            _title = _baseTitle;

            _menuCustomizeHeaderText = "Custom";
            _searchText = string.Empty;
            _resultText = string.Empty;
            _statusText = "준비됨";

            WeakReferenceMessenger.Default.Register<AppStatusChangedMessageType>(this, (r, m) =>
            {
                StatusText = m.Text;
                if (m.ChangedLoginState)
                {
                    MenuLoginCommand.NotifyCanExecuteChanged();
                    MenuLogoutCommand.NotifyCanExecuteChanged();
                    if (_uiRequest.LoginState == OpenApiLoginState.LoginSucceed)
                    {
                        Title = _baseTitle + (m.IsRealServer ? " (실투자)" : " (모의투자)");
                    }
                    else
                    {
                        Title = _baseTitle;
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

            WeakReferenceMessenger.Default.Register<SetUseContentMessageType>(this, (r, m) =>
            {
                UserContent = m.Control;
            });
        }

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _menuCustomizeHeaderText;

        [ObservableProperty]
        private List<string>? _menuCustomizeItems;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private string _searchText;

        private string _resultText;
        public string ResultText
        {
            get => _resultText;
            set
            {
                UserContent = null;
                SetProperty(ref _resultText, value);
            }
        }

        private ContentControl? _userContent;
        public ContentControl? UserContent
        {
            get => _userContent;
            set
            {
                if (_userContent != value)
                {
                    if (_userContent is IUserTool userTool)
                        userTool.CloseTool();

                    _userContent = value;
                    OnPropertyChanged(nameof(UserContent));
                }
            }
        }

        [RelayCommand]
        private void Loaded()
        {
            _uiRequest.Initialize();
        }

        [RelayCommand(CanExecute = nameof(CanMenuLogin))]
        private void MenuLogin()
        {
            _uiRequest.ReqApiLogin(bLogin: true);
        }

        private bool CanMenuLogin()
        {
            OpenApiLoginState state = _uiRequest.LoginState;
            return (state == OpenApiLoginState.None) || (state == OpenApiLoginState.LoginOuted);
        }

        [RelayCommand(CanExecute = nameof(CanMenuLogout))]
        private void MenuLogout()
        {
            _uiRequest.ReqApiLogin(bLogin: false);
        }

        private bool CanMenuLogout()
        {
            OpenApiLoginState state = _uiRequest.LoginState;
            return state == OpenApiLoginState.LoginSucceed;
        }

        [RelayCommand]
        private static void MenuExit()
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
