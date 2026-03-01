using Hive.Core.Models;
using Hive.Dialogs;
using Hive.Services;
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
        UpdateRepeaters();
    }

    private void UpdateRepeaters()
    {
        ProfilesRepeater.ItemsSource = ViewModel.Profiles;
        SharedAccessRepeater.ItemsSource = ViewModel.SharedAccessList;
        DevicesRepeater.ItemsSource = ViewModel.Devices;
        CountdownsRepeater.ItemsSource = ViewModel.Countdowns;
    }

    private async void OnSave(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveSettingsCommand.ExecuteAsync(null);
    }

    private async void OnAddProfile(object sender, RoutedEventArgs e)
    {
        var dialog = new AddProfileDialog(FamilyId) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.CreateProfileAsync(dialog.Result);
            ProfilesRepeater.ItemsSource = ViewModel.Profiles;
        }
    }

    private void OnDarkModeToggled(object sender, RoutedEventArgs e)
    {
        var themeService = App.Services.GetRequiredService<ThemeService>();
        themeService.SetTheme(ViewModel.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light);
    }

    private async void OnSyncIcs(object sender, RoutedEventArgs e)
    {
        await ViewModel.SyncIcsCalendarCommand.ExecuteAsync(null);
    }

    private void OnViewProfile(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Profile profile)
            Frame.Navigate(typeof(ProfileDetailPage), profile);
    }

    // Parental Lock
    private async void OnParentalLockToggled(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ParentalLockEnabled && ViewModel.Settings?.ParentalLockPinHash is null)
        {
            // First time enabling — set a PIN
            await ShowSetPinDialog();
        }
    }

    private async void OnSetPin(object sender, RoutedEventArgs e)
    {
        await ShowSetPinDialog();
    }

    private async Task ShowSetPinDialog()
    {
        var dialog = new PinEntryDialog(isSettingPin: true) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.EnteredPin is not null)
        {
            await ViewModel.SetPinAsync(dialog.EnteredPin);
        }
    }

    // Share Access
    private async void OnInvite(object sender, RoutedEventArgs e)
    {
        var dialog = new InviteDialog(FamilyId) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.InviteAsync(dialog.Result);
            SharedAccessRepeater.ItemsSource = ViewModel.SharedAccessList;
        }
    }

    private async void OnRevokeAccess(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ViewModel.RevokeAccessCommand.ExecuteAsync(id);
            SharedAccessRepeater.ItemsSource = ViewModel.SharedAccessList;
        }
    }

    // Devices
    private async void OnAddDevice(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Add Device",
            PrimaryButtonText = "Register",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };

        var panel = new StackPanel { Spacing = 16, MinWidth = 300 };
        var nameInput = new TextBox { Header = "Device name", PlaceholderText = "e.g. Kitchen Calendar" };
        var sizeCombo = new ComboBox { Header = "Screen size", HorizontalAlignment = HorizontalAlignment.Stretch };
        sizeCombo.Items.Add(new ComboBoxItem { Content = "10\"", Tag = DeviceSize.Ten });
        sizeCombo.Items.Add(new ComboBoxItem { Content = "15\"", Tag = DeviceSize.Fifteen });
        sizeCombo.Items.Add(new ComboBoxItem { Content = "27\"", Tag = DeviceSize.TwentySeven });
        sizeCombo.SelectedIndex = 2;

        panel.Children.Add(nameInput);
        panel.Children.Add(sizeCombo);
        dialog.Content = panel;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameInput.Text))
        {
            var size = sizeCombo.SelectedItem is ComboBoxItem item && item.Tag is DeviceSize ds
                ? ds : DeviceSize.TwentySeven;

            await ViewModel.RegisterDeviceAsync(new Device
            {
                DisplayName = nameInput.Text.Trim(),
                Size = size,
            });
            DevicesRepeater.ItemsSource = ViewModel.Devices;
        }
    }

    private async void OnDeleteDevice(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ViewModel.DeleteDeviceCommand.ExecuteAsync(id);
            DevicesRepeater.ItemsSource = ViewModel.Devices;
        }
    }

    // Countdowns
    private async void OnAddCountdown(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Add Countdown",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };

        var panel = new StackPanel { Spacing = 16, MinWidth = 300 };
        var titleInput = new TextBox { Header = "Event name", PlaceholderText = "e.g. Summer Vacation" };
        var emojiInput = new TextBox { Header = "Emoji", PlaceholderText = "\U0001F389", MaxLength = 4 };
        var datePicker = new CalendarDatePicker { Header = "Target date", HorizontalAlignment = HorizontalAlignment.Stretch };

        panel.Children.Add(titleInput);
        panel.Children.Add(emojiInput);
        panel.Children.Add(datePicker);
        dialog.Content = panel;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary
            && !string.IsNullOrWhiteSpace(titleInput.Text)
            && datePicker.Date.HasValue)
        {
            await ViewModel.CreateCountdownAsync(new EventCountdown
            {
                Title = titleInput.Text.Trim(),
                Emoji = string.IsNullOrWhiteSpace(emojiInput.Text) ? "\U0001F389" : emojiInput.Text.Trim(),
                TargetDate = DateOnly.FromDateTime(datePicker.Date.Value.DateTime),
            });
            CountdownsRepeater.ItemsSource = ViewModel.Countdowns;
        }
    }

    private async void OnDeleteCountdown(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ViewModel.DeleteCountdownCommand.ExecuteAsync(id);
            CountdownsRepeater.ItemsSource = ViewModel.Countdowns;
        }
    }

    // Magic Import
    private async void OnRunImport(object sender, RoutedEventArgs e)
    {
        await ViewModel.RunMagicImportCommand.ExecuteAsync(null);
    }

    // Photo Screensaver
    private async void OnManagePhotos(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Photo Albums",
            CloseButtonText = "Close",
            XamlRoot = XamlRoot,
            Content = new TextBlock
            {
                Text = "Photo album management allows you to organize photos into albums. "
                     + "Active albums rotate as a screensaver when the calendar is idle.\n\n"
                     + "To add photos, place image files in the app's photo directory.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400,
            },
        };

        await dialog.ShowAsync();
    }

    // Instacart
    private async void OnSendToInstacart(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Instacart",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
            Content = new TextBlock
            {
                Text = "Instacart integration sends your grocery list items directly to an Instacart order. "
                     + "This feature is currently available in the US only.\n\n"
                     + "Go to the Lists page, select your Grocery list, then items will be synced to Instacart.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400,
            },
        };

        await dialog.ShowAsync();
    }
}
