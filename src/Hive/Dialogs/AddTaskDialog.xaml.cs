using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class AddTaskDialog : ContentDialog
{
    private readonly IReadOnlyList<Profile> _profiles;
    private readonly ToggleButton[] _routineDayButtons;

    public TaskItem? Result { get; private set; }
    public IReadOnlyList<Guid> SelectedProfileIds { get; private set; } = [];

    public AddTaskDialog(IReadOnlyList<Profile> profiles, Guid familyAccountId)
    {
        InitializeComponent();
        _profiles = profiles;
        _routineDayButtons = [RoutineSun, RoutineMon, RoutineTue, RoutineWed, RoutineThu, RoutineFri, RoutineSat];
        ProfileCheckList.ItemsSource = profiles;
        _familyAccountId = familyAccountId;
    }

    private readonly Guid _familyAccountId;

    private void OnTypeChanged(object sender, RoutedEventArgs e)
    {
        var isChore = ChoreRadio.IsChecked == true;
        ChoreOptions.Visibility = isChore ? Visibility.Visible : Visibility.Collapsed;
        RoutineOptions.Visibility = isChore ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(TitleInput.Text))
        {
            args.Cancel = true;
            TitleInput.Header = "Title (required)";
            return;
        }

        var isChore = ChoreRadio.IsChecked == true;

        // Collect selected profile IDs from checkboxes
        var selectedIds = new List<Guid>();
        for (var i = 0; i < _profiles.Count; i++)
        {
            // Find the checkbox in the ItemsRepeater
            var element = ProfileCheckList.TryGetElement(i);
            if (element is CheckBox cb && cb.IsChecked == true && cb.Tag is Guid id)
            {
                selectedIds.Add(id);
            }
        }

        if (selectedIds.Count == 0)
        {
            args.Cancel = true;
            return;
        }

        SelectedProfileIds = selectedIds;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            Type = isChore ? TaskItemType.Chore : TaskItemType.Routine,
            Title = TitleInput.Text.Trim(),
            Emoji = string.IsNullOrWhiteSpace(EmojiInput.Text) ? null : EmojiInput.Text.Trim(),
            StarValue = (int)StarValueBox.Value,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (isChore)
        {
            if (DueDatePicker.Date.HasValue)
                task.DueDate = DueDatePicker.Date.Value;
            task.DueTime = DueTimePicker.Time != TimeSpan.Zero
                ? TimeOnly.FromTimeSpan(DueTimePicker.Time) : null;
            task.Recurrence = ChoreRecurrenceControl.GetRule();
        }
        else
        {
            task.RoutineTimeOfDay = TimeOfDayCombo.SelectedIndex switch
            {
                0 => RoutineTimeOfDay.Morning,
                1 => RoutineTimeOfDay.Afternoon,
                2 => RoutineTimeOfDay.Evening,
                _ => RoutineTimeOfDay.Morning,
            };

            var days = new List<DayOfWeek>();
            for (var i = 0; i < 7; i++)
            {
                if (_routineDayButtons[i].IsChecked == true)
                    days.Add((DayOfWeek)i);
            }
            task.RoutineDays = days.Count > 0 ? days.ToArray() : null;
        }

        Result = task;
    }
}
