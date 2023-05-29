using KOAStudio.Core.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for ItemsView.xaml
    /// </summary>
    public partial class ItemsView : UserControl
    {
        public ItemsView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                treeView.ItemContainerGenerator.ContainerFromItemRecursive(e.NewValue)?.BringIntoView();
            }
        }
    }
}
