using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Models;
using Hive.Core.Services;

namespace Hive.ViewModels;

public partial class TasksViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly IProfileService _profileService;

    private readonly Guid _familyAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty] private ViewState _state = ViewState.Loading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<Profile> _profiles = [];
    [ObservableProperty] private ObservableCollection<TaskItem> _tasks = [];
    [ObservableProperty] private ObservableCollection<TaskCompletion> _completions = [];
    [ObservableProperty] private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private bool _showCelebration;

    public TasksViewModel(ITaskService taskService, IProfileService profileService)
    {
        _taskService = taskService;
        _profileService = profileService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;

            var profiles = await _profileService.GetProfilesAsync(_familyAccountId);
            Profiles = new ObservableCollection<Profile>(profiles);

            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task LoadTasksAsync()
    {
        try
        {
            var tasks = await _taskService.GetTasksAsync(_familyAccountId);
            Tasks = new ObservableCollection<TaskItem>(tasks);

            var completions = await _taskService.GetCompletionsAsync(_familyAccountId, SelectedDate);
            Completions = new ObservableCollection<TaskCompletion>(completions);

            State = Tasks.Count > 0 ? ViewState.Loaded : ViewState.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task ToggleTaskCompletionAsync(TaskCompletionRequest request)
    {
        var isCompleted = Completions.Any(c =>
            c.TaskItemId == request.TaskId && c.ProfileId == request.ProfileId);

        if (isCompleted)
        {
            await _taskService.UncompleteTaskAsync(request.TaskId, request.ProfileId, SelectedDate);
        }
        else
        {
            await _taskService.CompleteTaskAsync(
                request.TaskId, request.ProfileId, SelectedDate, request.TimeOfDay);

            if (await _taskService.AreAllTasksCompleteAsync(
                _familyAccountId, request.ProfileId, SelectedDate))
            {
                ShowCelebration = true;
                _ = Task.Delay(2000).ContinueWith(_ => ShowCelebration = false);
            }
        }

        await LoadTasksAsync();
    }

    public IEnumerable<TaskItem> GetTasksForProfile(Guid profileId) =>
        Tasks.Where(t => t.Assignments.Any(a => a.ProfileId == profileId));

    public IEnumerable<TaskItem> GetRoutinesForProfile(Guid profileId, RoutineTimeOfDay timeOfDay) =>
        GetTasksForProfile(profileId)
            .Where(t => t.Type == TaskItemType.Routine && t.RoutineTimeOfDay == timeOfDay);

    public IEnumerable<TaskItem> GetChoresForProfile(Guid profileId) =>
        GetTasksForProfile(profileId).Where(t => t.Type == TaskItemType.Chore);

    public bool IsTaskCompleted(Guid taskId, Guid profileId) =>
        Completions.Any(c => c.TaskItemId == taskId && c.ProfileId == profileId);

    public async Task CreateTaskAsync(TaskItem task, IReadOnlyList<Guid> profileIds)
    {
        task.FamilyAccountId = _familyAccountId;
        task.Assignments = profileIds.Select(pid => new TaskAssignment
        {
            Id = Guid.NewGuid(),
            ProfileId = pid,
        }).ToList();

        await _taskService.CreateTaskAsync(task);
        await LoadTasksAsync();
    }

    public async Task UpdateTaskAsync(TaskItem task, IReadOnlyList<Guid> profileIds)
    {
        task.FamilyAccountId = _familyAccountId;
        task.Assignments = profileIds.Select(pid => new TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            ProfileId = pid,
        }).ToList();

        await _taskService.UpdateTaskAsync(task);
        await LoadTasksAsync();
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(Guid taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        await LoadTasksAsync();
    }
}

public record TaskCompletionRequest(Guid TaskId, Guid ProfileId, RoutineTimeOfDay? TimeOfDay = null);
