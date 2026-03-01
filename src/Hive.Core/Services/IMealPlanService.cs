using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IMealPlanService
{
    Task<IReadOnlyList<MealPlanEntry>> GetMealPlanAsync(
        Guid familyAccountId,
        DateOnly weekStart,
        int days = 7,
        CancellationToken ct = default);

    Task<MealPlanEntry> CreateEntryAsync(MealPlanEntry entry, CancellationToken ct = default);

    Task<MealPlanEntry> UpdateEntryAsync(MealPlanEntry entry, CancellationToken ct = default);

    Task DeleteEntryAsync(Guid entryId, CancellationToken ct = default);

    // Recipes
    Task<IReadOnlyList<Recipe>> GetRecipesAsync(
        Guid familyAccountId,
        MealCategory? category = null,
        CancellationToken ct = default);

    Task<Recipe?> GetRecipeByIdAsync(Guid recipeId, CancellationToken ct = default);

    Task<Recipe> CreateRecipeAsync(Recipe recipe, CancellationToken ct = default);

    Task<Recipe> UpdateRecipeAsync(Recipe recipe, CancellationToken ct = default);

    Task DeleteRecipeAsync(Guid recipeId, CancellationToken ct = default);
}
