using System.Security.Cryptography;
using System.Text;
using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class SettingsService : ISettingsService
{
    private readonly HiveDbContext _db;

    public SettingsService(HiveDbContext db) => _db = db;

    public async Task<CalendarSettings> GetSettingsAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        var settings = await _db.CalendarSettings
            .FirstOrDefaultAsync(s => s.FamilyAccountId == familyAccountId, ct);

        if (settings is null)
        {
            settings = new CalendarSettings { FamilyAccountId = familyAccountId };
            _db.CalendarSettings.Add(settings);
            await _db.SaveChangesAsync(ct);
        }

        return settings;
    }

    public async Task<CalendarSettings> UpdateSettingsAsync(
        CalendarSettings settings, CancellationToken ct = default)
    {
        _db.CalendarSettings.Update(settings);
        await _db.SaveChangesAsync(ct);
        return settings;
    }

    public async Task<bool> ValidateParentalLockPinAsync(
        Guid familyAccountId, string pin, CancellationToken ct = default)
    {
        var settings = await GetSettingsAsync(familyAccountId, ct);
        if (string.IsNullOrEmpty(settings.ParentalLockPinHash))
            return false;

        return settings.ParentalLockPinHash == HashPin(pin);
    }

    public async Task SetParentalLockPinAsync(
        Guid familyAccountId, string pin, CancellationToken ct = default)
    {
        var settings = await GetSettingsAsync(familyAccountId, ct);
        settings.ParentalLockPinHash = HashPin(pin);
        settings.ParentalLockEnabled = true;
        await _db.SaveChangesAsync(ct);
    }

    private static string HashPin(string pin)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin));
        return Convert.ToBase64String(bytes);
    }
}
