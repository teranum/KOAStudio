using KOAStudio.Core.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for VersionView.xaml
    /// </summary>
    public partial class VersionView : Window
    {
        public IList<GithubTagInfo> TagInfos { get; }

        public VersionView(IList<GithubTagInfo> tagInfos)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            TagInfos = tagInfos;

            this.DataContext = this;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var sInfo = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            Process.Start(sInfo);
            e.Handled = true;
        }
    }
}
