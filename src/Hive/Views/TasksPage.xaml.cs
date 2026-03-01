using Hive.Controls;
using Hive.Core.Models;
using Hive.Dialogs;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class TasksPage : Page
{
    private TasksViewModel ViewModel { get; }
    private static readonly Guid FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public TasksPage()
    {
        ViewModel = App.Services.GetRequiredService<TasksViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        UpdateUI();
    }

    private async void OnAddTask(object sender, RoutedEventArgs e)
    {
        var dialog = new AddTaskDialog(ViewModel.Profiles.ToList(), FamilyId)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateTaskAsync(dialog.Result, dialog.SelectedProfileIds);
            UpdateUI();
        }
    }

    private async void OnEditTask(TaskItem task)
    {
        var dialog = new AddTaskDialog(ViewModel.Profiles.ToList(), FamilyId)
        {
            XamlRoot = XamlRoot,
        };
        dialog.LoadTask(task);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.UpdateTaskAsync(dialog.Result, dialog.SelectedProfileIds);
            UpdateUI();
        }
    }

    private async void OnDeleteTask(TaskItem task)
    {
        var confirm = new ContentDialog
        {
            Title = "Delete Task",
            Content = $"Delete \"{task.Title}\"? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteTaskCommand.ExecuteAsync(task.Id);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        DateLabel.Text = ViewModel.SelectedDate.ToString("dddd, MMMM d, yyyy");

        EmptyState.Visibility = ViewModel.State == ViewState.Empty
            ? Visibility.Visible : Visibility.Collapsed;

        ProfileColumnsPanel.Children.Clear();

        foreach (var profile in ViewModel.Profiles)
        {
            var column = BuildProfileColumn(profile);
            ProfileColumnsPanel.Children.Add(column);
        }
    }

    private Border BuildProfileColumn(Profile profile)
    {
        var column = new Border
        {
            Style = (Style)Resources["CardStyle"],
            Width = 280,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var stack = new StackPanel { Spacing = 12 };

        // Profile header
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var colorDot = new Border
        {
            Width = 8, Height = 8,
            CornerRadius = new CornerRadius(4),
            Background = HexToBrush(profile.Color),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(colorDot, 0);
        headerGrid.Children.Add(colorDot);

        var nameText = new TextBlock
        {
            Text = profile.Name,
            Style = (Style)Resources["SectionHeadingStyle"],
        };
        Grid.SetColumn(nameText, 1);
        headerGrid.Children.Add(nameText);

        var starText = new TextBlock
        {
            Style = (Style)Resources["SmallLabelStyle"],
            Text = $"\u2b50 {profile.StarBalance}",
        };
        Grid.SetColumn(starText, 2);
        headerGrid.Children.Add(starText);

        stack.Children.Add(headerGrid);
        stack.Children.Add(new Border { Style = (Style)Resources["SeparatorStyle"] });

        // Chores
        var chores = ViewModel.GetChoresForProfile(profile.Id).ToList();
        if (chores.Count > 0)
        {
            stack.Children.Add(new TextBlock
            {
                Text = "Chores",
                Style = (Style)Resources["SmallLabelStyle"],
                Foreground = (Brush)Resources["TextTertiaryBrush"],
                Margin = new Thickness(0, 4, 0, 0),
            });

            foreach (var chore in chores)
            {
                var taskControl = new TaskItemControl
                {
                    Task = chore,
                    IsCompleted = ViewModel.IsTaskCompleted(chore.Id, profile.Id),
                };
                taskControl.CompletionToggled += async (_, t) =>
                {
                    await ViewModel.ToggleTaskCompletionCommand.ExecuteAsync(
                        new TaskCompletionRequest(t.Id, profile.Id));
                    UpdateUI();
                };
                taskControl.TaskTapped += (_, t) => OnEditTask(t);
                stack.Children.Add(taskControl);
            }
        }

        // Routines
        var routines = ViewModel.GetTasksForProfile(profile.Id)
            .Where(t => t.Type == TaskItemType.Routine).ToList();
        if (routines.Count > 0)
        {
            stack.Children.Add(new TextBlock
            {
                Text = "Routines",
                Style = (Style)Resources["SmallLabelStyle"],
                Foreground = (Brush)Resources["TextTertiaryBrush"],
                Margin = new Thickness(0, 8, 0, 0),
            });

            foreach (var routine in routines)
            {
                var taskControl = new TaskItemControl
                {
                    Task = routine,
                    IsCompleted = ViewModel.IsTaskCompleted(routine.Id, profile.Id),
                };
                taskControl.CompletionToggled += async (_, t) =>
                {
                    await ViewModel.ToggleTaskCompletionCommand.ExecuteAsync(
                        new TaskCompletionRequest(t.Id, profile.Id, routine.RoutineTimeOfDay));
                    UpdateUI();
                };
                taskControl.TaskTapped += (_, t) => OnEditTask(t);
                stack.Children.Add(taskControl);
            }
        }

        // Empty state for this profile
        if (chores.Count == 0 && routines.Count == 0)
        {
            stack.Children.Add(new TextBlock
            {
                Text = "No tasks assigned",
                Style = (Style)Resources["BodySecondaryStyle"],
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20),
            });
        }

        column.Child = stack;
        return column;
    }

    private static SolidColorBrush HexToBrush(string hex)
    {
        var r = Convert.ToByte(hex.Substring(1, 2), 16);
        var g = Convert.ToByte(hex.Substring(3, 2), 16);
        var b = Convert.ToByte(hex.Substring(5, 2), 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b));
    }
}
