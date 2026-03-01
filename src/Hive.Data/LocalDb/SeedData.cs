using Hive.Core.Models;

namespace Hive.Data.LocalDb;

public static class SeedData
{
    public static readonly Guid FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid MomId = Guid.Parse("00000000-0000-0000-0001-000000000001");
    public static readonly Guid DadId = Guid.Parse("00000000-0000-0000-0001-000000000002");
    public static readonly Guid EmmaId = Guid.Parse("00000000-0000-0000-0001-000000000003");
    public static readonly Guid LucasId = Guid.Parse("00000000-0000-0000-0001-000000000004");
    public static readonly Guid FamilyProfileId = Guid.Parse("00000000-0000-0000-0001-000000000005");

    public static async Task SeedAsync(HiveDbContext db)
    {
        if (await db.FamilyAccounts.FindAsync(FamilyId) is not null)
            return; // Already seeded

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // ═══ Family Account ═══
        var family = new FamilyAccount
        {
            Id = FamilyId,
            Email = "demo@ourskylight.com",
            DisplayName = "Demo Family",
            TimeZone = "America/New_York",
            Address = "123 Main St, Anytown, USA",
            Subscription = SubscriptionTier.Plus,
            CreatedAt = now,
        };
        db.FamilyAccounts.Add(family);

        // ═══ Profiles ═══
        var profiles = new[]
        {
            new Profile { Id = MomId, FamilyAccountId = FamilyId, Name = "Mom", Color = "#C2553A", Emoji = "👩", SortOrder = 0, StarBalance = 0 },
            new Profile { Id = DadId, FamilyAccountId = FamilyId, Name = "Dad", Color = "#1B6B93", Emoji = "👨", SortOrder = 1, StarBalance = 0 },
            new Profile { Id = EmmaId, FamilyAccountId = FamilyId, Name = "Emma", Color = "#6A8E4E", Emoji = "👧", SortOrder = 2, StarBalance = 45 },
            new Profile { Id = LucasId, FamilyAccountId = FamilyId, Name = "Lucas", Color = "#D4923A", Emoji = "👦", SortOrder = 3, StarBalance = 32 },
            new Profile { Id = FamilyProfileId, FamilyAccountId = FamilyId, Name = "Family", Color = "#7B5EA7", Emoji = "👨‍👩‍👧‍👦", SortOrder = 4, StarBalance = 0 },
        };
        db.Profiles.AddRange(profiles);

        // ═══ Settings ═══
        db.CalendarSettings.Add(new CalendarSettings
        {
            FamilyAccountId = FamilyId,
            StartWeekOn = DayOfWeek.Sunday,
            ScheduleViewDays = 5,
            StartOnCurrentDay = true,
            DimPastEvents = true,
            ShadeWeekends = false,
            PreviewChoresInCalendar = true,
        });

        // ═══ Calendar Events ═══
        var baseDate = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.FromHours(-5));

