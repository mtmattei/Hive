using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class RewardService : IRewardService
{
    private readonly HiveDbContext _db;
    private readonly IProfileService _profileService;

    public RewardService(HiveDbContext db, IProfileService profileService)
    {
        _db = db;
        _profileService = profileService;
    }

    public async Task<IReadOnlyList<Reward>> GetRewardsAsync(
        Guid familyAccountId, Guid? profileId = null, CancellationToken ct = default)
    {
        var query = _db.Rewards
            .Include(r => r.EligibleProfiles).ThenInclude(ep => ep.Profile)
            .Where(r => r.FamilyAccountId == familyAccountId);

        if (profileId.HasValue)
        {
            query = query.Where(r => r.EligibleProfiles.Any(ep => ep.ProfileId == profileId.Value));
        }

        return await query.ToListAsync(ct);
    }

    public async Task<Reward> CreateRewardAsync(Reward reward, CancellationToken ct = default)
    {
        reward.Id = reward.Id == Guid.Empty ? Guid.NewGuid() : reward.Id;
        _db.Rewards.Add(reward);
        await _db.SaveChangesAsync(ct);
        return reward;
    }

    public async Task<Reward> UpdateRewardAsync(Reward reward, CancellationToken ct = default)
    {
        _db.Rewards.Update(reward);
        await _db.SaveChangesAsync(ct);
        return reward;
    }

    public async Task DeleteRewardAsync(Guid rewardId, CancellationToken ct = default)
    {
        var reward = await _db.Rewards.FindAsync([rewardId], ct);
        if (reward is not null)
        {
            _db.Rewards.Remove(reward);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<int> GetStarBalanceAsync(Guid profileId, CancellationToken ct = default)
    {
        var profile = await _profileService.GetProfileByIdAsync(profileId, ct)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");
        return profile.StarBalance;
    }

    public async Task<RewardRedemption> RedeemRewardAsync(
        Guid rewardId, Guid profileId, CancellationToken ct = default)
    {
        var reward = await _db.Rewards.FindAsync([rewardId], ct)
            ?? throw new InvalidOperationException($"Reward {rewardId} not found");

        var profile = await _db.Profiles.FindAsync([profileId], ct)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        if (profile.StarBalance < reward.StarCost)
            throw new InvalidOperationException(
                $"Insufficient stars. Need {reward.StarCost}, have {profile.StarBalance}");

        var redemption = new RewardRedemption
        {
            Id = Guid.NewGuid(),
            RewardId = rewardId,
            ProfileId = profileId,
            StarsSpent = reward.StarCost,
            RedeemedAt = DateTimeOffset.UtcNow,
        };

        _db.RewardRedemptions.Add(redemption);
        await _profileService.AdjustStarsAsync(profileId, -reward.StarCost, ct);
        await _db.SaveChangesAsync(ct);

        return redemption;
    }

    public async Task<IReadOnlyList<RewardRedemption>> GetRedemptionHistoryAsync(
        Guid profileId, CancellationToken ct = default)
    {
        return await _db.RewardRedemptions
            .Include(r => r.Reward)
            .Where(r => r.ProfileId == profileId)
            .OrderByDescending(r => r.RedeemedAt)
            .ToListAsync(ct);
    }
}
