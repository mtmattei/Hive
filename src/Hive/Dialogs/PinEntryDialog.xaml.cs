using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class PinEntryDialog : ContentDialog
{
    public string? EnteredPin { get; private set; }
    public bool IsSettingPin { get; }

    public PinEntryDialog(bool isSettingPin = false)
    {
        InitializeComponent();
        IsSettingPin = isSettingPin;

        if (isSettingPin)
        {
            Title = "Set PIN";
            PromptText.Text = "Choose a 4-6 digit PIN for parental lock.";
            PrimaryButtonText = "Set PIN";
        }
    }

    private void OnConfirm(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var pin = PinInput.Password.Trim();
        if (pin.Length < 4 || pin.Length > 6 || !pin.All(char.IsDigit))
        {
            args.Cancel = true;
            ErrorText.Text = "PIN must be 4-6 digits.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        EnteredPin = pin;
    }
}
