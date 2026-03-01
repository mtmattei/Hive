using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class ListService : IListService
{
    private readonly HiveDbContext _db;

    public ListService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<CustomList>> GetListsAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.Lists
            .Include(l => l.Items.OrderBy(i => i.SortOrder))
            .Where(l => l.FamilyAccountId == familyAccountId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<CustomList?> GetListByIdAsync(Guid listId, CancellationToken ct = default)
    {
        return await _db.Lists
            .Include(l => l.Items.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(l => l.Id == listId, ct);
    }

    public async Task<CustomList> CreateListAsync(CustomList list, CancellationToken ct = default)
    {
        list.Id = list.Id == Guid.Empty ? Guid.NewGuid() : list.Id;

        var maxOrder = await _db.Lists
            .Where(l => l.FamilyAccountId == list.FamilyAccountId)
            .MaxAsync(l => (int?)l.SortOrder, ct) ?? -1;
        list.SortOrder = maxOrder + 1;

        _db.Lists.Add(list);
        await _db.SaveChangesAsync(ct);
        return list;
    }

    public async Task<CustomList> UpdateListAsync(CustomList list, CancellationToken ct = default)
    {
        _db.Lists.Update(list);
        await _db.SaveChangesAsync(ct);
        return list;
    }

    public async Task DeleteListAsync(Guid listId, CancellationToken ct = default)
    {
        var list = await _db.Lists.FindAsync([listId], ct);
        if (list is not null)
        {
            _db.Lists.Remove(list);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<ListItem> AddItemAsync(Guid listId, ListItem item, CancellationToken ct = default)
    {
        item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
        item.ListId = listId;

        var maxOrder = await _db.ListItems
            .Where(i => i.ListId == listId)
            .MaxAsync(i => (int?)i.SortOrder, ct) ?? -1;
        item.SortOrder = maxOrder + 1;

        _db.ListItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<ListItem> UpdateItemAsync(ListItem item, CancellationToken ct = default)
    {
        _db.ListItems.Update(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task DeleteItemAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await _db.ListItems.FindAsync([itemId], ct);
        if (item is not null)
        {
            _db.ListItems.Remove(item);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task ToggleItemCompletionAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await _db.ListItems.FindAsync([itemId], ct)
            ?? throw new InvalidOperationException($"List item {itemId} not found");

        item.IsCompleted = !item.IsCompleted;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReorderItemsAsync(Guid listId, IReadOnlyList<Guid> orderedItemIds, CancellationToken ct = default)
    {
        var items = await _db.ListItems
            .Where(i => i.ListId == listId)
            .ToListAsync(ct);

        for (var i = 0; i < orderedItemIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == orderedItemIds[i]);
            if (item is not null)
                item.SortOrder = i;
        }

        await _db.SaveChangesAsync(ct);
    }
}
