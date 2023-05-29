namespace KOAStudio.Core.Models;

#if NET

public record AppStatusChangedMessageType(string Text, bool ChangedLoginState, bool IsRealServer);
public record SetResultTextMessageType(string Text, bool IsAdd);
public record SetMenuCustomizeMessageType(string HeaderText, object Items);
public record SetPropertiesMessageType(string Text, object Items);
public record QueryNextEnabledMessageType(bool IsEnable);
public record SetTabLogsMessageType(object Items);
public record LogOutputMessageType(int TabIndex, object? Content, int MaxLines, bool Focus);
public record SetTabTreesMessageType(object Items);
public record SetTreeItemsMessageType(int TabIndex, object? Items);

#else

public record AppStatusChangedMessageType
{
    public string Text;
    public bool ChangedLoginState;
    public bool IsRealServer;

    public AppStatusChangedMessageType(string Text, bool ChangedLoginState, bool IsRealServer)
    {
        this.Text = Text;
        this.ChangedLoginState = ChangedLoginState;
        this.IsRealServer = IsRealServer;
    }
}
public record SetResultTextMessageType
{
    public string Text;
    public bool IsAdd;
    public SetResultTextMessageType(string Text, bool IsAdd)
    {
        this.Text = Text;
        this.IsAdd = IsAdd;
    }
}
public record SetMenuCustomizeMessageType
{
    public string HeaderText;
    public object Items;
    public SetMenuCustomizeMessageType(string HeaderText, object Items)
    {
        this.HeaderText = HeaderText;
        this.Items = Items;
    }
}
public record SetPropertiesMessageType
{
    public string Text;
    public object Items;
    public SetPropertiesMessageType(string Text, object Items)
    {
        this.Text = Text;
        this.Items = Items;
    }
}
public record QueryNextEnabledMessageType
{
    public bool IsEnable;
    public QueryNextEnabledMessageType(bool IsEnable)
    {
        this.IsEnable = IsEnable;
    }
}
public record SetTabLogsMessageType
{
    public object Items;
    public SetTabLogsMessageType(object Items)
    {
        this.Items = Items;
    }
}
public record LogOutputMessageType
{
    public int TabIndex;
    public object? Content;
    public int MaxLines;
    public bool Focus;
    public LogOutputMessageType(int TabIndex, object? Content, int MaxLines, bool Focus)
    {
        this.TabIndex = TabIndex;
        this.Content = Content;
        this.MaxLines = MaxLines;
        this.Focus = Focus;
    }
}
public record SetTabTreesMessageType
{
    public object Items;
    public SetTabTreesMessageType(object Items)
    {
        this.Items = Items;
    }
}
public record SetTreeItemsMessageType
{
    public int TabIndex;
    public object? Items;
    public SetTreeItemsMessageType(int TabIndex, object? Items)
    {
        this.TabIndex = TabIndex;
        this.Items = Items;
    }
}

#endif