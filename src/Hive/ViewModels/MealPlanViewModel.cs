using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Models;
using Hive.Core.Services;

namespace Hive.ViewModels;

public partial class MealPlanViewModel : ObservableObject
{
    private readonly IMealPlanService _mealPlanService;

    private readonly Guid _familyAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty] private ViewState _state = ViewState.Loading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private DateOnly _weekStart;
    [ObservableProperty] private ObservableCollection<MealPlanEntry> _entries = [];
    [ObservableProperty] private ObservableCollection<Recipe> _recipes = [];

    public MealPlanViewModel(IMealPlanService mealPlanService)
    {
        _mealPlanService = mealPlanService;
        _weekStart = DateOnly.FromDateTime(DateTime.Today).AddDays(
            -(int)DateTime.Today.DayOfWeek);
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;
            await LoadWeekAsync();
            await LoadRecipesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task LoadWeekAsync()
    {
        try
        {
            var entries = await _mealPlanService.GetMealPlanAsync(_familyAccountId, WeekStart);
            Entries = new ObservableCollection<MealPlanEntry>(entries);
            State = ViewState.Loaded;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
    {
        var recipes = await _mealPlanService.GetRecipesAsync(_familyAccountId);
        Recipes = new ObservableCollection<Recipe>(recipes);
    }

    [RelayCommand]
    private async Task NavigateWeekAsync(int direction)
    {
        WeekStart = WeekStart.AddDays(7 * direction);
        await LoadWeekAsync();
    }

    public async Task CreateEntryAsync(MealPlanEntry entry)
    {
        entry.FamilyAccountId = _familyAccountId;
        await _mealPlanService.CreateEntryAsync(entry);
        await LoadWeekAsync();
    }

    public async Task UpdateEntryAsync(MealPlanEntry entry)
    {
        await _mealPlanService.UpdateEntryAsync(entry);
        await LoadWeekAsync();
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(Guid entryId)
    {
        await _mealPlanService.DeleteEntryAsync(entryId);
        await LoadWeekAsync();
    }

    public async Task CreateRecipeAsync(Recipe recipe)
    {
        recipe.FamilyAccountId = _familyAccountId;
        await _mealPlanService.CreateRecipeAsync(recipe);
        await LoadRecipesAsync();
    }

    [RelayCommand]
    private async Task DeleteRecipeAsync(Guid recipeId)
    {
        await _mealPlanService.DeleteRecipeAsync(recipeId);
        await LoadRecipesAsync();
    }

    public IEnumerable<MealPlanEntry> GetEntriesForDay(DateOnly date) =>
        Entries.Where(e => e.Date == date);

    public MealPlanEntry? GetEntry(DateOnly date, MealCategory category) =>
        Entries.FirstOrDefault(e => e.Date == date && e.Category == category);

    public string WeekLabel =>
        $"{WeekStart:MMM d} \u2013 {WeekStart.AddDays(6):MMM d, yyyy}";
}
