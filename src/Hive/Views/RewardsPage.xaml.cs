using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class RewardsPage : Page
{
    private RewardsViewModel ViewModel { get; }

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

    private void OnAddReward(object sender, RoutedEventArgs e)
    {
        // TODO: Show add reward dialog
    }

    private void UpdateUI()
    {
        ProfileRewardsRepeater.ItemsSource = ViewModel.Profiles;
        EmptyState.Visibility = ViewModel.State == ViewState.Empty
            ? Visibility.Visible : Visibility.Collapsed;
    }
}
