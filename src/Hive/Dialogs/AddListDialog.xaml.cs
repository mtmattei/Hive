using Hive.Core.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Hive.Dialogs;

public sealed partial class AddListDialog : ContentDialog
{
    private static readonly string[] ListColors =
    [
        "#1B6B93", "#C2553A", "#6A8E4E", "#D4923A", "#7B5EA7",
        "#2E86AB", "#E94F37", "#44BBA4",
    ];

    private string _selectedColor = ListColors[0];
    private Border? _selectedBorder;

    public CustomList? Result { get; private set; }

    public AddListDialog(Guid familyAccountId)
    {
        InitializeComponent();
        _familyAccountId = familyAccountId;
        BuildColorRow();
    }

    private readonly Guid _familyAccountId;

    private void BuildColorRow()
    {
        foreach (var hex in ListColors)
        {
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);

            var circle = new Border
            {
                Width = 32, Height = 32, CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b)),
                BorderThickness = new Thickness(3),
                BorderBrush = hex == _selectedColor
                    ? new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x1A, 0x1A, 0x1A))
                    : new SolidColorBrush(Colors.Transparent),
                Tag = hex,
            };

            circle.PointerPressed += (s, _) =>
            {
                if (s is Border bd && bd.Tag is string color)
                {
                    _selectedColor = color;
                    if (_selectedBorder is not null)
                        _selectedBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    bd.BorderBrush = new SolidColorBrush(
                        ColorHelper.FromArgb(0xFF, 0x1A, 0x1A, 0x1A));
                    _selectedBorder = bd;
                }
            };

            if (hex == _selectedColor)
                _selectedBorder = circle;

            ColorRow.Children.Add(circle);
        }
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(TitleInput.Text))
        {
            args.Cancel = true;
            TitleInput.Header = "List name (required)";
            return;
        }

        Result = new CustomList
        {
            Id = Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            Title = TitleInput.Text.Trim(),
            Type = TypeCombo.SelectedIndex == 1 ? ListType.Grocery : ListType.ToDo,
            Color = _selectedColor,
        };
    }
}
