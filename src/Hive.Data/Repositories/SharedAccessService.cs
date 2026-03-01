using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class SharedAccessService : ISharedAccessService
{
    private readonly HiveDbContext _db;

    public SharedAccessService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<SharedAccess>> GetSharedAccessAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.SharedAccess
            .Where(s => s.FamilyAccountId == familyAccountId)
            .OrderByDescending(s => s.InvitedAt)
            .ToListAsync(ct);
    }

    public async Task<SharedAccess> InviteAsync(SharedAccess invite, CancellationToken ct = default)
    {
        invite.Id = invite.Id == Guid.Empty ? Guid.NewGuid() : invite.Id;
        invite.InvitedAt = DateTimeOffset.UtcNow;
        invite.Accepted = false;
        _db.SharedAccess.Add(invite);
        await _db.SaveChangesAsync(ct);
        return invite;
    }

    public async Task AcceptInviteAsync(Guid inviteId, CancellationToken ct = default)
    {
        var invite = await _db.SharedAccess.FindAsync([inviteId], ct);
        if (invite is not null)
        {
            invite.Accepted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAccessAsync(Guid inviteId, CancellationToken ct = default)
    {
        var invite = await _db.SharedAccess.FindAsync([inviteId], ct);
        if (invite is not null)
        {
            _db.SharedAccess.Remove(invite);
            await _db.SaveChangesAsync(ct);
        }
    }
}
