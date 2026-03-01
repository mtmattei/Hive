using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Extensions;
using Hive.Core.Models;
using Hive.Core.Services;

namespace Hive.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly ICalendarService _calendarService;
    private readonly IProfileService _profileService;

    // Use the demo family account ID for MVP
    private readonly Guid _familyAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty] private ViewState _state = ViewState.Loading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private CalendarViewMode _currentView = CalendarViewMode.Schedule;
    [ObservableProperty] private DateTimeOffset _selectedDate = DateTimeOffset.Now;
    [ObservableProperty] private ObservableCollection<CalendarEvent> _events = [];
    [ObservableProperty] private ObservableCollection<Profile> _profiles = [];
    [ObservableProperty] private ObservableCollection<Guid> _visibleProfileIds = [];
    [ObservableProperty] private CalendarEvent? _selectedEvent;

    public CalendarViewModel(ICalendarService calendarService, IProfileService profileService)
    {
        _calendarService = calendarService;
        _profileService = profileService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;

            var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
            Profiles = new ObservableCollection<Profile>(profiles);
            VisibleProfileIds = new ObservableCollection<Guid>(profiles.Select(p => p.Id));

            await LoadEventsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        try
        {
            var (rangeStart, rangeEnd) = GetDateRange();
            var profileFilter = VisibleProfileIds.Count < Profiles.Count
                ? VisibleProfileIds.AsEnumerable()
                : null;

            var events = await _calendarService.GetEventsAsync(
                _familyAccountId, rangeStart, rangeEnd, profileFilter);

            Events = new ObservableCollection<CalendarEvent>(events);
            State = Events.Count > 0 ? ViewState.Loaded : ViewState.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private void SwitchView(CalendarViewMode mode)
    {
        CurrentView = mode;
        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private void GoToToday()
    {
        SelectedDate = DateTimeOffset.Now;
        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private void NavigateForward()
    {
        SelectedDate = CurrentView switch
        {
            CalendarViewMode.Day => SelectedDate.AddDays(1),
            CalendarViewMode.Week or CalendarViewMode.Schedule => SelectedDate.AddDays(7),
            CalendarViewMode.Month => SelectedDate.AddMonths(1),
            _ => SelectedDate.AddDays(7),
        };
        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private void NavigateBackward()
    {
        SelectedDate = CurrentView switch
        {
            CalendarViewMode.Day => SelectedDate.AddDays(-1),
            CalendarViewMode.Week or CalendarViewMode.Schedule => SelectedDate.AddDays(-7),
            CalendarViewMode.Month => SelectedDate.AddMonths(-1),
            _ => SelectedDate.AddDays(-7),
        };
        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private void SelectEvent(CalendarEvent? evt)
    {
        SelectedEvent = evt;
    }

    [RelayCommand]
    private void ToggleProfileVisibility(Guid profileId)
    {
        if (VisibleProfileIds.Contains(profileId))
            VisibleProfileIds.Remove(profileId);
        else
            VisibleProfileIds.Add(profileId);

        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private async Task DeleteEventAsync(Guid eventId)
    {
        await _calendarService.DeleteEventAsync(eventId);
        SelectedEvent = null;
        await LoadEventsAsync();
    }

    private (DateTimeOffset Start, DateTimeOffset End) GetDateRange()
    {
        return CurrentView switch
        {
            CalendarViewMode.Day => (SelectedDate.StartOfDay(), SelectedDate.EndOfDay()),
            CalendarViewMode.Week => (SelectedDate.StartOfWeek(), SelectedDate.EndOfWeek()),
            CalendarViewMode.Schedule => (SelectedDate.StartOfDay(), SelectedDate.AddDays(7).EndOfDay()),
            CalendarViewMode.Month => (SelectedDate.StartOfMonth(), SelectedDate.EndOfMonth()),
            _ => (SelectedDate.StartOfWeek(), SelectedDate.EndOfWeek()),
        };
    }

    // Grouped events by day for Schedule/Day views
    public IEnumerable<IGrouping<DateOnly, CalendarEvent>> EventsByDay =>
        Events.GroupBy(e => e.StartTime.ToDateOnly()).OrderBy(g => g.Key);

    // Events for a specific day in Month view
    public IEnumerable<CalendarEvent> GetEventsForDay(DateOnly date) =>
        Events.Where(e => e.StartTime.ToDateOnly() == date);
}
