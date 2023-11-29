using System.Windows;
using System.Windows.Input;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for KOAWindow.xaml
    /// </summary>
    public partial class KOAWindow : Window
    {
        public KOAWindow()
        {
            InitializeComponent();
            MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };
        }
    }
}
