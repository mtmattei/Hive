using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        ProfilesRepeater.ItemsSource = ViewModel.Profiles;
    }

    private async void OnSave(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveSettingsCommand.ExecuteAsync(null);
    }

    private void OnAddProfile(object sender, RoutedEventArgs e)
    {
        // TODO: Show add profile dialog
    }
}
