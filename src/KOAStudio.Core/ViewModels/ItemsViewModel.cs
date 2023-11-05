using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;

namespace KOAStudio.Core.ViewModels
{
    internal partial class TreeTabData : ObservableObject
    {
        public TreeTabData()
        {
            IconId = 0;
            Text = string.Empty;
        }

        public int IconId { get; set; }
        public string Text { get; set; }

        [ObservableProperty]
        private string _FilterText = string.Empty;

        [ObservableProperty]
        private List<object>? _Items;
    }

    internal partial class ItemsViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        public ItemsViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;

            // 아이템 탭 등록
            WeakReferenceMessenger.Default.Register(this, (MessageHandler<object, SetTabTreesMessageType>)((r, m) =>
            {
                if (m.Items is List<IconText> items)
                {
                    var newTabDatas = new List<TreeTabData>();
                    foreach (var item in items)
                    {
                        newTabDatas.Add(new TreeTabData() { IconId = item.IconId, Text = item.Text });
                        tab_items.Add(null);
                    }
                    this.TabDatas = newTabDatas;
                }
            }));

            // 아이템 업데이트 메시지 등록
            WeakReferenceMessenger.Default.Register<SetTreeItemsMessageType>(this, (r, m) =>
            {
                int TabIndex = m.TabIndex;
                if (TabDatas is null || TabIndex < 0 || TabIndex >= TabDatas.Count)
                    return;

                var newItems = m.Items as List<object>;
                TabDatas[TabIndex].Items = newItems;
                tab_items[TabIndex] = newItems;
            });

        }

        List<List<object>?> tab_items = new List<List<object>?>();

        [ObservableProperty]
        private List<TreeTabData>? _TabDatas;

        [ObservableProperty]
        private int _TabSelectedIndex;

        [ObservableProperty]
        private bool _FilterOnlyNodeChecked;

        private IconTextItem? save_selectedItem;
        [RelayCommand]
        private void TreeView_SelectedItemChanged(IconTextItem? selectedItem)
        {
            if (selectedItem is null) return;
            if (save_selectedItem != selectedItem)
            {
                save_selectedItem = selectedItem;
                _uiRequest.ItemSelectedChanged(TabSelectedIndex, selectedItem);
            }
        }

        [RelayCommand]
        private async Task Filter()
        {
            if (TabDatas == null || TabDatas.Count == 0) return;
            string FilterText = TabDatas[TabSelectedIndex].FilterText;
            if (FilterText.Length == 0)
            {
                TabDatas[TabSelectedIndex].Items = tab_items[TabSelectedIndex];
                return;
            }
            var orglistItems = tab_items[TabSelectedIndex];
            if (orglistItems == null || orglistItems.Count == 0)
            {
                return;
            }

            bool bOnlyNode = FilterOnlyNodeChecked;

            var task = Task.Run(() =>
            {
                List<object> newlistItems = new List<object>();
                foreach (var orgItem in orglistItems)
                {
                    var imagetitle = orgItem as IconTextItem;
                    if (imagetitle != null)
                    {
                        IconTextItem? finded;
                        if (bOnlyNode)
                            finded = FindMatchedItemOnlyNode(imagetitle, FilterText);
                        else
                            finded = FindMatchedItem(imagetitle, FilterText);
                        if (finded != null)
                            newlistItems.Add(finded);
                    }
                }
                return newlistItems;
            });
            TabDatas[TabSelectedIndex].Items = await task.ConfigureAwait(true);
        }

        [RelayCommand]
        private async Task Popup_FilterMode()
        {
            FilterOnlyNodeChecked ^= true;
            await Filter().ConfigureAwait(true);
        }
        // sub functions
        IconTextItem? FindMatchedItemOnlyNode(IconTextItem orgitem, string text)
        {
            IconTextItem? me = null;

            if (orgitem.Items.Count > 0)
            {
                foreach (var childitem in orgitem.Items)
                {
                    if (childitem is IconTextItem imagetitle)
                    {
                        IconTextItem? finded = FindMatchedItemOnlyNode(imagetitle, text);
                        if (finded != null)
                        {
                            if (me == null)
                                me = new IconTextItem(orgitem.IconId, orgitem.Text)
                                {
                                    IsExpanded = true,
                                    IsActived = orgitem.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0,
                                };
                            me.AddChild(finded);
                        }
                    }
                }
            }

            if (me == null)
            {
                if (orgitem.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    me = new IconTextItem(orgitem.IconId, orgitem.Text)
                    {
                        IsExpanded = true,
                        IsActived = true
                    };
                }
            }
            return me;
        }

        IconTextItem? FindMatchedItem(IconTextItem orgitem, string text)
        {
            IconTextItem? me = null;

            if (orgitem.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                me = CopyItem(orgitem);
                me.IsActived = true;
            }

            if (me == null && orgitem.Items.Count > 0)
            {
                foreach (var childitem in orgitem.Items)
                {
                    if (childitem is IconTextItem imagetitle)
                    {
                        IconTextItem? finded = FindMatchedItem(imagetitle, text);
                        if (finded != null)
                        {
                            if (me == null)
                                me = new IconTextItem(orgitem.IconId, orgitem.Text)
                                {
                                    IsExpanded = true
                                };
                            me.AddChild(finded);
                        }
                    }
                }
            }

            return me;
        }

        IconTextItem CopyItem(IconTextItem orgitem)
        {
            var newItem = new IconTextItem(orgitem.IconId, orgitem.Text);
            foreach (var childitem in orgitem.Items)
            {
                if (childitem is IconTextItem item)
                {
                    newItem.AddChild(CopyItem(item));
                }
            }
            return newItem;
        }
    }
}
