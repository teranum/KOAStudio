namespace KOAStudio.Core.Models;

public class PropertyItem(string Name, string Value, string Desc, PropertyItem.VALUE_TYPE ValueType = PropertyItem.VALUE_TYPE.VALUE_STRING, bool IsValueReadOnly = false)
{
    public enum VALUE_TYPE
    {
        VALUE_STRING,
        VALUE_LONG,
    }

    public string Name { get; } = Name;
    public string Value { get; set; } = Value;
    public string Desc { get; } = Desc;

    public VALUE_TYPE ValueType = ValueType;
    public bool IsValueReadOnly { get; } = IsValueReadOnly;
}

