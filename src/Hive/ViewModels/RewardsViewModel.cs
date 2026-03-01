using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Models;
using Hive.Core.Services;

namespace Hive.ViewModels;

public partial class RewardsViewModel : ObservableObject
{
    private readonly IRewardService _rewardService;
    private readonly IProfileService _profileService;

    private readonly Guid _familyAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty] private ViewState _state = ViewState.Loading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<Profile> _profiles = [];
    [ObservableProperty] private ObservableCollection<Reward> _rewards = [];
    [ObservableProperty] private bool _showRedeemCelebration;

    public RewardsViewModel(IRewardService rewardService, IProfileService profileService)
    {
        _rewardService = rewardService;
        _profileService = profileService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;

            var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
            Profiles = new ObservableCollection<Profile>(profiles);

            await LoadRewardsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task LoadRewardsAsync()
    {
        try
        {
            var rewards = await _rewardService.GetRewardsAsync(_familyAccountId);
            Rewards = new ObservableCollection<Reward>(rewards);

            var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
            Profiles = new ObservableCollection<Profile>(profiles);

            State = Rewards.Count > 0 ? ViewState.Loaded : ViewState.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task RedeemRewardAsync(RedeemRequest request)
    {
        try
        {
            await _rewardService.RedeemRewardAsync(request.RewardId, request.ProfileId);
            ShowRedeemCelebration = true;
            _ = Task.Delay(2000).ContinueWith(_ => ShowRedeemCelebration = false);
            await LoadRewardsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public IEnumerable<Reward> GetRewardsForProfile(Guid profileId) =>
        Rewards.Where(r => r.EligibleProfiles.Any(ep => ep.ProfileId == profileId));

    public double GetRewardProgress(Guid profileId, Reward reward)
    {
        var profile = Profiles.FirstOrDefault(p => p.Id == profileId);
        if (profile is null || reward.StarCost == 0) return 0;
        return Math.Min(1.0, (double)profile.StarBalance / reward.StarCost);
    }

    public bool CanRedeem(Guid profileId, Reward reward)
    {
        var profile = Profiles.FirstOrDefault(p => p.Id == profileId);
        return profile is not null && profile.StarBalance >= reward.StarCost;
    }

    public async Task CreateRewardAsync(Reward reward, IReadOnlyList<Guid> profileIds)
    {
        reward.FamilyAccountId = _familyAccountId;
        reward.EligibleProfiles = profileIds.Select(pid => new RewardEligibility
        {
            Id = Guid.NewGuid(),
            ProfileId = pid,
        }).ToList();

        await _rewardService.CreateRewardAsync(reward);
        await LoadRewardsAsync();
    }

    public async Task UpdateRewardAsync(Reward reward, IReadOnlyList<Guid> profileIds)
    {
        reward.FamilyAccountId = _familyAccountId;
        reward.EligibleProfiles = profileIds.Select(pid => new RewardEligibility
        {
            Id = Guid.NewGuid(),
            RewardId = reward.Id,
            ProfileId = pid,
        }).ToList();

        await _rewardService.UpdateRewardAsync(reward);
        await LoadRewardsAsync();
    }

    [RelayCommand]
    private async Task DeleteRewardAsync(Guid rewardId)
    {
        await _rewardService.DeleteRewardAsync(rewardId);
        await LoadRewardsAsync();
    }
}

public record RedeemRequest(Guid RewardId, Guid ProfileId);
