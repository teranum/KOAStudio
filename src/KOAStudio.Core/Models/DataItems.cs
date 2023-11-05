using System.Collections.Generic;

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

public class IconText
{
    public IconText(int iconId, string text)
    {
        Text = text;
        IconId = iconId;
    }
    public int IconId { get; set; }

    public string Text { get; set; }
}

public class IconTextItem : IconText
{
    public IconTextItem(int imageId, string text) : base(imageId, text)
    {
        Items = new List<object>();
    }

    public IconTextItem? Parent;
    public IList<object> Items { get; }
    public void AddChild(IconTextItem item)
    {
        item.Parent = this;
        Items.Add(item);
    }

    public bool IsExpanded { get; set; }
    public bool IsSelected { get; set; }
    public bool IsActived { get; set; }
}

public class PropertyItem
{
    public enum VALUE_TYPE
    {
        VALUE_STRING,
        VALUE_LONG
    }

    public PropertyItem(string Name, string Value, string Desc, VALUE_TYPE type = VALUE_TYPE.VALUE_STRING, bool IsValueReadOnly = false)
    {
        this.Name = Name;
        this.Value = Value;
        this.Desc = Desc;
        this.type = type;
        this.IsValueReadOnly = IsValueReadOnly;
    }
    public string Name { get; }
    public string Value { get; set; }
    public string Desc { get; }

    public VALUE_TYPE type;
    public bool IsValueReadOnly { get; }
}

