using Hive.Core.Models;

namespace Hive.Core.Services;

public interface ICountdownService
{
    Task<IReadOnlyList<EventCountdown>> GetCountdownsAsync(Guid familyAccountId, CancellationToken ct = default);
    Task<EventCountdown> CreateCountdownAsync(EventCountdown countdown, CancellationToken ct = default);
    Task<EventCountdown> UpdateCountdownAsync(EventCountdown countdown, CancellationToken ct = default);
    Task DeleteCountdownAsync(Guid countdownId, CancellationToken ct = default);
}
