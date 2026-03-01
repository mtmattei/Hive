using Hive.Core.Models;

namespace Hive.Core.Services;

public interface ISharedAccessService
{
    Task<IReadOnlyList<SharedAccess>> GetSharedAccessAsync(Guid familyAccountId, CancellationToken ct = default);
    Task<SharedAccess> InviteAsync(SharedAccess invite, CancellationToken ct = default);
    Task AcceptInviteAsync(Guid inviteId, CancellationToken ct = default);
    Task RevokeAccessAsync(Guid inviteId, CancellationToken ct = default);
}
