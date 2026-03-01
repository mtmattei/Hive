namespace Hive.Core.Models;

public class FamilyAccount
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "America/New_York";
    public string? Address { get; set; }
    public SubscriptionTier Subscription { get; set; } = SubscriptionTier.Free;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Profile> Profiles { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<SharedAccess> SharedAccess { get; set; } = [];
    public CalendarSettings? Settings { get; set; }
}

public enum SubscriptionTier
{
    Free,
    Plus
}
