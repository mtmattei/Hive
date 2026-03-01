using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Hive.Controls;

public sealed partial class RecurrenceEditor : UserControl
{
    private readonly ToggleButton[] _dayButtons;

    public RecurrenceEditor()
    {
        InitializeComponent();
        _dayButtons = [DaySun, DayMon, DayTue, DayWed, DayThu, DayFri, DaySat];
    }

    public bool IsRecurring => EnableRepeat.IsOn;

    public RecurrenceRule? GetRule()
    {
        if (!EnableRepeat.IsOn)
            return null;

        var frequency = FrequencyCombo.SelectedIndex switch
        {
            0 => RecurrenceFrequency.Daily,
            1 => RecurrenceFrequency.Weekly,
            2 => RecurrenceFrequency.Monthly,
            3 => RecurrenceFrequency.Yearly,
            _ => RecurrenceFrequency.Daily,
        };

        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = frequency,
            Interval = (int)IntervalBox.Value,
        };

        // Days of week for weekly
        if (frequency == RecurrenceFrequency.Weekly)
        {
            var days = new List<DayOfWeek>();
            for (var i = 0; i < 7; i++)
            {
                if (_dayButtons[i].IsChecked == true)
                    days.Add((DayOfWeek)i);
            }
            if (days.Count > 0)
                rule.DaysOfWeek = days.ToArray();
        }

        // Repeat until
        if (RepeatUntilPicker.Date.HasValue)
        {
            rule.RepeatUntil = RepeatUntilPicker.Date.Value;
        }

        return rule;
    }

    public void SetRule(RecurrenceRule? rule)
    {
        if (rule is null)
        {
            EnableRepeat.IsOn = false;
            return;
        }

        EnableRepeat.IsOn = true;
        FrequencyCombo.SelectedIndex = (int)rule.Frequency;
        IntervalBox.Value = rule.Interval;

        if (rule.DaysOfWeek is not null)
        {
            foreach (var day in rule.DaysOfWeek)
                _dayButtons[(int)day].IsChecked = true;
        }

        if (rule.RepeatUntil.HasValue)
            RepeatUntilPicker.Date = rule.RepeatUntil.Value;
    }

    private void OnRepeatToggled(object sender, RoutedEventArgs e)
    {
        RecurrenceOptions.Visibility = EnableRepeat.IsOn
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnFrequencyChanged(object sender, SelectionChangedEventArgs e)
    {
        DaysOfWeekPanel.Visibility = FrequencyCombo.SelectedIndex == 1
            ? Visibility.Visible : Visibility.Collapsed;
    }
}
