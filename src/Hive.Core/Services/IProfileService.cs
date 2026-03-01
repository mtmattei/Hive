using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IProfileService
{
    Task<IReadOnlyList<Profile>> GetProfilesAsync(
        Guid familyAccountId,
        CancellationToken ct = default);

    Task<Profile?> GetProfileByIdAsync(Guid profileId, CancellationToken ct = default);

    Task<Profile> CreateProfileAsync(Profile profile, CancellationToken ct = default);

    Task<Profile> UpdateProfileAsync(Profile profile, CancellationToken ct = default);

    Task DeleteProfileAsync(Guid profileId, CancellationToken ct = default);

    Task<int> AdjustStarsAsync(Guid profileId, int delta, CancellationToken ct = default);
}
