using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class DeviceService : IDeviceService
{
    private readonly HiveDbContext _db;

    public DeviceService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<Device>> GetDevicesAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.Devices
            .Where(d => d.FamilyAccountId == familyAccountId)
            .OrderBy(d => d.DisplayName)
            .ToListAsync(ct);
    }

    public async Task<Device> RegisterDeviceAsync(Device device, CancellationToken ct = default)
    {
        device.Id = device.Id == Guid.Empty ? Guid.NewGuid() : device.Id;
        device.ActivationCode = GenerateActivationCode();
        _db.Devices.Add(device);
        await _db.SaveChangesAsync(ct);
        return device;
    }

    public async Task<Device?> ActivateDeviceAsync(
        string activationCode, Guid familyAccountId, CancellationToken ct = default)
    {
        var device = await _db.Devices
            .FirstOrDefaultAsync(d => d.ActivationCode == activationCode
                && d.FamilyAccountId == familyAccountId, ct);

        if (device is not null)
        {
            device.IsActivated = true;
            await _db.SaveChangesAsync(ct);
        }

        return device;
    }

    public async Task DeleteDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _db.Devices.FindAsync([deviceId], ct);
        if (device is not null)
        {
            _db.Devices.Remove(device);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static string GenerateActivationCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
