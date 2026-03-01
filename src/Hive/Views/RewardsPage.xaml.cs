using Hive.Controls;
using Hive.Core.Models;
using Hive.Dialogs;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class RewardsPage : Page
{
    private RewardsViewModel ViewModel { get; }
    private static readonly Guid FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public RewardsPage()
    {
        ViewModel = App.Services.GetRequiredService<RewardsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        UpdateUI();
    }

    private async void OnAddReward(object sender, RoutedEventArgs e)
    {
        var dialog = new AddRewardDialog(ViewModel.Profiles.ToList(), FamilyId)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateRewardAsync(dialog.Result, dialog.SelectedProfileIds);
            UpdateUI();
        }
    }

    private async void OnEditReward(Reward reward)
    {
        var dialog = new AddRewardDialog(ViewModel.Profiles.ToList(), FamilyId)
        {
            XamlRoot = XamlRoot,
        };
        dialog.LoadReward(reward);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.UpdateRewardAsync(dialog.Result, dialog.SelectedProfileIds);
            UpdateUI();
        }
    }

    private async void OnDeleteReward(Reward reward)
    {
        var confirm = new ContentDialog
        {
            Title = "Delete Reward",
            Content = $"Delete \"{reward.Title}\"? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteRewardCommand.ExecuteAsync(reward.Id);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        EmptyState.Visibility = ViewModel.State == ViewState.Empty
            ? Visibility.Visible : Visibility.Collapsed;

        ProfileRewardsPanel.Children.Clear();

        foreach (var profile in ViewModel.Profiles)
        {
            var section = BuildProfileSection(profile);
            ProfileRewardsPanel.Children.Add(section);
        }
    }

    private StackPanel BuildProfileSection(Profile profile)
    {
        var section = new StackPanel { Spacing = 12 };

        // Profile header
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var avatar = new Border
        {
            Width = 32, Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = HexToBrush(profile.Color),
            Margin = new Thickness(0, 0, 12, 0),
        };
        var initial = new TextBlock
        {
            Text = profile.Name.Length > 0 ? profile.Name[..1].ToUpperInvariant() : "?",
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        avatar.Child = initial;
        Grid.SetColumn(avatar, 0);
        headerGrid.Children.Add(avatar);

        var nameText = new TextBlock
        {
            Text = profile.Name,
            Style = (Style)Resources["SectionHeadingStyle"],
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(nameText, 1);
        headerGrid.Children.Add(nameText);

        var starPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
        };
        starPanel.Children.Add(new TextBlock { Text = "\u2b50", FontSize = 16 });
        starPanel.Children.Add(new TextBlock
        {
            Text = profile.StarBalance.ToString(),
            Style = (Style)Resources["BodySemiBoldStyle"],
            Foreground = (Brush)Resources["OchreBrush"],
        });
        Grid.SetColumn(starPanel, 2);
        headerGrid.Children.Add(starPanel);

        section.Children.Add(headerGrid);

        // Reward cards for this profile
        var rewards = ViewModel.GetRewardsForProfile(profile.Id).ToList();
        if (rewards.Count > 0)
        {
            foreach (var reward in rewards)
            {
                var card = new RewardCard
                {
                    Reward = reward,
                    StarBalance = profile.StarBalance,
                };
                card.RedeemClicked += async (_, r) =>
                {
                    await ViewModel.RedeemRewardCommand.ExecuteAsync(
                        new RedeemRequest(r.Id, profile.Id));
                    UpdateUI();
                };
                card.EditClicked += (_, r) => OnEditReward(r);
                section.Children.Add(card);
            }
        }
        else
        {
            section.Children.Add(new TextBlock
            {
                Text = "No rewards set up yet",
                Style = (Style)Resources["BodySecondaryStyle"],
                Margin = new Thickness(44, 0, 0, 0),
            });
        }

        return section;
    }

    private static SolidColorBrush HexToBrush(string hex)
    {
        var r = Convert.ToByte(hex.Substring(1, 2), 16);
        var g = Convert.ToByte(hex.Substring(3, 2), 16);
        var b = Convert.ToByte(hex.Substring(5, 2), 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b));
    }
}
