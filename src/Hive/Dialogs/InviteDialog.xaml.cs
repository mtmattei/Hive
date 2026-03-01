using Hive.Core.Models;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Dialogs;

public sealed partial class InviteDialog : ContentDialog
{
    private readonly Guid _familyAccountId;

    public SharedAccess? Result { get; private set; }

    public InviteDialog(Guid familyAccountId)
    {
        InitializeComponent();
        _familyAccountId = familyAccountId;
    }

    private void OnSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var email = EmailInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            args.Cancel = true;
            EmailInput.Header = "Email address (required)";
            return;
        }

        Result = new SharedAccess
        {
            Id = Guid.NewGuid(),
            FamilyAccountId = _familyAccountId,
            InviteeEmail = email,
            Role = (AccessRole)RoleCombo.SelectedIndex,
            InvitedAt = DateTimeOffset.UtcNow,
        };
    }
}
