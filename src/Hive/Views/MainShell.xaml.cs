using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Hive.Views;

public sealed partial class MainShell : UserControl
{
    private Button? _activeNavButton;

    public MainShell()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Navigate to Calendar by default
        NavigateTo("Calendar", NavCalendar);
    }

    private void OnNavClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            NavigateTo(tag, button);
        }
    }

    private void NavigateTo(string page, Button navButton)
    {
        // Update active state styling
        if (_activeNavButton is not null)
        {
            _activeNavButton.Foreground = new SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
        }

        navButton.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        _activeNavButton = navButton;

        // Navigate
        var pageType = page switch
        {
            "Calendar" => typeof(CalendarPage),
            "Tasks" => typeof(TasksPage),
            "Rewards" => typeof(RewardsPage),
            "Meals" => typeof(MealPlanPage),
            "Lists" => typeof(ListsPage),
            "Settings" => typeof(SettingsPage),
            _ => typeof(CalendarPage),
        };

        ContentFrame.Navigate(pageType);
    }
}
