using Hive.Core.Models;
using Hive.Dialogs;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class MealPlanPage : Page
{
    private MealPlanViewModel ViewModel { get; }
    private static readonly Guid FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static readonly string[] CategoryEmojis = ["\U0001F373", "\U0001F96A", "\U0001F35D", "\U0001F34E"];
    private static readonly string[] CategoryLabels = ["Breakfast", "Lunch", "Dinner", "Snack"];

    public MealPlanPage()
    {
        ViewModel = App.Services.GetRequiredService<MealPlanViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        UpdateUI();
    }

    private void UpdateUI()
    {
        WeekLabel.Text = ViewModel.WeekLabel;
        EmptyState.Visibility = ViewModel.Entries.Count == 0
            ? Visibility.Visible : Visibility.Collapsed;
        BuildMealGrid();
    }

    private void BuildMealGrid()
    {
        MealGrid.Children.Clear();
        MealGrid.ColumnDefinitions.Clear();
        MealGrid.RowDefinitions.Clear();

        // Header row + 4 meal category rows
        MealGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (int i = 0; i < 4; i++)
            MealGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Label column + 7 day columns
        MealGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        for (int d = 0; d < 7; d++)
            MealGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

        // Day headers
        for (int d = 0; d < 7; d++)
        {
            var date = ViewModel.WeekStart.AddDays(d);
            var isToday = date == DateOnly.FromDateTime(DateTime.Today);
            var header = new TextBlock
            {
                Text = $"{date:ddd}\n{date:M/d}",
                FontSize = 13,
                FontWeight = isToday ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal,
                Foreground = isToday
                    ? (Brush)Resources["SeaBrush"]
                    : new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x4A, 0x4A, 0x4A)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
                Margin = new Thickness(4, 0, 4, 8),
            };
            Grid.SetRow(header, 0);
            Grid.SetColumn(header, d + 1);
            MealGrid.Children.Add(header);
        }

        // Category rows
        for (int c = 0; c < 4; c++)
        {
            var category = (MealCategory)c;

            // Row label
            var label = new TextBlock
            {
                Text = $"{CategoryEmojis[c]} {CategoryLabels[c]}",
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 4, 8, 4),
            };
            Grid.SetRow(label, c + 1);
            Grid.SetColumn(label, 0);
            MealGrid.Children.Add(label);

            // Day cells
            for (int d = 0; d < 7; d++)
            {
                var date = ViewModel.WeekStart.AddDays(d);
                var entry = ViewModel.GetEntry(date, category);
                var cell = CreateMealCell(entry, date, category);
                Grid.SetRow(cell, c + 1);
                Grid.SetColumn(cell, d + 1);
                MealGrid.Children.Add(cell);
            }
        }
    }

    private Border CreateMealCell(MealPlanEntry? entry, DateOnly date, MealCategory category)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(6),
            Margin = new Thickness(2),
            MinHeight = 48,
        };

        if (entry is not null)
        {
            var panel = new StackPanel { Spacing = 2 };
            panel.Children.Add(new TextBlock
            {
                Text = entry.CustomMealName ?? entry.Recipe?.Title ?? "Meal",
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxLines = 2,
            });

            if (entry.Recipe is not null)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "\U0001F4D6 Recipe",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x88, 0x88, 0x88)),
                });
            }

            border.Child = panel;
            border.Tag = entry;
            border.PointerPressed += (s, _) =>
            {
                if (s is Border bd && bd.Tag is MealPlanEntry e)
                    OnEditMeal(e);
            };
        }
        else
        {
            var addBtn = new Button
            {
                Content = "+",
                FontSize = 14,
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.4,
                Padding = new Thickness(8, 2, 8, 2),
                Tag = new MealCellInfo(date, category),
            };
            addBtn.Click += OnQuickAddMeal;
            border.Child = addBtn;
        }

        return border;
    }

    private async void OnQuickAddMeal(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MealCellInfo info)
        {
            var dialog = new AddMealDialog(FamilyId, ViewModel.Recipes)
            {
                XamlRoot = XamlRoot,
            };
            // Pre-set the date and category
            dialog.FindName("DatePicker");
            dialog.FindName("CategoryCombo");

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.Result is not null)
            {
                await ViewModel.CreateEntryAsync(dialog.Result);
                UpdateUI();
            }
        }
    }

    private async void OnAddMeal(object sender, RoutedEventArgs e)
    {
        var dialog = new AddMealDialog(FamilyId, ViewModel.Recipes)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateEntryAsync(dialog.Result);
            UpdateUI();
        }
    }

    private async void OnEditMeal(MealPlanEntry entry)
    {
        var dialog = new AddMealDialog(FamilyId, ViewModel.Recipes)
        {
            XamlRoot = XamlRoot,
        };
        dialog.LoadEntry(entry);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.UpdateEntryAsync(dialog.Result);
            UpdateUI();
        }
    }

    private async void OnAddRecipe(object sender, RoutedEventArgs e)
    {
        var dialog = new AddRecipeDialog(FamilyId)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateRecipeAsync(dialog.Result);
        }
    }

    private async void OnPreviousWeek(object sender, RoutedEventArgs e)
    {
        await ViewModel.NavigateWeekCommand.ExecuteAsync(-1);
        UpdateUI();
    }

    private async void OnNextWeek(object sender, RoutedEventArgs e)
    {
        await ViewModel.NavigateWeekCommand.ExecuteAsync(1);
        UpdateUI();
    }
}

internal record MealCellInfo(DateOnly Date, MealCategory Category);
