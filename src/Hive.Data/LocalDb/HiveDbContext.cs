using Hive.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.LocalDb;

public class HiveDbContext : DbContext
{
    public HiveDbContext(DbContextOptions<HiveDbContext> options) : base(options) { }

    public DbSet<FamilyAccount> FamilyAccounts => Set<FamilyAccount>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<RecurrenceRule> RecurrenceRules => Set<RecurrenceRule>();
    public DbSet<SyncedCalendar> SyncedCalendars => Set<SyncedCalendar>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<TaskCompletion> TaskCompletions => Set<TaskCompletion>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<RewardEligibility> RewardEligibilities => Set<RewardEligibility>();
    public DbSet<RewardRedemption> RewardRedemptions => Set<RewardRedemption>();
    public DbSet<CustomList> Lists => Set<CustomList>();
    public DbSet<ListItem> ListItems => Set<ListItem>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<CalendarSettings> CalendarSettings => Set<CalendarSettings>();
    public DbSet<SharedAccess> SharedAccess => Set<SharedAccess>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<EventCountdown> EventCountdowns => Set<EventCountdown>();
    public DbSet<PhotoAlbum> PhotoAlbums => Set<PhotoAlbum>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<MagicImport> MagicImports => Set<MagicImport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // FamilyAccount
        modelBuilder.Entity<FamilyAccount>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Profiles).WithOne(x => x.FamilyAccount).HasForeignKey(x => x.FamilyAccountId);
            e.HasMany(x => x.Devices).WithOne(x => x.FamilyAccount).HasForeignKey(x => x.FamilyAccountId);
            e.HasMany(x => x.SharedAccess).WithOne(x => x.FamilyAccount).HasForeignKey(x => x.FamilyAccountId);
            e.HasOne(x => x.Settings).WithOne(x => x.FamilyAccount).HasForeignKey<CalendarSettings>(x => x.FamilyAccountId);
        });

        // Profile
        modelBuilder.Entity<Profile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Events).WithOne(x => x.Profile).HasForeignKey(x => x.ProfileId);
            e.HasMany(x => x.TaskAssignments).WithOne(x => x.Profile).HasForeignKey(x => x.ProfileId);
            e.HasMany(x => x.EligibleRewards).WithOne(x => x.Profile).HasForeignKey(x => x.ProfileId);
            e.HasOne(x => x.LinkedCalendar).WithOne(x => x.LinkedProfile).HasForeignKey<Profile>(x => x.LinkedCalendarId);
        });

        // CalendarEvent
        modelBuilder.Entity<CalendarEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Recurrence).WithMany().HasForeignKey("RecurrenceRuleId").OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.FamilyAccountId, x.StartTime });
        });

        // RecurrenceRule
        modelBuilder.Entity<RecurrenceRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DaysOfWeek)
                .HasConversion(
                    v => v == null ? null : string.Join(",", v.Select(d => (int)d)),
                    v => v == null ? null : v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => (DayOfWeek)int.Parse(s)).ToArray());
        });

        // TaskItem
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Assignments).WithOne(x => x.TaskItem).HasForeignKey(x => x.TaskItemId);
            e.HasOne(x => x.Recurrence).WithMany().HasForeignKey("RecurrenceRuleId").OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.RoutineDays)
                .HasConversion(
                    v => v == null ? null : string.Join(",", v.Select(d => (int)d)),
                    v => v == null ? null : v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => (DayOfWeek)int.Parse(s)).ToArray());
        });

        // TaskAssignment
        modelBuilder.Entity<TaskAssignment>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // TaskCompletion
        modelBuilder.Entity<TaskCompletion>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.TaskItem).WithMany().HasForeignKey(x => x.TaskItemId);
            e.HasOne(x => x.Profile).WithMany().HasForeignKey(x => x.ProfileId);
            e.HasIndex(x => new { x.TaskItemId, x.ProfileId, x.CompletedDate }).IsUnique();
        });

        // Reward
        modelBuilder.Entity<Reward>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.EligibleProfiles).WithOne(x => x.Reward).HasForeignKey(x => x.RewardId);
        });

        // RewardEligibility
        modelBuilder.Entity<RewardEligibility>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // RewardRedemption
        modelBuilder.Entity<RewardRedemption>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Reward).WithMany().HasForeignKey(x => x.RewardId);
            e.HasOne(x => x.Profile).WithMany().HasForeignKey(x => x.ProfileId);
        });

        // CustomList
        modelBuilder.Entity<CustomList>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Items).WithOne(x => x.List).HasForeignKey(x => x.ListId).OnDelete(DeleteBehavior.Cascade);
        });

        // ListItem
        modelBuilder.Entity<ListItem>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // MealPlanEntry
        modelBuilder.Entity<MealPlanEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Recipe).WithMany().HasForeignKey(x => x.RecipeId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.FamilyAccountId, x.Date, x.Category });
        });

        // Recipe
        modelBuilder.Entity<Recipe>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // CalendarSettings
        modelBuilder.Entity<CalendarSettings>(e =>
        {
            e.HasKey(x => x.FamilyAccountId);
        });

        // SharedAccess
        modelBuilder.Entity<SharedAccess>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // Device
        modelBuilder.Entity<Device>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // SyncedCalendar
        modelBuilder.Entity<SyncedCalendar>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // EventCountdown
        modelBuilder.Entity<EventCountdown>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.FamilyAccountId, x.TargetDate });
            e.Ignore(x => x.DaysRemaining);
            e.Ignore(x => x.IsToday);
            e.Ignore(x => x.IsPast);
        });

        // PhotoAlbum
        modelBuilder.Entity<PhotoAlbum>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Photos).WithOne(x => x.Album).HasForeignKey(x => x.AlbumId).OnDelete(DeleteBehavior.Cascade);
        });

        // Photo
        modelBuilder.Entity<Photo>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // MagicImport
        modelBuilder.Entity<MagicImport>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}
