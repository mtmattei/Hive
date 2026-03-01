using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class CountdownService : ICountdownService
{
    private readonly HiveDbContext _db;

    public CountdownService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<EventCountdown>> GetCountdownsAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.EventCountdowns
            .Where(c => c.FamilyAccountId == familyAccountId && c.IsActive)
            .OrderBy(c => c.TargetDate)
            .ToListAsync(ct);
    }

    public async Task<EventCountdown> CreateCountdownAsync(
        EventCountdown countdown, CancellationToken ct = default)
    {
        countdown.Id = countdown.Id == Guid.Empty ? Guid.NewGuid() : countdown.Id;
        _db.EventCountdowns.Add(countdown);
        await _db.SaveChangesAsync(ct);
        return countdown;
    }

    public async Task<EventCountdown> UpdateCountdownAsync(
        EventCountdown countdown, CancellationToken ct = default)
    {
        _db.EventCountdowns.Update(countdown);
        await _db.SaveChangesAsync(ct);
        return countdown;
    }

    public async Task DeleteCountdownAsync(Guid countdownId, CancellationToken ct = default)
    {
        var countdown = await _db.EventCountdowns.FindAsync([countdownId], ct);
        if (countdown is not null)
        {
            _db.EventCountdowns.Remove(countdown);
            await _db.SaveChangesAsync(ct);
        }
    }
}
