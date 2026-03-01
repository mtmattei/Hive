using Hive.Core.Services;
using Hive.Data.LocalDb;
using Hive.Data.Repositories;
using Hive.Services;
using Hive.ViewModels;
using Hive.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace Hive;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window? MainWindow { get; private set; }

    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Ensure database is created and seeded
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HiveDbContext>();
            db.Database.EnsureCreated();
            SeedData.SeedAsync(db).GetAwaiter().GetResult();
        }

        // Initialize theme
        var themeService = Services.GetRequiredService<ThemeService>();
        themeService.Initialize();

        _window = new Window();
        MainWindow = _window;

#if DEBUG
        _window.Title = "Hive \u2014 Family Calendar";
#endif

        _window.Content = new MainShell();

        // Apply saved theme
        if (_window.Content is FrameworkElement root)
            root.RequestedTheme = themeService.CurrentTheme;

        _window.Activate();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
#if DEBUG
            builder.AddDebug();
#endif
        });

        // Database
        services.AddDbContext<HiveDbContext>(options =>
            options.UseSqlite("Data Source=hive.db"));

        // Services
        services.AddTransient<IProfileService, ProfileService>();
        services.AddTransient<ICalendarService, CalendarService>();
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<IRewardService, RewardService>();
        services.AddTransient<IListService, ListService>();
        services.AddTransient<ISettingsService, SettingsService>();
        services.AddTransient<ISyncService, IcsCalendarSyncService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ThemeService>();
        services.AddTransient<IMealPlanService, MealPlanService>();
        services.AddTransient<ISharedAccessService, SharedAccessService>();
        services.AddTransient<IDeviceService, DeviceService>();
        services.AddTransient<ICountdownService, CountdownService>();
        services.AddTransient<IPhotoService, PhotoService>();
        services.AddTransient<IMagicImportService, MagicImportService>();
        services.AddSingleton<IWeatherService, WeatherService>();

        // ViewModels
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<TasksViewModel>();
        services.AddTransient<RewardsViewModel>();
        services.AddTransient<ListsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MealPlanViewModel>();
    }
}
