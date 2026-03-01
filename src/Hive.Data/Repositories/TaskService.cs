using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class TaskService : ITaskService
{
    private readonly HiveDbContext _db;
    private readonly IProfileService _profileService;

    public TaskService(HiveDbContext db, IProfileService profileService)
    {
        _db = db;
        _profileService = profileService;
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(
        Guid familyAccountId, Guid? profileId = null, CancellationToken ct = default)
    {
        var query = _db.Tasks
            .Include(t => t.Assignments).ThenInclude(a => a.Profile)
            .Include(t => t.Recurrence)
            .Where(t => t.FamilyAccountId == familyAccountId);

        if (profileId.HasValue)
        {
            query = query.Where(t => t.Assignments.Any(a => a.ProfileId == profileId.Value));
        }

        return await query.OrderBy(t => t.SortOrder).ToListAsync(ct);
    }

    public async Task<TaskItem?> GetTaskByIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _db.Tasks
            .Include(t => t.Assignments).ThenInclude(a => a.Profile)
            .Include(t => t.Recurrence)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task, CancellationToken ct = default)
    {
        task.Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id;
        task.CreatedAt = DateTimeOffset.UtcNow;

        var maxOrder = await _db.Tasks
            .Where(t => t.FamilyAccountId == task.FamilyAccountId)
            .MaxAsync(t => (int?)t.SortOrder, ct) ?? -1;
        task.SortOrder = maxOrder + 1;

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task<TaskItem> UpdateTaskAsync(TaskItem task, CancellationToken ct = default)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _db.Tasks.FindAsync([taskId], ct);
        if (task is not null)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<TaskCompletion> CompleteTaskAsync(
        Guid taskId, Guid profileId, DateOnly date,
        RoutineTimeOfDay? timeOfDay = null, CancellationToken ct = default)
    {
        var task = await _db.Tasks.FindAsync([taskId], ct)
            ?? throw new InvalidOperationException($"Task {taskId} not found");

        var existing = await _db.TaskCompletions
            .FirstOrDefaultAsync(c =>
                c.TaskItemId == taskId &&
                c.ProfileId == profileId &&
                c.CompletedDate == date, ct);

        if (existing is not null)
            return existing;

        var completion = new TaskCompletion
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            ProfileId = profileId,
            CompletedDate = date,
            CompletedTimeOfDay = timeOfDay,
            StarsEarned = task.StarValue,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        _db.TaskCompletions.Add(completion);

        if (task.StarValue > 0)
        {
            await _profileService.AdjustStarsAsync(profileId, task.StarValue, ct);
        }

        await _db.SaveChangesAsync(ct);
        return completion;
    }

    public async Task UncompleteTaskAsync(
        Guid taskId, Guid profileId, DateOnly date, CancellationToken ct = default)
    {
        var completion = await _db.TaskCompletions
            .FirstOrDefaultAsync(c =>
                c.TaskItemId == taskId &&
                c.ProfileId == profileId &&
                c.CompletedDate == date, ct);

        if (completion is not null)
        {
            if (completion.StarsEarned > 0)
            {
                await _profileService.AdjustStarsAsync(profileId, -completion.StarsEarned, ct);
            }

            _db.TaskCompletions.Remove(completion);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<TaskCompletion>> GetCompletionsAsync(
        Guid familyAccountId, DateOnly date, Guid? profileId = null, CancellationToken ct = default)
    {
        var query = _db.TaskCompletions
            .Include(c => c.TaskItem)
            .Include(c => c.Profile)
            .Where(c => c.TaskItem!.FamilyAccountId == familyAccountId && c.CompletedDate == date);

        if (profileId.HasValue)
        {
            query = query.Where(c => c.ProfileId == profileId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<bool> AreAllTasksCompleteAsync(
        Guid familyAccountId, Guid profileId, DateOnly date, CancellationToken ct = default)
    {
        var tasks = await GetTasksAsync(familyAccountId, profileId, ct);
        if (tasks.Count == 0)
            return false;

        var completions = await GetCompletionsAsync(familyAccountId, date, profileId, ct);
        var completedTaskIds = completions.Select(c => c.TaskItemId).ToHashSet();

        return tasks.All(t => completedTaskIds.Contains(t.Id));
    }
}
