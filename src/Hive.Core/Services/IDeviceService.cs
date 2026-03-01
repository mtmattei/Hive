using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IDeviceService
{
    Task<IReadOnlyList<Device>> GetDevicesAsync(Guid familyAccountId, CancellationToken ct = default);
    Task<Device> RegisterDeviceAsync(Device device, CancellationToken ct = default);
    Task<Device?> ActivateDeviceAsync(string activationCode, Guid familyAccountId, CancellationToken ct = default);
    Task DeleteDeviceAsync(Guid deviceId, CancellationToken ct = default);
}
