using System.Windows;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for MessageOkCancel.xaml
    /// </summary>
    public partial class MessageOkCancel : Window
    {
        public MessageOkCancel(string Message, string Caption)
        {
            InitializeComponent();
            MsgText.Text = Message;
            Title = Caption;

            Owner = Application.Current.MainWindow;
            Topmost = Owner.Topmost;
        }

        private void BtnOkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancleClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
