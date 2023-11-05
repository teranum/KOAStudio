using System.Windows.Controls;

namespace KOAStudio.Core.Helpers
{
    public static class ClassExtended
    {
#if !NET
        public static string[] Split(this string _this, char separator, StringSplitOptions options)
        {
            return _this.Split(new char[] { separator }, options);
        }
        public static string[] Split(this string _this, string separator, StringSplitOptions options)
        {
            return _this.Split(new string[] { separator }, options);
        }
#endif
        public static TreeViewItem? ContainerFromItemRecursive(this ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem is not null)
                return treeViewItem;
            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
                if (search is not null)
                    return search;
            }
            return null;
        }
    }
}
