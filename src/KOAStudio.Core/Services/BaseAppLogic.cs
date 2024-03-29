﻿using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;
using System.Windows.Controls;

namespace KOAStudio.Core.Services
{
    public class BaseAppLogic : ILogicNotify
    {
        public void SetMenuCustomize(string headerText, object items)
        {
            WeakReferenceMessenger.Default.Send(new SetMenuCustomizeMessageType(headerText, items));
        }

        public void SetStatusText(string text, bool changedLoginState = false, bool realServer = false)
        {
            WeakReferenceMessenger.Default.Send(new AppStatusChangedMessageType(text, changedLoginState, realServer));
        }

        public void SetResultText(string text, bool bAdd = false)
        {
            WeakReferenceMessenger.Default.Send(new SetResultTextMessageType(text, bAdd));
        }

        public void SetProperties(string headerText, object items)
        {
            WeakReferenceMessenger.Default.Send(new SetPropertiesMessageType(headerText, items));
        }

        public void SetPropertyQueryNextEnable(bool bEnable)
        {
            WeakReferenceMessenger.Default.Send(new QueryNextEnabledMessageType(bEnable));
        }

        public void SetTabLists(object items)
        {
            WeakReferenceMessenger.Default.Send(new SetTabLogsMessageType(items));
        }

        public void OutputLog(int tabIndex, object? content = null, int maxLines = -1, bool focus = false)
        {
            WeakReferenceMessenger.Default.Send(new LogOutputMessageType(tabIndex, content, maxLines, focus));
        }

        public static void OutputLogResetAllChangeState()
        {
            WeakReferenceMessenger.Default.Send(new LogOutputResetAllChangeStateMessageType());
        }

        public void SetTabTrees(object items)
        {
            WeakReferenceMessenger.Default.Send(new SetTabTreesMessageType(items));
        }

        public void SetTreeItems(int tabIndex, object items)
        {
            WeakReferenceMessenger.Default.Send(new SetTreeItemsMessageType(tabIndex, items));
        }

        public void SetUserContent(ContentControl Control)
        {
            WeakReferenceMessenger.Default.Send(new SetUseContentMessageType(Control));
        }
    }
}
