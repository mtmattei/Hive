using Hive.Dialogs;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }
    private static readonly Guid FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000001");

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

    private async void OnAddProfile(object sender, RoutedEventArgs e)
    {
        var dialog = new AddProfileDialog(FamilyId)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateProfileAsync(dialog.Result);
            ProfilesRepeater.ItemsSource = ViewModel.Profiles;
        }
    }
}
