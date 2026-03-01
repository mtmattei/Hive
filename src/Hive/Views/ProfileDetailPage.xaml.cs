using Hive.Controls;
using Hive.Core.Models;
using Hive.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class ProfileDetailPage : Page
{
    private static readonly Guid FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ProfileDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Profile profile)
        {
            await LoadProfileAsync(profile);
        }
    }

    private async Task LoadProfileAsync(Profile profile)
    {
        PageTitle.Text = profile.Name;
        NameText.Text = profile.Name;
        AvatarEmoji.Text = profile.Emoji ?? profile.Name[..1].ToUpperInvariant();
        AvatarBorder.Background = HexToBrush(profile.Color);
        StarText.Text = profile.StarBalance.ToString();

        // Load upcoming events
        var calendarService = App.Services.GetRequiredService<ICalendarService>();
        var now = DateTimeOffset.Now;
        var events = await calendarService.GetEventsAsync(
            FamilyId, now, now.AddDays(7), [profile.Id]);

        EventsPanel.Children.Clear();
        if (events.Count > 0)
        {
            foreach (var evt in events.Take(5))
            {
                var card = new EventCard { Event = evt };
                EventsPanel.Children.Add(card);
            }
        }
        else
        {
            EventsPanel.Children.Add(new TextBlock
            {
                Text = "No upcoming events",
                Style = (Style)Resources["BodySecondaryStyle"],
            });
        }

        // Load assigned tasks
        var taskService = App.Services.GetRequiredService<ITaskService>();
        var tasks = await taskService.GetTasksAsync(FamilyId, profile.Id);

        TasksPanel.Children.Clear();
        if (tasks.Count > 0)
        {
            foreach (var task in tasks)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                row.Children.Add(new TextBlock { Text = task.Emoji ?? "\U0001f4cb", FontSize = 16 });
                row.Children.Add(new TextBlock
                {
                    Text = task.Title,
                    Style = (Style)Resources["BodyStyle"],
                    VerticalAlignment = VerticalAlignment.Center,
                });
                if (task.StarValue > 0)
                {
                    row.Children.Add(new TextBlock
                    {
                        Text = $"\u2b50 {task.StarValue}",
                        Style = (Style)Resources["SmallLabelStyle"],
                        Foreground = (Brush)Resources["OchreBrush"],
                        VerticalAlignment = VerticalAlignment.Center,
                    });
                }
                TasksPanel.Children.Add(row);
            }
        }
        else
        {
            TasksPanel.Children.Add(new TextBlock
            {
                Text = "No tasks assigned",
                Style = (Style)Resources["BodySecondaryStyle"],
            });
        }

        // Load rewards
        var rewardService = App.Services.GetRequiredService<IRewardService>();
        var rewards = await rewardService.GetRewardsAsync(FamilyId, profile.Id);

        RewardsPanel.Children.Clear();
        if (rewards.Count > 0)
        {
            foreach (var reward in rewards)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                row.Children.Add(new TextBlock { Text = reward.Emoji ?? "\U0001f3c6", FontSize = 16 });
                row.Children.Add(new TextBlock
                {
                    Text = reward.Title,
                    Style = (Style)Resources["BodyStyle"],
                    VerticalAlignment = VerticalAlignment.Center,
                });
                row.Children.Add(new TextBlock
                {
                    Text = $"\u2b50 {reward.StarCost}",
                    Style = (Style)Resources["SmallLabelStyle"],
                    Foreground = (Brush)Resources["OchreBrush"],
                    VerticalAlignment = VerticalAlignment.Center,
                });
                RewardsPanel.Children.Add(row);
            }
        }
        else
        {
            RewardsPanel.Children.Add(new TextBlock
            {
                Text = "No rewards available",
                Style = (Style)Resources["BodySecondaryStyle"],
            });
        }
    }

    private void OnBack(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }

    private static SolidColorBrush HexToBrush(string hex)
    {
        var r = Convert.ToByte(hex.Substring(1, 2), 16);
        var g = Convert.ToByte(hex.Substring(3, 2), 16);
        var b = Convert.ToByte(hex.Substring(5, 2), 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b));
    }
}
