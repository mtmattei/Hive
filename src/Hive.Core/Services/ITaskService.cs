using Hive.Core.Models;

namespace Hive.Core.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(
        Guid familyAccountId,
        Guid? profileId = null,
        CancellationToken ct = default);

    Task<TaskItem?> GetTaskByIdAsync(Guid taskId, CancellationToken ct = default);

    Task<TaskItem> CreateTaskAsync(TaskItem task, CancellationToken ct = default);

    Task<TaskItem> UpdateTaskAsync(TaskItem task, CancellationToken ct = default);

    Task DeleteTaskAsync(Guid taskId, CancellationToken ct = default);

    Task<TaskCompletion> CompleteTaskAsync(
        Guid taskId,
        Guid profileId,
        DateOnly date,
        RoutineTimeOfDay? timeOfDay = null,
        CancellationToken ct = default);

    Task UncompleteTaskAsync(
        Guid taskId,
        Guid profileId,
        DateOnly date,
        CancellationToken ct = default);

    Task<IReadOnlyList<TaskCompletion>> GetCompletionsAsync(
        Guid familyAccountId,
        DateOnly date,
        Guid? profileId = null,
        CancellationToken ct = default);

    Task<bool> AreAllTasksCompleteAsync(
        Guid familyAccountId,
        Guid profileId,
        DateOnly date,
        CancellationToken ct = default);
}
