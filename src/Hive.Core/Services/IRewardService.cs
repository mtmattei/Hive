using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IRewardService
{
    Task<IReadOnlyList<Reward>> GetRewardsAsync(
        Guid familyAccountId,
        Guid? profileId = null,
        CancellationToken ct = default);

    Task<Reward> CreateRewardAsync(Reward reward, CancellationToken ct = default);

    Task<Reward> UpdateRewardAsync(Reward reward, CancellationToken ct = default);

    Task DeleteRewardAsync(Guid rewardId, CancellationToken ct = default);

    Task<int> GetStarBalanceAsync(Guid profileId, CancellationToken ct = default);

    Task<RewardRedemption> RedeemRewardAsync(
        Guid rewardId,
        Guid profileId,
        CancellationToken ct = default);

    Task<IReadOnlyList<RewardRedemption>> GetRedemptionHistoryAsync(
        Guid profileId,
        CancellationToken ct = default);
}
