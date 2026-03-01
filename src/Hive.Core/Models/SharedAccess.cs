namespace Hive.Core.Models;

public class SharedAccess
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public FamilyAccount? FamilyAccount { get; set; }
    public string InviteeEmail { get; set; } = string.Empty;
    public AccessRole Role { get; set; }
    public DateTimeOffset InvitedAt { get; set; }
    public bool Accepted { get; set; }
}

public enum AccessRole
{
    Admin,
    Member,
    Viewer
}
