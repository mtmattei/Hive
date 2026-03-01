using System.Collections.ObjectModel;
using Hive.Core.Extensions;
using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Hive.Controls;

public sealed partial class ScheduleView : UserControl
{
    public static readonly DependencyProperty EventsProperty =
        DependencyProperty.Register(nameof(Events), typeof(ObservableCollection<CalendarEvent>),
            typeof(ScheduleView), new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(nameof(SelectedDate), typeof(DateTimeOffset),
            typeof(ScheduleView), new PropertyMetadata(DateTimeOffset.Now, OnDataChanged));

    public static readonly DependencyProperty DayCountProperty =
        DependencyProperty.Register(nameof(DayCount), typeof(int),
            typeof(ScheduleView), new PropertyMetadata(5, OnDataChanged));

    public ObservableCollection<CalendarEvent>? Events
    {
        get => (ObservableCollection<CalendarEvent>?)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    public DateTimeOffset SelectedDate
    {
        get => (DateTimeOffset)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public int DayCount
    {
        get => (int)GetValue(DayCountProperty);
        set => SetValue(DayCountProperty, value);
    }

    public ScheduleView()
    {
        InitializeComponent();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScheduleView view)
            view.RenderColumns();
    }

    private void RenderColumns()
    {
        ColumnsGrid.Children.Clear();
        ColumnsGrid.ColumnDefinitions.Clear();

        var today = DateTimeOffset.Now.ToDateOnly();

        for (var i = 0; i < DayCount; i++)
        {
            ColumnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 200 });

            var date = SelectedDate.AddDays(i);
            var dateOnly = date.ToDateOnly();
            var isToday = dateOnly == today;

            var dayEvents = Events?
                .Where(e => e.StartTime.ToDateOnly() == dateOnly)
                .OrderBy(e => e.IsAllDay ? 0 : 1)
                .ThenBy(e => e.StartTime)
                .ToList() ?? [];

            // Day column
            var column = new Border
            {
                Background = isToday
                    ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xF2, 0xF9, 0xFB))
                    : new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Margin = new Thickness(4, 0, 4, 0),
                BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xF0, 0xEE, 0xEA)),
                BorderThickness = new Thickness(1),
            };

            var stack = new StackPanel { Spacing = 8 };

            // Day header
            var headerStack = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 0, 8) };

            var dayName = new TextBlock
            {
                Text = date.ToString("ddd").ToUpper(),
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                CharacterSpacing = 60,
                Foreground = isToday
                    ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x1B, 0x6B, 0x93))
                    : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x5A, 0x5A, 0x5A)),
            };

            var dayNumber = new TextBlock
            {
                Text = date.Day.ToString(),
                FontSize = 32,
                FontWeight = Microsoft.UI.Text.FontWeights.ExtraBold,
                Foreground = isToday
                    ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x1B, 0x6B, 0x93))
                    : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x1A, 0x1A, 0x1A)),
            };

            headerStack.Children.Add(dayName);
            headerStack.Children.Add(dayNumber);
            stack.Children.Add(headerStack);

            // Event cards
            if (dayEvents.Count == 0)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "No events",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x9A, 0x9A, 0x9A)),
                    Margin = new Thickness(0, 8, 0, 0),
                });
            }
            else
            {
                foreach (var evt in dayEvents)
                {
                    var card = new EventCard { Event = evt };
                    stack.Children.Add(card);
                }
            }

            column.Child = stack;
            Grid.SetColumn(column, i);
            ColumnsGrid.Children.Add(column);
        }
    }
}
