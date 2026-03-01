using Hive.Core.Models;

namespace Hive.Core.Services;

public interface ISettingsService
{
    Task<CalendarSettings> GetSettingsAsync(
        Guid familyAccountId,
        CancellationToken ct = default);

    Task<CalendarSettings> UpdateSettingsAsync(
        CalendarSettings settings,
        CancellationToken ct = default);

    Task<bool> ValidateParentalLockPinAsync(
        Guid familyAccountId,
        string pin,
        CancellationToken ct = default);

    Task SetParentalLockPinAsync(
        Guid familyAccountId,
        string pin,
        CancellationToken ct = default);
}
