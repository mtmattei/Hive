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
    private readonly ISharedAccessService _sharedAccessService;
    private readonly IDeviceService _deviceService;
    private readonly ICountdownService _countdownService;

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

    // Sleep mode
    [ObservableProperty] private bool _sleepScheduleEnabled;
    [ObservableProperty] private TimeSpan _sleepFromTime = new(22, 0, 0);
    [ObservableProperty] private TimeSpan _sleepToTime = new(7, 0, 0);

    // Sync
    [ObservableProperty] private string _syncIcsUrl = string.Empty;
    [ObservableProperty] private string? _syncStatusMessage;

    // Share access
    [ObservableProperty] private ObservableCollection<SharedAccess> _sharedAccessList = [];

    // Devices
    [ObservableProperty] private ObservableCollection<Device> _devices = [];

    // Countdowns
    [ObservableProperty] private ObservableCollection<EventCountdown> _countdowns = [];

    // Magic Import
    [ObservableProperty] private string _importText = string.Empty;
    [ObservableProperty] private string? _importStatusMessage;

    public SettingsViewModel(
        ISettingsService settingsService,
        IProfileService profileService,
        ThemeService themeService,
        ISharedAccessService sharedAccessService,
        IDeviceService deviceService,
        ICountdownService countdownService)
    {
        _settingsService = settingsService;
        _profileService = profileService;
        _themeService = themeService;
        _sharedAccessService = sharedAccessService;
        _deviceService = deviceService;
        _countdownService = countdownService;
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

            // Sleep mode
            SleepScheduleEnabled = settings.SleepScheduleEnabled;
            if (settings.SleepFrom.HasValue)
                SleepFromTime = settings.SleepFrom.Value.ToTimeSpan();
            if (settings.SleepTo.HasValue)
                SleepToTime = settings.SleepTo.Value.ToTimeSpan();

            var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
            Profiles = new ObservableCollection<Profile>(profiles);

            // Load share access
            var access = await _sharedAccessService.GetSharedAccessAsync(_familyAccountId);
            SharedAccessList = new ObservableCollection<SharedAccess>(access);

            // Load devices
            var devices = await _deviceService.GetDevicesAsync(_familyAccountId);
            Devices = new ObservableCollection<Device>(devices);

            // Load countdowns
            var countdowns = await _countdownService.GetCountdownsAsync(_familyAccountId);
            Countdowns = new ObservableCollection<EventCountdown>(countdowns);

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

        // Sleep mode
        Settings.SleepScheduleEnabled = SleepScheduleEnabled;
        Settings.SleepFrom = new TimeOnly(SleepFromTime.Hours, SleepFromTime.Minutes);
        Settings.SleepTo = new TimeOnly(SleepToTime.Hours, SleepToTime.Minutes);

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

    // Parental lock
    public async Task<bool> ValidatePinAsync(string pin) =>
        await _settingsService.ValidateParentalLockPinAsync(_familyAccountId, pin);

    public async Task SetPinAsync(string pin) =>
        await _settingsService.SetParentalLockPinAsync(_familyAccountId, pin);

    // Share access
    public async Task InviteAsync(SharedAccess invite)
    {
        await _sharedAccessService.InviteAsync(invite);
        var access = await _sharedAccessService.GetSharedAccessAsync(_familyAccountId);
        SharedAccessList = new ObservableCollection<SharedAccess>(access);
    }

    [RelayCommand]
    private async Task RevokeAccessAsync(Guid inviteId)
    {
        await _sharedAccessService.RevokeAccessAsync(inviteId);
        var access = await _sharedAccessService.GetSharedAccessAsync(_familyAccountId);
        SharedAccessList = new ObservableCollection<SharedAccess>(access);
    }

    // Devices
    public async Task RegisterDeviceAsync(Device device)
    {
        device.FamilyAccountId = _familyAccountId;
        await _deviceService.RegisterDeviceAsync(device);
        var devices = await _deviceService.GetDevicesAsync(_familyAccountId);
        Devices = new ObservableCollection<Device>(devices);
    }

    [RelayCommand]
    private async Task DeleteDeviceAsync(Guid deviceId)
    {
        await _deviceService.DeleteDeviceAsync(deviceId);
        var devices = await _deviceService.GetDevicesAsync(_familyAccountId);
        Devices = new ObservableCollection<Device>(devices);
    }

    // Countdowns
    public async Task CreateCountdownAsync(EventCountdown countdown)
    {
        countdown.FamilyAccountId = _familyAccountId;
        await _countdownService.CreateCountdownAsync(countdown);
        var countdowns = await _countdownService.GetCountdownsAsync(_familyAccountId);
        Countdowns = new ObservableCollection<EventCountdown>(countdowns);
    }

    [RelayCommand]
    private async Task DeleteCountdownAsync(Guid countdownId)
    {
        await _countdownService.DeleteCountdownAsync(countdownId);
        var countdowns = await _countdownService.GetCountdownsAsync(_familyAccountId);
        Countdowns = new ObservableCollection<EventCountdown>(countdowns);
    }

    // Magic import
    [RelayCommand]
    private async Task RunMagicImportAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportText))
        {
            ImportStatusMessage = "Paste or type content to import.";
            return;
        }

        try
        {
            ImportStatusMessage = "Parsing...";
            var importService = App.Services.GetService(typeof(IMagicImportService)) as IMagicImportService;
            if (importService is null) { ImportStatusMessage = "Import service not available"; return; }

            var defaultProfileId = Profiles.Count > 0 ? Profiles[0].Id : Guid.Empty;
            var result = await importService.ImportFromTextAsync(_familyAccountId, ImportText, defaultProfileId);

            ImportStatusMessage = result.Status == ImportStatus.Completed
                ? $"Imported {result.EventsCreated} event(s) successfully!"
                : $"Import failed: {result.ErrorMessage}";

            ImportText = string.Empty;
        }
        catch (Exception ex)
        {
            ImportStatusMessage = $"Import failed: {ex.Message}";
        }
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
