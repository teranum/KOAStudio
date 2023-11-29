namespace KOAStudio.Core.Models;

public class IdTextItem(int imageId, string text) : IdText(imageId, text)
{
    public IdTextItem? Parent;
    public IList<object> Items { get; } = new List<object>();
    public void AddChild(IdTextItem item)
    {
        item.Parent = this;
        Items.Add(item);
    }

    public bool IsExpanded { get; set; }
    public bool IsSelected { get; set; }
    public bool IsActived { get; set; }
}

