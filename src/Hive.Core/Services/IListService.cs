using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IListService
{
    Task<IReadOnlyList<CustomList>> GetListsAsync(
        Guid familyAccountId,
        CancellationToken ct = default);

    Task<CustomList?> GetListByIdAsync(Guid listId, CancellationToken ct = default);

    Task<CustomList> CreateListAsync(CustomList list, CancellationToken ct = default);

    Task<CustomList> UpdateListAsync(CustomList list, CancellationToken ct = default);

    Task DeleteListAsync(Guid listId, CancellationToken ct = default);

    Task<ListItem> AddItemAsync(Guid listId, ListItem item, CancellationToken ct = default);

    Task<ListItem> UpdateItemAsync(ListItem item, CancellationToken ct = default);

    Task DeleteItemAsync(Guid itemId, CancellationToken ct = default);

    Task ToggleItemCompletionAsync(Guid itemId, CancellationToken ct = default);

    Task ReorderItemsAsync(Guid listId, IReadOnlyList<Guid> orderedItemIds, CancellationToken ct = default);
}
