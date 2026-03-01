using Hive.Core.Models;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class AddRewardDialog : ContentDialog
{
    private readonly IReadOnlyList<Profile> _profiles;
    private readonly Guid _familyAccountId;

    public Reward? Result { get; private set; }
    public IReadOnlyList<Guid> SelectedProfileIds { get; private set; } = [];

    public AddRewardDialog(IReadOnlyList<Profile> profiles, Guid familyAccountId)
    {
        InitializeComponent();
        _profiles = profiles;
        _familyAccountId = familyAccountId;
        ProfileCheckList.ItemsSource = profiles;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(TitleInput.Text))
        {
            args.Cancel = true;
            TitleInput.Header = "Reward title (required)";
            return;
        }

        // Collect selected profile IDs
        var selectedIds = new List<Guid>();
        for (var i = 0; i < _profiles.Count; i++)
        {
            var element = ProfileCheckList.TryGetElement(i);
            if (element is CheckBox cb && cb.IsChecked == true && cb.Tag is Guid id)
            {
                selectedIds.Add(id);
            }
        }

        if (selectedIds.Count == 0)
        {
            args.Cancel = true;
            return;
        }

        SelectedProfileIds = selectedIds;

        Result = new Reward
        {
            Id = Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            Title = TitleInput.Text.Trim(),
            Description = string.IsNullOrWhiteSpace(DescriptionInput.Text) ? null : DescriptionInput.Text.Trim(),
            Emoji = string.IsNullOrWhiteSpace(EmojiInput.Text) ? null : EmojiInput.Text.Trim(),
            StarCost = (int)StarCostBox.Value,
            RenewAfterRedeem = RenewToggle.IsOn,
        };
    }
}
