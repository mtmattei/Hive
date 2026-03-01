using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Controls;

public sealed partial class ProfileChip : UserControl
{
    public event EventHandler<Profile>? ProfileToggled;

    public ProfileChip()
    {
        InitializeComponent();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is Profile profile)
        {
            ProfileToggled?.Invoke(this, profile);
        }
    }
}
