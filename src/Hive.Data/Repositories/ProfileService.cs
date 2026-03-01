using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class ProfileService : IProfileService
{
    private readonly HiveDbContext _db;

    public ProfileService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<Profile>> GetProfilesAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.Profiles
            .Where(p => p.FamilyAccountId == familyAccountId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<Profile?> GetProfileByIdAsync(Guid profileId, CancellationToken ct = default)
    {
        return await _db.Profiles.FindAsync([profileId], ct);
    }

    public async Task<Profile> CreateProfileAsync(Profile profile, CancellationToken ct = default)
    {
        profile.Id = profile.Id == Guid.Empty ? Guid.NewGuid() : profile.Id;

        var maxOrder = await _db.Profiles
            .Where(p => p.FamilyAccountId == profile.FamilyAccountId)
            .MaxAsync(p => (int?)p.SortOrder, ct) ?? -1;
        profile.SortOrder = maxOrder + 1;

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(ct);
        return profile;
    }

    public async Task<Profile> UpdateProfileAsync(Profile profile, CancellationToken ct = default)
    {
        _db.Profiles.Update(profile);
        await _db.SaveChangesAsync(ct);
        return profile;
    }

    public async Task DeleteProfileAsync(Guid profileId, CancellationToken ct = default)
    {
        var profile = await _db.Profiles.FindAsync([profileId], ct);
        if (profile is not null)
        {
            _db.Profiles.Remove(profile);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<int> AdjustStarsAsync(Guid profileId, int delta, CancellationToken ct = default)
    {
        var profile = await _db.Profiles.FindAsync([profileId], ct)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        profile.StarBalance = Math.Max(0, profile.StarBalance + delta);
        await _db.SaveChangesAsync(ct);
        return profile.StarBalance;
    }
}
