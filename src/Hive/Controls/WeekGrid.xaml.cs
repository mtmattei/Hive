using System.Collections.ObjectModel;
using Hive.Core.Extensions;
using Hive.Core.Models;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Hive.Controls;

public sealed partial class WeekGrid : UserControl
{
    public static readonly DependencyProperty EventsProperty =
        DependencyProperty.Register(nameof(Events), typeof(ObservableCollection<CalendarEvent>),
            typeof(WeekGrid), new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(nameof(SelectedDate), typeof(DateTimeOffset),
            typeof(WeekGrid), new PropertyMetadata(DateTimeOffset.Now, OnDataChanged));

    public static readonly DependencyProperty StartWeekOnProperty =
        DependencyProperty.Register(nameof(StartWeekOn), typeof(DayOfWeek),
            typeof(WeekGrid), new PropertyMetadata(DayOfWeek.Sunday, OnDataChanged));

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

    public DayOfWeek StartWeekOn
    {
        get => (DayOfWeek)GetValue(StartWeekOnProperty);
        set => SetValue(StartWeekOnProperty, value);
    }

    private const double HourHeight = 60;

    public WeekGrid()
    {
        InitializeComponent();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WeekGrid grid) grid.Render();
    }

    private void Render()
    {
        RenderDayHeaders();
        RenderTimeGrid();
    }

    private void RenderDayHeaders()
    {
        DayHeadersGrid.Children.Clear();
        DayHeadersGrid.ColumnDefinitions.Clear();

        // Time label column
        DayHeadersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

        var weekStart = SelectedDate.StartOfWeek(StartWeekOn);
        var today = DateTimeOffset.Now.ToDateOnly();

        for (var i = 0; i < 7; i++)
        {
            DayHeadersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var date = weekStart.AddDays(i);
            var isToday = date.ToDateOnly() == today;

            var header = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 2,
            };

            header.Children.Add(new TextBlock
            {
                Text = date.ToString("ddd").ToUpper(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(isToday
                    ? ColorHelper.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)
                    : ColorHelper.FromArgb(0xAA, 0xFF, 0xFF, 0xFF)),
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            if (isToday)
            {
                var badge = new Border
                {
                    Width = 28, Height = 28, CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xC2, 0x55, 0x3A)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                badge.Child = new TextBlock
                {
                    Text = date.Day.ToString(),
                    FontSize = 13, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                header.Children.Add(badge);
            }
            else
            {
                header.Children.Add(new TextBlock
                {
                    Text = date.Day.ToString(),
                    FontSize = 14, FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            }

            Grid.SetColumn(header, i + 1);
            DayHeadersGrid.Children.Add(header);
        }
    }

    private void RenderTimeGrid()
    {
        TimeGrid.Children.Clear();
        TimeGrid.ColumnDefinitions.Clear();
        TimeGrid.Height = 24 * HourHeight;

        // Time label column + 7 day columns
        TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        for (var i = 0; i < 7; i++)
            TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Hour labels and gridlines
        for (var hour = 0; hour < 24; hour++)
        {
            var label = new TextBlock
            {
                Text = new TimeOnly(hour, 0).ToString("h tt"),
                FontSize = 10,
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9A, 0x9A, 0x9A)),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8, hour * HourHeight, 4, 0),
            };
            Grid.SetColumn(label, 0);
            TimeGrid.Children.Add(label);

            // Gridline
            var line = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xF0, 0xEE, 0xEA)),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, hour * HourHeight, 0, 0),
            };
            Grid.SetColumn(line, 0);
            Grid.SetColumnSpan(line, 8);
            TimeGrid.Children.Add(line);
        }

        // Current time indicator
        var now = DateTimeOffset.Now;
        var weekStart = SelectedDate.StartOfWeek(StartWeekOn);
        var todayIndex = (int)(now.Date - weekStart.Date).TotalDays;

        if (todayIndex is >= 0 and < 7)
        {
            var minutesFromMidnight = now.Hour * 60 + now.Minute;
            var yPos = minutesFromMidnight * (HourHeight / 60);

            var timeLine = new Rectangle
            {
                Height = 2,
                Fill = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xD4, 0x92, 0x3A)),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, yPos, 0, 0),
            };
            Grid.SetColumn(timeLine, todayIndex + 1);
            TimeGrid.Children.Add(timeLine);
        }

        // Place events
        if (Events is null) return;

        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
        {
            var date = weekStart.AddDays(dayIndex).ToDateOnly();
            var dayEvents = Events
                .Where(e => !e.IsAllDay && e.StartTime.ToDateOnly() == date)
                .OrderBy(e => e.StartTime)
                .ToList();

            foreach (var evt in dayEvents)
            {
                var startMinutes = evt.StartTime.Hour * 60 + evt.StartTime.Minute;
                var endMinutes = evt.EndTime.HasValue
                    ? evt.EndTime.Value.Hour * 60 + evt.EndTime.Value.Minute
                    : startMinutes + 60;
                var duration = Math.Max(endMinutes - startMinutes, 15);

                var yStart = startMinutes * (HourHeight / 60);
                var height = duration * (HourHeight / 60);

                var card = new EventCard
                {
                    Event = evt,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(2, yStart, 2, 0),
                    Height = height,
                };

                Grid.SetColumn(card, dayIndex + 1);
                TimeGrid.Children.Add(card);
            }
        }
    }
}
