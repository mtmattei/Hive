using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class CalendarPage : Page
{
    private CalendarViewModel ViewModel { get; }

    public CalendarPage()
    {
        ViewModel = App.Services.GetRequiredService<CalendarViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        UpdateUI();
    }

    private void OnViewSwitch(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            var mode = Enum.Parse<CalendarViewMode>(tag);
            ViewModel.SwitchViewCommand.Execute(mode);
            UpdateUI();
        }
    }

    private void OnNavigateForward(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateForwardCommand.Execute(null);
        UpdateUI();
    }

    private void OnNavigateBackward(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateBackwardCommand.Execute(null);
        UpdateUI();
    }

    private void OnGoToToday(object sender, RoutedEventArgs e)
    {
        ViewModel.GoToTodayCommand.Execute(null);
        UpdateUI();
    }

    private void OnAddEvent(object sender, RoutedEventArgs e)
    {
        // TODO: Show add event dialog
    }

    private void UpdateUI()
    {
        // Update header title based on view and date
        HeaderTitle.Text = ViewModel.CurrentView switch
        {
            CalendarViewMode.Day => ViewModel.SelectedDate.ToString("dddd, MMMM d"),
            CalendarViewMode.Week => $"Week of {ViewModel.SelectedDate.ToString("MMMM d, yyyy")}",
            CalendarViewMode.Month => ViewModel.SelectedDate.ToString("MMMM yyyy"),
            CalendarViewMode.Schedule => ViewModel.SelectedDate.ToString("MMMM yyyy"),
            _ => ViewModel.SelectedDate.ToString("MMMM yyyy"),
        };

        // Update state visibility
        LoadingRing.Visibility = ViewModel.State == ViewState.Loading ? Visibility.Visible : Visibility.Collapsed;
        LoadingRing.IsActive = ViewModel.State == ViewState.Loading;
        EmptyState.Visibility = ViewModel.State == ViewState.Empty ? Visibility.Visible : Visibility.Collapsed;

        // Update profile filter
        ProfileFilterRepeater.ItemsSource = ViewModel.Profiles;
    }
}
