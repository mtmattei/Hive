using Hive.Core.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Hive.Dialogs;

public sealed partial class AddProfileDialog : ContentDialog
{
    private static readonly string[] AvailableColors =
    [
        "#C2553A", "#1B6B93", "#6A8E4E", "#D4923A", "#7B5EA7",
        "#2E86AB", "#A23B72", "#F18F01", "#C73E1D", "#3B1F2B",
        "#44BBA4", "#E94F37", "#393E41", "#D4A373", "#5C6B73",
    ];

    private string _selectedColor = AvailableColors[0];
    private Border? _selectedBorder;

    public Profile? Result { get; private set; }

    public AddProfileDialog(Guid familyAccountId)
    {
        InitializeComponent();
        _familyAccountId = familyAccountId;
        BuildColorPicker();
    }

    private readonly Guid _familyAccountId;

    private void BuildColorPicker()
    {
        var items = new List<FrameworkElement>();
        foreach (var hex in AvailableColors)
        {
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);

            var border = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b)),
                BorderThickness = new Thickness(3),
                BorderBrush = hex == _selectedColor
                    ? new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x1A, 0x1A, 0x1A))
                    : new SolidColorBrush(Colors.Transparent),
                Tag = hex,
            };

            border.PointerPressed += (s, _) =>
            {
                if (s is Border b && b.Tag is string color)
                {
                    _selectedColor = color;
                    if (_selectedBorder is not null)
                        _selectedBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    b.BorderBrush = new SolidColorBrush(
                        ColorHelper.FromArgb(0xFF, 0x1A, 0x1A, 0x1A));
                    _selectedBorder = b;
                }
            };

            if (hex == _selectedColor)
                _selectedBorder = border;

            items.Add(border);
        }

        ColorPicker.ItemsSource = items;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            args.Cancel = true;
            NameInput.Header = "Name (required)";
            return;
        }

        Result = new Profile
        {
            Id = Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            Name = NameInput.Text.Trim(),
            Color = _selectedColor,
            Emoji = string.IsNullOrWhiteSpace(EmojiInput.Text) ? null : EmojiInput.Text.Trim(),
        };
    }
}
