namespace Hive.Core.Models;

public class MealPlanEntry
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public DateOnly Date { get; set; }
    public MealCategory Category { get; set; }
    public Guid? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public string? CustomMealName { get; set; }
}

public class Recipe
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public MealCategory Category { get; set; }
    public string? Description { get; set; }
    public string? Ingredients { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsGenerated { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public enum MealCategory
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}
