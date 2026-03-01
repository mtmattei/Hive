namespace Hive.Core.Models;

public class CustomList
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ListType Type { get; set; }
    public string Color { get; set; } = "#1B6B93";
    public int SortOrder { get; set; }

    public ICollection<ListItem> Items { get; set; } = [];
}

public class ListItem
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public CustomList? List { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

public enum ListType
{
    ToDo,
    Grocery
}
