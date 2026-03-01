using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Controls;

public sealed partial class EventCard : UserControl
{
    public static readonly DependencyProperty EventProperty =
        DependencyProperty.Register(nameof(Event), typeof(CalendarEvent), typeof(EventCard),
            new PropertyMetadata(null, OnEventChanged));

    public CalendarEvent? Event
    {
        get => (CalendarEvent?)GetValue(EventProperty);
        set => SetValue(EventProperty, value);
    }

    public string Title => Event?.Title ?? string.Empty;
    public string ProfileColor => Event?.Profile?.Color ?? "#9A9A9A";
    public string ProfileInitial => Event?.Profile?.Name?.Length > 0
        ? Event.Profile.Name[..1].ToUpper() : "?";
    public string TimeDisplay => Event is null ? string.Empty
        : Event.IsAllDay ? "All Day"
        : $"{Event.StartTime:h:mm tt}" + (Event.EndTime.HasValue ? $" – {Event.EndTime:h:mm tt}" : "");
    public Visibility IsTimedEvent => Event?.IsAllDay == false
        ? Visibility.Visible : Visibility.Collapsed;

    public event EventHandler<CalendarEvent>? EventTapped;

    public EventCard()
    {
        InitializeComponent();
        Tapped += (_, _) =>
        {
            if (Event is not null)
                EventTapped?.Invoke(this, Event);
        };
    }

    private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EventCard card)
        {
            card.Bindings.Update();
        }
    }
}
