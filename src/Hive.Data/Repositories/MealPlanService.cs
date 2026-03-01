using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class MealPlanService : IMealPlanService
{
    private readonly HiveDbContext _db;

    public MealPlanService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<MealPlanEntry>> GetMealPlanAsync(
        Guid familyAccountId, DateOnly weekStart, int days = 7, CancellationToken ct = default)
    {
        var end = weekStart.AddDays(days);
        return await _db.MealPlanEntries
            .Include(m => m.Recipe)
            .Where(m => m.FamilyAccountId == familyAccountId && m.Date >= weekStart && m.Date < end)
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Category)
            .ToListAsync(ct);
    }

    public async Task<MealPlanEntry> CreateEntryAsync(MealPlanEntry entry, CancellationToken ct = default)
    {
        entry.Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id;
        _db.MealPlanEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<MealPlanEntry> UpdateEntryAsync(MealPlanEntry entry, CancellationToken ct = default)
    {
        _db.MealPlanEntries.Update(entry);
        await _db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task DeleteEntryAsync(Guid entryId, CancellationToken ct = default)
    {
        var entry = await _db.MealPlanEntries.FindAsync([entryId], ct);
        if (entry is not null)
        {
            _db.MealPlanEntries.Remove(entry);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipesAsync(
        Guid familyAccountId, MealCategory? category = null, CancellationToken ct = default)
    {
        var query = _db.Recipes.Where(r => r.FamilyAccountId == familyAccountId);
        if (category is not null)
            query = query.Where(r => r.Category == category.Value);
        return await query.OrderBy(r => r.Title).ToListAsync(ct);
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid recipeId, CancellationToken ct = default)
    {
        return await _db.Recipes.FindAsync([recipeId], ct);
    }

    public async Task<Recipe> CreateRecipeAsync(Recipe recipe, CancellationToken ct = default)
    {
        recipe.Id = recipe.Id == Guid.Empty ? Guid.NewGuid() : recipe.Id;
        recipe.CreatedAt = DateTimeOffset.UtcNow;
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync(ct);
        return recipe;
    }

    public async Task<Recipe> UpdateRecipeAsync(Recipe recipe, CancellationToken ct = default)
    {
        _db.Recipes.Update(recipe);
        await _db.SaveChangesAsync(ct);
        return recipe;
    }

    public async Task DeleteRecipeAsync(Guid recipeId, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes.FindAsync([recipeId], ct);
        if (recipe is not null)
        {
            _db.Recipes.Remove(recipe);
            await _db.SaveChangesAsync(ct);
        }
    }
}
