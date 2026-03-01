namespace Hive.Core.Models;

public class PhotoAlbum
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Photo> Photos { get; set; } = [];
}

public class Photo
{
    public Guid Id { get; set; }
    public Guid AlbumId { get; set; }
    public PhotoAlbum? Album { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}
