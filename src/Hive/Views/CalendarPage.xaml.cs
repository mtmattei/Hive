using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Dialogs;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
        _ = LoadWeatherAsync();
        _ = LoadCountdownsAsync();
    }

    private async Task LoadWeatherAsync()
    {
        try
        {
            var weatherService = App.Services.GetRequiredService<IWeatherService>();
            var forecast = await weatherService.GetCurrentAsync("New York");
            if (forecast is not null)
            {
                WeatherText.Text = $"{forecast.Icon} {forecast.TempCurrentF}\u00B0F {forecast.Condition}";
                WeatherBadge.Visibility = Visibility.Visible;
            }
        }
        catch { /* Weather is non-critical */ }
    }

    private async Task LoadCountdownsAsync()
    {
        try
        {
            var countdownService = App.Services.GetRequiredService<ICountdownService>();
            var countdowns = await countdownService.GetCountdownsAsync(
                Guid.Parse("00000000-0000-0000-0000-000000000001"));
            var next = countdowns.Where(c => c.DaysRemaining >= 0).OrderBy(c => c.DaysRemaining).FirstOrDefault();
            if (next is not null)
            {
                CountdownText.Text = $"{next.Emoji} {next.Title}: {next.DaysRemaining}d";
                CountdownBadge.Visibility = Visibility.Visible;
            }
        }
        catch { /* Countdown is non-critical */ }
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

    private async void OnAddEvent(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEventDialog(ViewModel.Profiles.ToList(), ViewModel.SelectedDate)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateEventAsync(dialog.Result);
            UpdateUI();
        }
    }

    public async void OnEditEvent(CalendarEvent evt)
    {
        var dialog = new AddEventDialog(ViewModel.Profiles.ToList(), evt.StartTime)
        {
            XamlRoot = XamlRoot,
        };
        dialog.LoadEvent(evt);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.UpdateEventAsync(dialog.Result);
            UpdateUI();
        }
    }

    public async void OnDeleteEvent(CalendarEvent evt)
    {
        var confirm = new ContentDialog
        {
            Title = "Delete Event",
            Content = $"Delete \"{evt.Title}\"? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteEventCommand.ExecuteAsync(evt.Id);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        HeaderTitle.Text = ViewModel.CurrentView switch
        {
            CalendarViewMode.Day => ViewModel.SelectedDate.ToString("dddd, MMMM d"),
            CalendarViewMode.Week => $"Week of {ViewModel.SelectedDate.ToString("MMMM d, yyyy")}",
            CalendarViewMode.Month => ViewModel.SelectedDate.ToString("MMMM yyyy"),
            CalendarViewMode.Schedule => ViewModel.SelectedDate.ToString("MMMM yyyy"),
            _ => ViewModel.SelectedDate.ToString("MMMM yyyy"),
        };

        LoadingRing.Visibility = ViewModel.State == ViewState.Loading ? Visibility.Visible : Visibility.Collapsed;
        LoadingRing.IsActive = ViewModel.State == ViewState.Loading;
        EmptyState.Visibility = ViewModel.State == ViewState.Empty ? Visibility.Visible : Visibility.Collapsed;

        ProfileFilterRepeater.ItemsSource = ViewModel.Profiles;

        var isSchedule = ViewModel.CurrentView == CalendarViewMode.Schedule;
        var isWeek = ViewModel.CurrentView == CalendarViewMode.Week;
        var isMonth = ViewModel.CurrentView == CalendarViewMode.Month;

        ScheduleViewControl.Visibility = (isSchedule || ViewModel.CurrentView == CalendarViewMode.Day)
            ? Visibility.Visible : Visibility.Collapsed;
        WeekViewControl.Visibility = isWeek ? Visibility.Visible : Visibility.Collapsed;
        MonthViewControl.Visibility = isMonth ? Visibility.Visible : Visibility.Collapsed;

        if (isSchedule || ViewModel.CurrentView == CalendarViewMode.Day)
        {
            ScheduleViewControl.Events = ViewModel.Events;
            ScheduleViewControl.SelectedDate = ViewModel.SelectedDate;
            ScheduleViewControl.DayCount = ViewModel.CurrentView == CalendarViewMode.Day ? 1 : 5;
        }
        else if (isWeek)
        {
            WeekViewControl.Events = ViewModel.Events;
            WeekViewControl.SelectedDate = ViewModel.SelectedDate;
        }
        else if (isMonth)
        {
            MonthViewControl.Events = ViewModel.Events;
            MonthViewControl.SelectedDate = ViewModel.SelectedDate;
        }

        UpdateViewSwitcherStyle();
    }

    private void UpdateViewSwitcherStyle()
    {
        var buttons = new[] { BtnSchedule, BtnDay, BtnWeek, BtnMonth };
        var activeTag = ViewModel.CurrentView.ToString();

        foreach (var btn in buttons)
        {
            var isActive = (string)btn.Tag == activeTag;
            btn.Background = isActive
                ? (Brush)Resources["WhiteBrush"]
                : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            btn.FontWeight = isActive
                ? Microsoft.UI.Text.FontWeights.Bold
                : Microsoft.UI.Text.FontWeights.Normal;
        }
    }
}
