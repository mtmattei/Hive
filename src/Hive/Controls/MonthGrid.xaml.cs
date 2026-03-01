using System.Collections.ObjectModel;
using Hive.Core.Extensions;
using Hive.Core.Models;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Hive.Controls;

public sealed partial class MonthGrid : UserControl
{
    public static readonly DependencyProperty EventsProperty =
        DependencyProperty.Register(nameof(Events), typeof(ObservableCollection<CalendarEvent>),
            typeof(MonthGrid), new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(nameof(SelectedDate), typeof(DateTimeOffset),
            typeof(MonthGrid), new PropertyMetadata(DateTimeOffset.Now, OnDataChanged));

    public static readonly DependencyProperty StartWeekOnProperty =
        DependencyProperty.Register(nameof(StartWeekOn), typeof(DayOfWeek),
            typeof(MonthGrid), new PropertyMetadata(DayOfWeek.Sunday, OnDataChanged));

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

    private const int MaxEventChips = 3;

    public MonthGrid()
    {
        InitializeComponent();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MonthGrid grid) grid.Render();
    }

    private void Render()
    {
        RenderDowHeaders();
        RenderDayCells();
    }

    private void RenderDowHeaders()
    {
        DowHeaderGrid.Children.Clear();
        DowHeaderGrid.ColumnDefinitions.Clear();

        for (var i = 0; i < 7; i++)
        {
            DowHeaderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var dow = (DayOfWeek)(((int)StartWeekOn + i) % 7);
            var label = new TextBlock
            {
                Text = dow.ToString()[..3].ToUpper(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                CharacterSpacing = 60,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetColumn(label, i);
            DowHeaderGrid.Children.Add(label);
        }
    }

    private void RenderDayCells()
    {
        DayCellsGrid.Children.Clear();
        DayCellsGrid.ColumnDefinitions.Clear();
        DayCellsGrid.RowDefinitions.Clear();

        for (var i = 0; i < 7; i++)
            DayCellsGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < 6; i++)
            DayCellsGrid.RowDefinitions.Add(
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 80 });

        var monthStart = SelectedDate.StartOfMonth();
        var firstDay = monthStart.DayOfWeek;
        var offset = ((int)firstDay - (int)StartWeekOn + 7) % 7;
        var gridStart = monthStart.AddDays(-offset);
        var today = DateTimeOffset.Now.ToDateOnly();

        for (var row = 0; row < 6; row++)
        {
            for (var col = 0; col < 7; col++)
            {
                var cellDate = gridStart.AddDays(row * 7 + col);
                var dateOnly = cellDate.ToDateOnly();
                var isCurrentMonth = cellDate.Month == SelectedDate.Month;
                var isToday = dateOnly == today;
                var isWeekend = cellDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

                var cell = new Border
                {
                    BorderBrush = new SolidColorBrush(
                        ColorHelper.FromArgb(0xFF, 0xF0, 0xEE, 0xEA)),
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(4),
                    Background = isToday
                        ? new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xE8, 0xF4, 0xF8))
                        : isWeekend
                            ? new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xF5, 0xF3, 0xEE))
                            : new SolidColorBrush(Colors.Transparent),
                    Opacity = isCurrentMonth ? 1.0 : 0.4,
                };

                var stack = new StackPanel { Spacing = 2 };

                // Day number
                if (isToday)
                {
                    var badge = new Border
                    {
                        Width = 24, Height = 24, CornerRadius = new CornerRadius(12),
                        Background = new SolidColorBrush(
                            ColorHelper.FromArgb(0xFF, 0x1B, 0x6B, 0x93)),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 2),
                    };
                    badge.Child = new TextBlock
                    {
                        Text = cellDate.Day.ToString(),
                        FontSize = 11, FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    stack.Children.Add(badge);
                }
                else
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = cellDate.Day.ToString(),
                        FontSize = 12,
                        Foreground = new SolidColorBrush(
                            ColorHelper.FromArgb(0xFF, 0x5A, 0x5A, 0x5A)),
                        Margin = new Thickness(2, 0, 0, 2),
                    });
                }

                // Event chips
                var dayEvents = Events?
                    .Where(e => e.StartTime.ToDateOnly() == dateOnly)
                    .Take(MaxEventChips + 1)
                    .ToList() ?? [];

                for (var idx = 0; idx < Math.Min(dayEvents.Count, MaxEventChips); idx++)
                {
                    var evt = dayEvents[idx];
                    var profileColor = evt.Profile?.Color ?? "#9A9A9A";
                    var r = Convert.ToByte(profileColor.Substring(1, 2), 16);
                    var g = Convert.ToByte(profileColor.Substring(3, 2), 16);
                    var b = Convert.ToByte(profileColor.Substring(5, 2), 16);

                    var chip = new Border
                    {
                        Background = new SolidColorBrush(
                            Windows.UI.Color.FromArgb(30, r, g, b)),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(4, 1, 4, 1),
                        Margin = new Thickness(0, 0, 0, 1),
                    };
                    chip.Child = new TextBlock
                    {
                        Text = evt.Title,
                        FontSize = 10,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxLines = 1,
                        Foreground = new SolidColorBrush(
                            Windows.UI.Color.FromArgb(0xFF, r, g, b)),
                    };
                    stack.Children.Add(chip);
                }

                if (dayEvents.Count > MaxEventChips)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"+ {dayEvents.Count - MaxEventChips} more",
                        FontSize = 9,
                        Foreground = new SolidColorBrush(
                            ColorHelper.FromArgb(0xFF, 0x9A, 0x9A, 0x9A)),
                    });
                }

                cell.Child = stack;
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                DayCellsGrid.Children.Add(cell);
            }
        }
    }
}