        var events = new[]
        {
            // Today
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = MomId,
                Title = "Team standup", StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(9).AddMinutes(30),
                Notes = "Weekly sync with product team", CreatedAt = now, UpdatedAt = now,
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = EmmaId,
                Title = "Soccer practice", StartTime = baseDate.AddHours(16), EndTime = baseDate.AddHours(17).AddMinutes(30),
                Location = "Memorial Park Field 3", CreatedAt = now, UpdatedAt = now,
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = FamilyProfileId,
                Title = "Family dinner", StartTime = baseDate.AddHours(18).AddMinutes(30), EndTime = baseDate.AddHours(19).AddMinutes(30),
                CreatedAt = now, UpdatedAt = now,
            },
            // Tomorrow
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = DadId,
                Title = "Dentist appointment", StartTime = baseDate.AddDays(1).AddHours(10), EndTime = baseDate.AddDays(1).AddHours(11),
                Location = "Dr. Smith, 456 Oak Ave", CreatedAt = now, UpdatedAt = now,
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = LucasId,
                Title = "Piano lesson", StartTime = baseDate.AddDays(1).AddHours(15), EndTime = baseDate.AddDays(1).AddHours(16),
                CreatedAt = now, UpdatedAt = now,
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = MomId,
                Title = "Yoga class", StartTime = baseDate.AddDays(1).AddHours(7), EndTime = baseDate.AddDays(1).AddHours(8),
                CreatedAt = now, UpdatedAt = now,
            },
            // Day after tomorrow
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = EmmaId,
                Title = "Science fair prep", StartTime = baseDate.AddDays(2).AddHours(14), EndTime = baseDate.AddDays(2).AddHours(16),
                Notes = "Bring poster board and markers", CreatedAt = now, UpdatedAt = now,
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = DadId,
                Title = "Client meeting", StartTime = baseDate.AddDays(2).AddHours(11), EndTime = baseDate.AddDays(2).AddHours(12),
                Location = "Zoom", CreatedAt = now, UpdatedAt = now,
            },
            // All-day events
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = FamilyProfileId,
                Title = "Spring Break", StartTime = baseDate.AddDays(5), IsAllDay = true,
                CreatedAt = now, UpdatedAt = now,
            },
            // Recurring: weekly standup
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = MomId,
                Title = "Weekly planning", StartTime = baseDate.AddDays(- (int)baseDate.DayOfWeek + 1).AddHours(8),
                EndTime = baseDate.AddDays(- (int)baseDate.DayOfWeek + 1).AddHours(8).AddMinutes(45),
                Recurrence = new RecurrenceRule
                {
                    Id = Guid.NewGuid(), Frequency = RecurrenceFrequency.Weekly, Interval = 1,
                    DaysOfWeek = [DayOfWeek.Monday],
                },
                CreatedAt = now, UpdatedAt = now,
            },
            // Next 3-4 days
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = LucasId,
                Title = "Basketball game", StartTime = baseDate.AddDays(3).AddHours(17), EndTime = baseDate.AddDays(3).AddHours(18).AddMinutes(30),
                Location = "Community Center Gym", CreatedAt = now, UpdatedAt = now,
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(), FamilyAccountId = FamilyId, ProfileId = MomId,
                Title = "Book club", StartTime = baseDate.AddDays(4).AddHours(19), EndTime = baseDate.AddDays(4).AddHours(21),
                Notes = "Reading: 'The Midnight Library'", CreatedAt = now, UpdatedAt = now,
            },
        };
        db.CalendarEvents.AddRange(events);

        // ═══ Tasks — Chores ═══
        var choreIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var chores = new[]
        {
            new TaskItem
            {
                Id = choreIds[0], FamilyAccountId = FamilyId, Type = TaskItemType.Chore,
                Title = "Make bed", Emoji = "🛏️", StarValue = 2, SortOrder = 0, CreatedAt = now,
            },
            new TaskItem
            {
                Id = choreIds[1], FamilyAccountId = FamilyId, Type = TaskItemType.Chore,
                Title = "Feed the dog", Emoji = "🐕", StarValue = 3, SortOrder = 1, CreatedAt = now,
            },
            new TaskItem
            {
                Id = choreIds[2], FamilyAccountId = FamilyId, Type = TaskItemType.Chore,
                Title = "Empty dishwasher", Emoji = "🍽️", StarValue = 5, SortOrder = 2, CreatedAt = now,
            },
            new TaskItem
            {
                Id = choreIds[3], FamilyAccountId = FamilyId, Type = TaskItemType.Chore,
                Title = "Take out trash", Emoji = "🗑️", StarValue = 3, SortOrder = 3, CreatedAt = now,
            },
            new TaskItem
            {
                Id = choreIds[4], FamilyAccountId = FamilyId, Type = TaskItemType.Chore,
                Title = "Tidy room", Emoji = "🧹", StarValue = 5, SortOrder = 4, CreatedAt = now,
            },
            new TaskItem
            {
                Id = choreIds[5], FamilyAccountId = FamilyId, Type = TaskItemType.Chore,
                Title = "Set the table", Emoji = "🍴", StarValue = 2, SortOrder = 5, CreatedAt = now,
            },
        };
        db.Tasks.AddRange(chores);

        // Chore assignments
        db.TaskAssignments.AddRange(
        [
            // Emma's chores
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[0], ProfileId = EmmaId },
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[1], ProfileId = EmmaId },
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[4], ProfileId = EmmaId },
            // Lucas's chores
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[0], ProfileId = LucasId },
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[2], ProfileId = LucasId },
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[3], ProfileId = LucasId },
            new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = choreIds[5], ProfileId = LucasId },
        ]);

        // ═══ Tasks — Routines ═══
        var routineIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var routines = new[]
        {
            new TaskItem
            {
                Id = routineIds[0], FamilyAccountId = FamilyId, Type = TaskItemType.Routine,
                Title = "Brush teeth", Emoji = "🪥", StarValue = 1, SortOrder = 0,
                RoutineTimeOfDay = RoutineTimeOfDay.Morning, CreatedAt = now,
            },
            new TaskItem
            {
                Id = routineIds[1], FamilyAccountId = FamilyId, Type = TaskItemType.Routine,
                Title = "Get dressed", Emoji = "👕", StarValue = 1, SortOrder = 1,
                RoutineTimeOfDay = RoutineTimeOfDay.Morning, CreatedAt = now,
            },
            new TaskItem
            {
                Id = routineIds[2], FamilyAccountId = FamilyId, Type = TaskItemType.Routine,
                Title = "Pack backpack", Emoji = "🎒", StarValue = 1, SortOrder = 2,
                RoutineTimeOfDay = RoutineTimeOfDay.Morning, CreatedAt = now,
            },
            new TaskItem
            {
                Id = routineIds[3], FamilyAccountId = FamilyId, Type = TaskItemType.Routine,
                Title = "Homework", Emoji = "📚", StarValue = 3, SortOrder = 0,
                RoutineTimeOfDay = RoutineTimeOfDay.Afternoon, CreatedAt = now,
            },
            new TaskItem
            {
                Id = routineIds[4], FamilyAccountId = FamilyId, Type = TaskItemType.Routine,
                Title = "Shower", Emoji = "🚿", StarValue = 1, SortOrder = 0,
                RoutineTimeOfDay = RoutineTimeOfDay.Evening, CreatedAt = now,
            },
            new TaskItem
            {
                Id = routineIds[5], FamilyAccountId = FamilyId, Type = TaskItemType.Routine,
                Title = "Read before bed", Emoji = "📖", StarValue = 2, SortOrder = 1,
                RoutineTimeOfDay = RoutineTimeOfDay.Evening, CreatedAt = now,
            },
        };
        db.Tasks.AddRange(routines);

        // Routine assignments (both kids)
        foreach (var routineId in routineIds)
        {
            db.TaskAssignments.AddRange(
            [
                new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = routineId, ProfileId = EmmaId },
                new TaskAssignment { Id = Guid.NewGuid(), TaskItemId = routineId, ProfileId = LucasId },
            ]);
        }

        // ═══ Rewards ═══
        var rewardIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var rewards = new[]
        {
            new Reward
            {
                Id = rewardIds[0], FamilyAccountId = FamilyId,
                Title = "Ice cream trip", Emoji = "🍦", StarCost = 50, RenewAfterRedeem = true,
                Description = "Pick any flavor!",
            },
            new Reward
            {
                Id = rewardIds[1], FamilyAccountId = FamilyId,
                Title = "Extra screen time", Emoji = "📱", StarCost = 30, RenewAfterRedeem = true,
                Description = "30 minutes of extra screen time",
            },
            new Reward
            {
                Id = rewardIds[2], FamilyAccountId = FamilyId,
                Title = "Choose dinner", Emoji = "🍕", StarCost = 40, RenewAfterRedeem = true,
                Description = "You pick what's for dinner!",
            },
            new Reward
            {
                Id = rewardIds[3], FamilyAccountId = FamilyId,
                Title = "Movie night pick", Emoji = "🎬", StarCost = 60, RenewAfterRedeem = true,
                Description = "Choose the family movie",
            },
        };
        db.Rewards.AddRange(rewards);

        // Reward eligibility (both kids)
        foreach (var rewardId in rewardIds)
        {
            db.RewardEligibilities.AddRange(
            [
                new RewardEligibility { Id = Guid.NewGuid(), RewardId = rewardId, ProfileId = EmmaId },
                new RewardEligibility { Id = Guid.NewGuid(), RewardId = rewardId, ProfileId = LucasId },
            ]);
        }

        // ═══ Lists ═══
        var shoppingListId = Guid.NewGuid();
        var todoListId = Guid.NewGuid();

        db.Lists.AddRange(
        [
            new CustomList
            {
                Id = shoppingListId, FamilyAccountId = FamilyId,
                Title = "Shopping", Type = ListType.Grocery, Color = "#6A8E4E", SortOrder = 0,
                Items =
                [
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Milk", SortOrder = 0 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Bread", SortOrder = 1 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Eggs", SortOrder = 2 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Chicken breast", SortOrder = 3 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Bananas", SortOrder = 4 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Pasta", SortOrder = 5 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Tomato sauce", IsCompleted = true, SortOrder = 6 },
                    new ListItem { Id = Guid.NewGuid(), ListId = shoppingListId, Text = "Apples", IsCompleted = true, SortOrder = 7 },
                ],
            },
            new CustomList
            {
                Id = todoListId, FamilyAccountId = FamilyId,
                Title = "To-Dos", Type = ListType.ToDo, Color = "#1B6B93", SortOrder = 1,
                Items =
                [
                    new ListItem { Id = Guid.NewGuid(), ListId = todoListId, Text = "Schedule vet appointment", SortOrder = 0 },
                    new ListItem { Id = Guid.NewGuid(), ListId = todoListId, Text = "Fix leaky faucet", SortOrder = 1 },
                    new ListItem { Id = Guid.NewGuid(), ListId = todoListId, Text = "Order birthday gift", SortOrder = 2 },
                    new ListItem { Id = Guid.NewGuid(), ListId = todoListId, Text = "Return library books", IsCompleted = true, SortOrder = 3 },
                    new ListItem { Id = Guid.NewGuid(), ListId = todoListId, Text = "Sign permission slip", IsCompleted = true, SortOrder = 4 },
                ],
            },
        ]);

        await db.SaveChangesAsync();
    }
}
