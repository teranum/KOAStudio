using CommunityToolkit.Mvvm.Messaging;
using KOAStudio.Core.Models;

namespace KOAStudio.Business
{
    internal sealed partial class BusinessLogic
    {
        // Implements ILogicNotify interface

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

        public void SetTabTrees(object items)
        {
            WeakReferenceMessenger.Default.Send(new SetTabTreesMessageType(items));
        }

        public void SetTreeItems(int tabIndex, object items)
        {
            WeakReferenceMessenger.Default.Send(new SetTreeItemsMessageType(tabIndex, items));
        }
    }
}
