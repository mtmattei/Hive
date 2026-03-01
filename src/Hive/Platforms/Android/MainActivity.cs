using Android.App;
using Android.Content.PM;

namespace Hive;

[Activity(
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    WindowSoftInputMode = Android.Views.SoftInput.AdjustNothing)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
}
