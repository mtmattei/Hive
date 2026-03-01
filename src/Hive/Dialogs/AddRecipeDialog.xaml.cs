using Hive.Core.Models;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class AddRecipeDialog : ContentDialog
{
    private readonly Guid _familyAccountId;
    private Guid? _editingRecipeId;

    public Recipe? Result { get; private set; }
    public bool IsEditing => _editingRecipeId.HasValue;

    public AddRecipeDialog(Guid familyAccountId)
    {
        InitializeComponent();
        _familyAccountId = familyAccountId;
    }

    public void LoadRecipe(Recipe recipe)
    {
        _editingRecipeId = recipe.Id;
        Title = "Edit Recipe";
        PrimaryButtonText = "Update";

        TitleInput.Text = recipe.Title;
        CategoryCombo.SelectedIndex = (int)recipe.Category;
        DescriptionInput.Text = recipe.Description ?? string.Empty;
        IngredientsInput.Text = recipe.Ingredients ?? string.Empty;
        InstructionsInput.Text = recipe.Instructions ?? string.Empty;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(TitleInput.Text))
        {
            args.Cancel = true;
            TitleInput.Header = "Recipe name (required)";
            return;
        }

        Result = new Recipe
        {
            Id = _editingRecipeId ?? Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            Title = TitleInput.Text.Trim(),
            Category = (MealCategory)CategoryCombo.SelectedIndex,
            Description = string.IsNullOrWhiteSpace(DescriptionInput.Text) ? null : DescriptionInput.Text.Trim(),
            Ingredients = string.IsNullOrWhiteSpace(IngredientsInput.Text) ? null : IngredientsInput.Text.Trim(),
            Instructions = string.IsNullOrWhiteSpace(InstructionsInput.Text) ? null : InstructionsInput.Text.Trim(),
            CreatedAt = _editingRecipeId.HasValue ? default : DateTimeOffset.UtcNow,
        };
    }
}
