namespace KOAStudio.Core.Models;

public class IdText(int Id, string text)
{
    public int Id { get; set; } = Id;

    public string Text { get; set; } = text;
}

