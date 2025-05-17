using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;

namespace KOAStudio.Core.ViewModels
{

    internal partial class ItemsViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        public ItemsViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;

            // 아이템 탭 등록
            WeakReferenceMessenger.Default.Register(this, (MessageHandler<object, SetTabTreesMessageType>)((r, m) =>
            {
                if (m.Items is List<IdText> items)
                {
                    var newTabDatas = new List<TabTreeData>();
                    foreach (var item in items)
                    {
                        newTabDatas.Add(new TabTreeData(item.Id, item.Text));
                        _tab_items.Add(null);
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
                _tab_items[TabIndex] = newItems;
            });

        }

        private readonly List<List<object>?> _tab_items = [];

        [ObservableProperty]
        public partial List<TabTreeData>? TabDatas { get; set; }

        [ObservableProperty]
        public partial int TabSelectedIndex { get; set; }
        [ObservableProperty]
        public partial bool FilterOnlyNodeChecked { get; set; }

        private IdTextItem? _save_selectedItem;
        [RelayCommand]
        private void TreeView_SelectedItemChanged(IdTextItem? selectedItem)
        {
            if (selectedItem is null) return;
            if (_save_selectedItem != selectedItem)
            {
                _save_selectedItem = selectedItem;
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
                TabDatas[TabSelectedIndex].Items = _tab_items[TabSelectedIndex];
                return;
            }
            var orglistItems = _tab_items[TabSelectedIndex];
            if (orglistItems == null || orglistItems.Count == 0)
            {
                return;
            }

            bool bOnlyNode = FilterOnlyNodeChecked;

            var task = Task.Run(() =>
            {
                List<object> newlistItems = [];
                foreach (var orgItem in orglistItems)
                {
                    if (orgItem is IdTextItem imagetitle)
                    {
                        IdTextItem? finded;
                        if (bOnlyNode)
                            finded = ItemsViewModel.FindMatchedItemOnlyNode(imagetitle, FilterText);
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
        static IdTextItem? FindMatchedItemOnlyNode(IdTextItem orgitem, string text)
        {
            IdTextItem? me = null;

            if (orgitem.Items.Count > 0)
            {
                foreach (var childitem in orgitem.Items)
                {
                    if (childitem is IdTextItem imagetitle)
                    {
                        IdTextItem? finded = ItemsViewModel.FindMatchedItemOnlyNode(imagetitle, text);
                        if (finded != null)
                        {
                            me ??= new IdTextItem(orgitem.Id, orgitem.Text)
                            {
                                IsExpanded = true,
                                IsActived = orgitem.Text.Contains(text, StringComparison.OrdinalIgnoreCase),
                            };
                            me.AddChild(finded);
                        }
                    }
                }
            }

            if (me == null)
            {
                if (orgitem.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    me = new IdTextItem(orgitem.Id, orgitem.Text)
                    {
                        IsExpanded = true,
                        IsActived = true,
                    };
                }
            }
            return me;
        }

        static IdTextItem? FindMatchedItem(IdTextItem orgitem, string text)
        {
            IdTextItem? me = null;

            if (orgitem.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                me = ItemsViewModel.CopyItem(orgitem);
                me.IsActived = true;
            }

            if (me == null && orgitem.Items.Count > 0)
            {
                foreach (var childitem in orgitem.Items)
                {
                    if (childitem is IdTextItem imagetitle)
                    {
                        IdTextItem? finded = FindMatchedItem(imagetitle, text);
                        if (finded != null)
                        {
                            me ??= new IdTextItem(orgitem.Id, orgitem.Text)
                            {
                                IsExpanded = true,
                            };
                            me.AddChild(finded);
                        }
                    }
                }
            }

            return me;
        }

        static IdTextItem CopyItem(IdTextItem orgitem)
        {
            var newItem = new IdTextItem(orgitem.Id, orgitem.Text);
            foreach (var childitem in orgitem.Items)
            {
                if (childitem is IdTextItem item)
                {
                    newItem.AddChild(ItemsViewModel.CopyItem(item));
                }
            }
            return newItem;
        }
    }
}
