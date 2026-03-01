using Hive.Core.Services;
using Hive.Data.LocalDb;
using Hive.Data.Repositories;
using Hive.ViewModels;
using Hive.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Hive;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

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

        _window = new Window();

#if DEBUG
        _window.Title = "Hive — Family Calendar";
#endif

        _window.Content = new MainShell();
        _window.Activate();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
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

        // ViewModels
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<TasksViewModel>();
        services.AddTransient<RewardsViewModel>();
        services.AddTransient<ListsViewModel>();
        services.AddTransient<SettingsViewModel>();
    }
}
