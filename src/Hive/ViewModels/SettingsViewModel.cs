using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Models;
using Hive.Core.Services;

namespace Hive.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IProfileService _profileService;

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
    [ObservableProperty] private int _startWeekOnIndex; // 0=Sunday, 1=Monday

    public SettingsViewModel(ISettingsService settingsService, IProfileService profileService)
    {
        _settingsService = settingsService;
        _profileService = profileService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;

            var settings = await _settingsService.GetSettingsAsync(_familyAccountId);
            Settings = settings;

            // Populate bindable properties
            ScheduleViewDays = settings.ScheduleViewDays;
            StartOnCurrentDay = settings.StartOnCurrentDay;
            DimPastEvents = settings.DimPastEvents;
            ShadeWeekends = settings.ShadeWeekends;
            PreviewChoresInCalendar = settings.PreviewChoresInCalendar;
            ParentalLockEnabled = settings.ParentalLockEnabled;
            StartWeekOnIndex = settings.StartWeekOn == DayOfWeek.Monday ? 1 : 0;

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
}
