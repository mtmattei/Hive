namespace Hive.Core.Models;

public class Reward
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public int StarCost { get; set; }
    public bool RenewAfterRedeem { get; set; }

    public ICollection<RewardEligibility> EligibleProfiles { get; set; } = [];
}

public class RewardEligibility
{
    public Guid Id { get; set; }
    public Guid RewardId { get; set; }
    public Reward? Reward { get; set; }
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
}

public class RewardRedemption
{
    public Guid Id { get; set; }
    public Guid RewardId { get; set; }
    public Reward? Reward { get; set; }
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public int StarsSpent { get; set; }
    public DateTimeOffset RedeemedAt { get; set; }
}
