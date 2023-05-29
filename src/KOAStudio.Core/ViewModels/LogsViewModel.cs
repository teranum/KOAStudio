using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace KOAStudio.Core.ViewModels
{
    internal class ListTabData
    {
        public ListTabData()
        {
            Title = string.Empty;
            Items = new ObservableCollection<string>();
        }
        public string Title { get; set; }
        public ObservableCollection<string> Items { get; set; }
    }

    internal partial class LogsViewModel : ObservableObject
    {
        private readonly IUIRequest _uiRequest;
        public LogsViewModel(IUIRequest uiRequest)
        {
            _uiRequest = uiRequest;

            // 로그 탭 등록
            WeakReferenceMessenger.Default.Register(this, (MessageHandler<object, SetTabLogsMessageType>)((r, m) =>
            {
                var items = m.Items as List<string>;
                if (items is not null)
                {
                    var newTabDatas = new List<ListTabData>();
                    foreach (var item in items)
                    {
                        newTabDatas.Add(new ListTabData() { Title = item });
                    }
                    this.TabDatas = newTabDatas;
                }
            }));

            // 로그등록
            WeakReferenceMessenger.Default.Register<LogOutputMessageType>(this, (r, m) =>
            {
                var TabIndex = m.TabIndex;
                if (TabDatas is null || TabIndex < 0 || TabIndex >= TabDatas.Count) return;
                var listBoxData = TabDatas[TabIndex].Items;
                var Content = m.Content;
                if (Content is null)
                    listBoxData.Clear();
                else
                {
                    var text = Content as string;
                    if (text is not null)
                    {
                        string sLine = DateTime.Now.ToString("HH:mm:ss.fff : ");
                        sLine += text;
                        listBoxData.Add(sLine);
                        if (m.MaxLines > 0 && listBoxData.Count > m.MaxLines)
                        {
                            listBoxData.RemoveAt(0);
                        }
                    }
                    else
                    {
                        var lines = Content as IReadOnlyList<string>;
                        if (lines is not null)
                        {
                            foreach (var line in lines)
                            {
                                listBoxData.Add(line);
                            }
                        }
                    }
                }
                if (m.Focus)
                {
                    TabSelectedIndex = TabIndex;
                }
            });
        }

        [ObservableProperty]
        private List<ListTabData>? _TabDatas;

        [ObservableProperty]
        private int _TabSelectedIndex;

        // 복사
        [RelayCommand]
        private void Popup_Copy()
        {
            if (TabDatas is not null)
            {
                var lines = TabDatas[TabSelectedIndex].Items;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string line in lines)
                {
                    stringBuilder.AppendLine(line);
                }
                string sAll = stringBuilder.ToString();
                if (sAll.Length > 0)
                {
                    Clipboard.SetText(sAll);
                }
            }
        }

        // 지우기
        [RelayCommand]
        private void Popup_Clear()
        {
            if (TabDatas is not null)
                TabDatas[TabSelectedIndex].Items.Clear();
        }

        // 실시간 중지
        [RelayCommand]
        private void Popup_Stop_RT()
        {
            _uiRequest.ReqStopRealTime();
        }

        // 리스트 더블클릭
        [RelayCommand]
        private void ListBox_MouseDoubleClick(string Text)
        {
            if (Text is null) return;
            _uiRequest.ReqTRHistory(TabSelectedIndex, Text);
        }
    }
}
