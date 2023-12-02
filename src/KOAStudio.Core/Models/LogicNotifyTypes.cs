using System.Windows.Controls;

namespace KOAStudio.Core.Models;

public class AppStatusChangedMessageType(string Text, bool ChangedLoginState, bool IsRealServer)
{
    public string Text = Text;
    public bool ChangedLoginState = ChangedLoginState;
    public bool IsRealServer = IsRealServer;
}
public class SetResultTextMessageType(string Text, bool IsAdd)
{
    public string Text = Text;
    public bool IsAdd = IsAdd;
}
public class SetMenuCustomizeMessageType(string HeaderText, object Items)
{
    public string HeaderText = HeaderText;
    public object Items = Items;
}
public class SetPropertiesMessageType(string Text, object Items)
{
    public string Text = Text;
    public object Items = Items;
}
public class QueryNextEnabledMessageType(bool IsEnable)
{
    public bool IsEnable = IsEnable;
}
public class SetTabLogsMessageType(object Items)
{
    public object Items = Items;
}
public class LogOutputMessageType(int TabIndex, object? Content, int MaxLines, bool Focus)
{
    public int TabIndex = TabIndex;
    public object? Content = Content;
    public int MaxLines = MaxLines;
    public bool Focus = Focus;
}
public class SetTabTreesMessageType(object Items)
{
    public object Items = Items;
}
public class SetTreeItemsMessageType(int TabIndex, object? Items)
{
    public int TabIndex = TabIndex;
    public object? Items = Items;
}
public class SetUseContentMessageType(ContentControl Control)
{
    public ContentControl Control = Control;
}
public record LogOutputResetAllChangeStateMessageType();
