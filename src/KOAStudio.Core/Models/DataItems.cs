namespace KOAStudio.Core.Models;

public enum OpenApiLoginState
{
    None,
    ApiCreateFailed,
    LoginProcess,
    LoginFailed,
    LoginSucceed,
    LoginOuted,
};

public class IconText(int iconId, string text)
{
    public int IconId { get; set; } = iconId;

    public string Text { get; set; } = text;
}

public class IconTextItem(int imageId, string text) : IconText(imageId, text)
{
    public IconTextItem? Parent;
    public IList<object> Items { get; } = new List<object>();
    public void AddChild(IconTextItem item)
    {
        item.Parent = this;
        Items.Add(item);
    }

    public bool IsExpanded { get; set; }
    public bool IsSelected { get; set; }
    public bool IsActived { get; set; }
}

public class PropertyItem(string Name, string Value, string Desc, PropertyItem.VALUE_TYPE type = PropertyItem.VALUE_TYPE.VALUE_STRING, bool IsValueReadOnly = false)
{
    public enum VALUE_TYPE
    {
        VALUE_STRING,
        VALUE_LONG,
    }

    public string Name { get; } = Name;
    public string Value { get; set; } = Value;
    public string Desc { get; } = Desc;

    public VALUE_TYPE type = type;
    public bool IsValueReadOnly { get; } = IsValueReadOnly;
}

