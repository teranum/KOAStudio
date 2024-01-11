using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using KOAStudio.Core.Views;
using System.Windows.Controls;

namespace KOAStudio.Core.ViewModels
{
    internal partial class KOAWindowViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        private readonly string _baseTitle;
        private readonly string _appVersion;
        private IList<GithubTagInfo>? _releaseTags;

        public KOAWindowViewModel(IUIRequest uiRequest)
        {
            var assemblyName = System.Windows.Application.ResourceAssembly.GetName();
            _appVersion = $"{assemblyName.Version!.Major}.{assemblyName.Version.Minor}";
            _baseTitle = $"{assemblyName.Name} v{_appVersion} - {(Environment.Is64BitProcess ? "64비트" : "32비트")}";

            _uiRequest = uiRequest;
            _title = _baseTitle;

            _menuCustomizeHeaderText = "Custom";

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

            _ = CheckVersionAsync();
        }

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _menuCustomizeHeaderText;

        [ObservableProperty]
        private List<string>? _menuCustomizeItems;

        [ObservableProperty]
        private string _statusText = "준비됨";

        [ObservableProperty]
        private string _statusUrl = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private string _resultText = string.Empty;
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

        [RelayCommand]
        private void Closed()
        {
            _uiRequest.Close();
        }

        [RelayCommand]
        static void Hyperlink_RequestNavigate(Uri url)
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo(url.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }

        private async Task CheckVersionAsync()
        {
            // 깃헙에서 최신 버전 정보 가져오기

            _releaseTags = await GithubVersion.GetRepoTagInfos("teranum", "KOAStudio").ConfigureAwait(true);
            if (_releaseTags != null && _releaseTags.Count > 0)
            {
                var lastTag = _releaseTags[0];
                if (string.Equals(lastTag.tag_name, _appVersion))
                {
                    StatusText = "최신 버전입니다.";
                }
                else
                {
                    StatusUrl = lastTag.html_url;
                    StatusText = $"새로운 버전({lastTag.tag_name})이 있습니다.";
                }
            }
        }

        [RelayCommand]
        void Menu_Version()
        {
            // 버젼 정보
            if (_releaseTags != null && _releaseTags.Count != 0)
            {
                var versionView = new VersionView(_releaseTags);
                versionView.ShowDialog();
            }
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
