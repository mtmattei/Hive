using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Services;

namespace Hive.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IProfileService _profileService;
    private readonly ThemeService _themeService;

    private readonly Guid _familyAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty] private ViewState _state = ViewState.Loading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private CalendarSettings? _settings;
    [ObservableProperty] private ObservableCollection<Profile> _profiles = [];

    // Bindable settings properties
    [ObservableProperty] private int _scheduleViewDays = 5;
    [ObservableProperty] private bool _startOnCurrentDay = true;
    [ObservableProperty] private bool _dimPastEvents = true;
    [ObservableProperty] private bool _shadeWeekends;
    [ObservableProperty] private bool _previewChoresInCalendar;
    [ObservableProperty] private bool _parentalLockEnabled;
    [ObservableProperty] private int _startWeekOnIndex;
    [ObservableProperty] private bool _isDarkMode;

    // Sync
    [ObservableProperty] private string _syncIcsUrl = string.Empty;
    [ObservableProperty] private string? _syncStatusMessage;

    public SettingsViewModel(
        ISettingsService settingsService,
        IProfileService profileService,
        ThemeService themeService)
    {
        _settingsService = settingsService;
        _profileService = profileService;
        _themeService = themeService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;

            var settings = await _settingsService.GetSettingsAsync(_familyAccountId);
            Settings = settings;

            ScheduleViewDays = settings.ScheduleViewDays;
            StartOnCurrentDay = settings.StartOnCurrentDay;
            DimPastEvents = settings.DimPastEvents;
            ShadeWeekends = settings.ShadeWeekends;
            PreviewChoresInCalendar = settings.PreviewChoresInCalendar;
            ParentalLockEnabled = settings.ParentalLockEnabled;
            StartWeekOnIndex = settings.StartWeekOn == DayOfWeek.Monday ? 1 : 0;
            IsDarkMode = _themeService.IsDarkMode;

            var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
            Profiles = new ObservableCollection<Profile>(profiles);

            State = ViewState.Loaded;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (Settings is null) return;

        Settings.ScheduleViewDays = ScheduleViewDays;
        Settings.StartOnCurrentDay = StartOnCurrentDay;
        Settings.DimPastEvents = DimPastEvents;
        Settings.ShadeWeekends = ShadeWeekends;
        Settings.PreviewChoresInCalendar = PreviewChoresInCalendar;
        Settings.ParentalLockEnabled = ParentalLockEnabled;
        Settings.StartWeekOn = StartWeekOnIndex == 1 ? DayOfWeek.Monday : DayOfWeek.Sunday;

        await _settingsService.UpdateSettingsAsync(Settings);
    }

    [RelayCommand]
    private void ToggleDarkMode()
    {
        _themeService.ToggleDarkMode();
        IsDarkMode = _themeService.IsDarkMode;
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(Guid profileId)
    {
        await _profileService.DeleteProfileAsync(profileId);
        var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
        Profiles = new ObservableCollection<Profile>(profiles);
    }

    public async Task CreateProfileAsync(Profile profile)
    {
        await _profileService.CreateProfileAsync(profile);
        var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
        Profiles = new ObservableCollection<Profile>(profiles);
    }

    [RelayCommand]
    private async Task SyncIcsCalendarAsync()
    {
        if (string.IsNullOrWhiteSpace(SyncIcsUrl))
        {
            SyncStatusMessage = "Please enter an ICS URL";
            return;
        }

        try
        {
            SyncStatusMessage = "Syncing...";
            var syncService = App.Services.GetService(typeof(ISyncService)) as ISyncService;
            if (syncService is null)
            {
                SyncStatusMessage = "Sync service not available";
                return;
            }

            var calendar = new SyncedCalendar
            {
                Id = Guid.NewGuid(),
                FamilyAccountId = _familyAccountId,
                Provider = CalendarProvider.IcsUrl,
                Direction = SyncDirection.OneWay,
                ExternalCalendarId = SyncIcsUrl.Trim(),
                ExternalCalendarName = "ICS Feed",
            };

            // Select first profile as default
            if (Profiles.Count > 0)
                calendar.LinkedProfileId = Profiles[0].Id;

            var result = await syncService.PullEventsAsync(calendar);

            SyncStatusMessage = result.Errors.Count > 0
                ? $"Sync completed with errors: {string.Join(", ", result.Errors)}"
                : $"Synced {result.EventsPulled} events successfully";
        }
        catch (Exception ex)
        {
            SyncStatusMessage = $"Sync failed: {ex.Message}";
        }
    }
}
