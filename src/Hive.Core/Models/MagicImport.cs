namespace Hive.Core.Models;

/// <summary>
/// Represents a magic import request — forwarded email, PDF, or photo
/// that gets parsed into calendar events automatically.
/// </summary>
public class MagicImport
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public ImportSourceType SourceType { get; set; }
    public string? SourceContent { get; set; }
    public string? FileName { get; set; }
    public ImportStatus Status { get; set; }
    public int EventsCreated { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public enum ImportSourceType
{
    Email,
    Pdf,
    Photo
}

public enum ImportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
