using Microsoft.UI.Xaml;

namespace Hive.Services;

public class ThemeService
{
    private const string ThemeSettingKey = "AppTheme";
    private ElementTheme _currentTheme = ElementTheme.Default;

    public ElementTheme CurrentTheme => _currentTheme;

    public void Initialize()
    {
        var saved = Windows.Storage.ApplicationData.Current?.LocalSettings?.Values[ThemeSettingKey];
        if (saved is int themeInt && Enum.IsDefined(typeof(ElementTheme), themeInt))
            _currentTheme = (ElementTheme)themeInt;
    }

    public void SetTheme(ElementTheme theme)
    {
        _currentTheme = theme;

        if (App.MainWindow?.Content is FrameworkElement root)
            root.RequestedTheme = theme;

        try
        {
            if (Windows.Storage.ApplicationData.Current?.LocalSettings is { } settings)
                settings.Values[ThemeSettingKey] = (int)theme;
        }
        catch
        {
            // LocalSettings may not be available on all platforms
        }
    }

    public void ToggleDarkMode()
    {
        var next = _currentTheme switch
        {
            ElementTheme.Dark => ElementTheme.Light,
            ElementTheme.Light => ElementTheme.Dark,
            _ => ElementTheme.Dark,
        };
        SetTheme(next);
    }

    public bool IsDarkMode => _currentTheme == ElementTheme.Dark;
}
