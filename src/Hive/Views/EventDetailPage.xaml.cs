using Hive.Core.Models;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class EventDetailPage : Page
{
    private CalendarViewModel ViewModel { get; }
    private CalendarEvent? _event;

    public EventDetailPage()
    {
        ViewModel = App.Services.GetRequiredService<CalendarViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Guid eventId)
        {
            _event = await ViewModel.GetEventByIdAsync(eventId);
            if (_event is not null) LoadEventDetails(_event);
        }
        else if (e.Parameter is CalendarEvent evt)
        {
            _event = evt;
            LoadEventDetails(evt);
        }
    }

    private void LoadEventDetails(CalendarEvent evt)
    {
        EventTitle.Text = evt.Title;
        ProfileName.Text = evt.Profile?.Name ?? "Unknown";

        var color = evt.Profile?.Color ?? "#9A9A9A";
        ProfileAvatar.Background = HexToBrush(color);
        ProfileInitial.Text = evt.Profile?.Name?.Length > 0
            ? evt.Profile.Name[..1].ToUpperInvariant() : "?";

        DateText.Text = evt.IsAllDay
            ? evt.StartTime.ToString("dddd, MMMM d, yyyy") + " (All day)"
            : evt.StartTime.ToString("dddd, MMMM d, yyyy");

        if (!evt.IsAllDay)
        {
            TimeRow.Visibility = Visibility.Visible;
            TimeText.Text = $"{evt.StartTime:h:mm tt}" +
                (evt.EndTime.HasValue ? $" \u2013 {evt.EndTime:h:mm tt}" : "");
        }
        else
        {
            TimeRow.Visibility = Visibility.Collapsed;
        }

        if (!string.IsNullOrEmpty(evt.Location))
        {
            LocationCard.Visibility = Visibility.Visible;
            LocationText.Text = evt.Location;
        }

        if (!string.IsNullOrEmpty(evt.Notes))
        {
            NotesCard.Visibility = Visibility.Visible;
            NotesText.Text = evt.Notes;
        }

        if (evt.Recurrence is not null)
        {
            RecurrenceCard.Visibility = Visibility.Visible;
            RecurrenceText.Text = FormatRecurrence(evt.Recurrence);
        }

        if (evt.ReminderMinutesBefore.HasValue && evt.ReminderMinutesBefore.Value > 0)
        {
            ReminderCard.Visibility = Visibility.Visible;
            ReminderText.Text = evt.ReminderMinutesBefore.Value switch
            {
                5 => "5 minutes before",
                15 => "15 minutes before",
                30 => "30 minutes before",
                60 => "1 hour before",
                1440 => "1 day before",
                _ => $"{evt.ReminderMinutesBefore.Value} minutes before",
            };
        }
    }

    private static string FormatRecurrence(RecurrenceRule rule) => rule.Frequency switch
    {
        RecurrenceFrequency.Daily => rule.Interval == 1 ? "Repeats daily" : $"Repeats every {rule.Interval} days",
        RecurrenceFrequency.Weekly when rule.DaysOfWeek is not null =>
            $"Repeats weekly on {string.Join(", ", rule.DaysOfWeek.Select(d => d.ToString()[..3]))}",
        RecurrenceFrequency.Weekly => rule.Interval == 1 ? "Repeats weekly" : $"Repeats every {rule.Interval} weeks",
        RecurrenceFrequency.Monthly => rule.Interval == 1 ? "Repeats monthly" : $"Repeats every {rule.Interval} months",
        RecurrenceFrequency.Yearly => "Repeats yearly",
        _ => "Repeats",
    };

    private void OnBack(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (_event is not null && Frame.Content is CalendarPage calPage)
        {
            calPage.OnEditEvent(_event);
        }
        else if (_event is not null)
        {
            // Navigate back to calendar and trigger edit
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }

    private async void OnDelete(object sender, RoutedEventArgs e)
    {
        if (_event is null) return;

        var confirm = new ContentDialog
        {
            Title = "Delete Event",
            Content = $"Delete \"{_event.Title}\"? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteEventCommand.ExecuteAsync(_event.Id);
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }

    private static SolidColorBrush HexToBrush(string hex)
    {
        var r = Convert.ToByte(hex.Substring(1, 2), 16);
        var g = Convert.ToByte(hex.Substring(3, 2), 16);
        var b = Convert.ToByte(hex.Substring(5, 2), 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b));
    }
}
