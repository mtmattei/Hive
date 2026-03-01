using Hive.Core.Models;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class AddMealDialog : ContentDialog
{
    private readonly Guid _familyAccountId;
    private Guid? _editingEntryId;

    public MealPlanEntry? Result { get; private set; }
    public bool IsEditing => _editingEntryId.HasValue;

    public AddMealDialog(Guid familyAccountId, IEnumerable<Recipe> recipes)
    {
        InitializeComponent();
        _familyAccountId = familyAccountId;

        RecipeCombo.ItemsSource = new[] { new Recipe { Title = "(None)" } }
            .Concat(recipes).ToList();
        RecipeCombo.SelectedIndex = 0;

        DatePicker.Date = DateTimeOffset.Now;
    }

    public void LoadEntry(MealPlanEntry entry)
    {
        _editingEntryId = entry.Id;
        Title = "Edit Meal";
        PrimaryButtonText = "Update";

        CategoryCombo.SelectedIndex = (int)entry.Category;
        DatePicker.Date = entry.Date.ToDateTime(TimeOnly.MinValue);
        CustomNameInput.Text = entry.CustomMealName ?? string.Empty;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var hasRecipe = RecipeCombo.SelectedIndex > 0 && RecipeCombo.SelectedItem is Recipe recipe && recipe.Id != Guid.Empty;
        var hasCustomName = !string.IsNullOrWhiteSpace(CustomNameInput.Text);

        if (!hasRecipe && !hasCustomName)
        {
            args.Cancel = true;
            CustomNameInput.Header = "Meal name (required if no recipe)";
            return;
        }

        var selectedRecipe = hasRecipe ? (Recipe)RecipeCombo.SelectedItem! : null;

        Result = new MealPlanEntry
        {
            Id = _editingEntryId ?? Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            Date = DateOnly.FromDateTime(DatePicker.Date?.DateTime ?? DateTime.Today),
            Category = (MealCategory)CategoryCombo.SelectedIndex,
            RecipeId = selectedRecipe?.Id,
            CustomMealName = hasCustomName ? CustomNameInput.Text.Trim() : selectedRecipe?.Title,
        };
    }
}
