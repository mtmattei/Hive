using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IMagicImportService
{
    Task<MagicImport> ImportFromTextAsync(Guid familyAccountId, string content, Guid defaultProfileId, CancellationToken ct = default);
    Task<IReadOnlyList<MagicImport>> GetImportHistoryAsync(Guid familyAccountId, CancellationToken ct = default);
}
