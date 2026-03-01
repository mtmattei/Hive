using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class AddEventDialog : ContentDialog
{
    private readonly IReadOnlyList<Profile> _profiles;
    private Guid? _editingEventId;

    public CalendarEvent? Result { get; private set; }
    public bool IsEditing => _editingEventId.HasValue;

    public AddEventDialog(IReadOnlyList<Profile> profiles, DateTimeOffset? defaultDate = null)
    {
        InitializeComponent();
        _profiles = profiles;

        ProfileCombo.ItemsSource = profiles;
        if (profiles.Count > 0)
            ProfileCombo.SelectedIndex = 0;

        var date = defaultDate ?? DateTimeOffset.Now;
        DatePicker.Date = date;
        StartTimePicker.Time = new TimeSpan(date.Hour, 0, 0);
        EndTimePicker.Time = new TimeSpan(date.Hour + 1, 0, 0);
    }

    public void LoadEvent(CalendarEvent evt)
    {
        _editingEventId = evt.Id;
        Title = "Edit Event";
        PrimaryButtonText = "Save";
        TitleInput.Text = evt.Title;
        AllDayToggle.IsOn = evt.IsAllDay;
        DatePicker.Date = evt.StartTime;
        StartTimePicker.Time = evt.StartTime.TimeOfDay;
        EndTimePicker.Time = evt.EndTime?.TimeOfDay ?? evt.StartTime.AddHours(1).TimeOfDay;
        LocationInput.Text = evt.Location ?? string.Empty;
        NotesInput.Text = evt.Notes ?? string.Empty;

        var profileIndex = _profiles.ToList().FindIndex(p => p.Id == evt.ProfileId);
        if (profileIndex >= 0) ProfileCombo.SelectedIndex = profileIndex;

        RecurrenceControl.SetRule(evt.Recurrence);

        if (evt.ReminderMinutesBefore.HasValue)
        {
            for (var i = 0; i < ReminderCombo.Items.Count; i++)
            {
                if (ReminderCombo.Items[i] is ComboBoxItem item &&
                    item.Tag is string tag && int.Parse(tag) == evt.ReminderMinutesBefore.Value)
                {
                    ReminderCombo.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void OnAllDayToggled(object sender, RoutedEventArgs e)
    {
        TimePanel.Visibility = AllDayToggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(TitleInput.Text))
        {
            args.Cancel = true;
            TitleInput.Header = "Title (required)";
            return;
        }

        if (ProfileCombo.SelectedItem is not Profile profile)
        {
            args.Cancel = true;
            return;
        }

        var date = DatePicker.Date ?? DateTimeOffset.Now;
        var startTime = AllDayToggle.IsOn
            ? new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset)
            : new DateTimeOffset(date.Year, date.Month, date.Day,
                StartTimePicker.Time.Hours, StartTimePicker.Time.Minutes, 0, date.Offset);

        DateTimeOffset? endTime = AllDayToggle.IsOn ? null
            : new DateTimeOffset(date.Year, date.Month, date.Day,
                EndTimePicker.Time.Hours, EndTimePicker.Time.Minutes, 0, date.Offset);

        var reminderMinutes = ReminderCombo.SelectedItem is ComboBoxItem reminderItem
            && reminderItem.Tag is string reminderTag && int.TryParse(reminderTag, out var mins) && mins > 0
            ? mins : (int?)null;

        Result = new CalendarEvent
        {
            Id = _editingEventId ?? Guid.NewGuid(),
            FamilyAccountId = profile.FamilyAccountId,
            Title = TitleInput.Text.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            IsAllDay = AllDayToggle.IsOn,
            Location = string.IsNullOrWhiteSpace(LocationInput.Text) ? null : LocationInput.Text.Trim(),
            Notes = string.IsNullOrWhiteSpace(NotesInput.Text) ? null : NotesInput.Text.Trim(),
            ProfileId = profile.Id,
            Profile = profile,
            Recurrence = RecurrenceControl.GetRule(),
            ReminderMinutesBefore = reminderMinutes,
        };
    }
}
