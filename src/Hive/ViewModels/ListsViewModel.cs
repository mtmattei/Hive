using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hive.Core.Models;
using Hive.Core.Services;

namespace Hive.ViewModels;

public partial class ListsViewModel : ObservableObject
{
    private readonly IListService _listService;

    private readonly Guid _familyAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty] private ViewState _state = ViewState.Loading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<CustomList> _lists = [];
    [ObservableProperty] private CustomList? _selectedList;
    [ObservableProperty] private bool _showCompleted;
    [ObservableProperty] private string _newItemText = string.Empty;

    public ListsViewModel(IListService listService)
    {
        _listService = listService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            State = ViewState.Loading;
            await LoadListsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private async Task LoadListsAsync()
    {
        try
        {
            var lists = await _listService.GetListsAsync(_familyAccountId);
            Lists = new ObservableCollection<CustomList>(lists);

            if (SelectedList is null && Lists.Count > 0)
                SelectedList = Lists[0];

            State = Lists.Count > 0 ? ViewState.Loaded : ViewState.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private void SelectList(CustomList list)
    {
        SelectedList = list;
    }

    [RelayCommand]
    private async Task ToggleItemAsync(Guid itemId)
    {
        await _listService.ToggleItemCompletionAsync(itemId);
        await LoadListsAsync();
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (SelectedList is null || string.IsNullOrWhiteSpace(NewItemText))
            return;

        await _listService.AddItemAsync(SelectedList.Id, new ListItem { Text = NewItemText.Trim() });
        NewItemText = string.Empty;
        await LoadListsAsync();
    }

    [RelayCommand]
    private async Task DeleteItemAsync(Guid itemId)
    {
        await _listService.DeleteItemAsync(itemId);
        await LoadListsAsync();
    }

    [RelayCommand]
    private void ToggleShowCompleted()
    {
        ShowCompleted = !ShowCompleted;
    }

    [RelayCommand]
    private async Task DeleteListAsync(Guid listId)
    {
        await _listService.DeleteListAsync(listId);
        SelectedList = null;
        await LoadListsAsync();
    }

    public IEnumerable<ListItem> VisibleItems =>
        SelectedList?.Items
            .Where(i => ShowCompleted || !i.IsCompleted)
            .OrderBy(i => i.IsCompleted)
            .ThenBy(i => i.SortOrder) ?? Enumerable.Empty<ListItem>();

    public async Task CreateListAsync(CustomList list)
    {
        list.FamilyAccountId = _familyAccountId;
        await _listService.CreateListAsync(list);
        await LoadListsAsync();
        SelectedList = Lists.FirstOrDefault(l => l.Id == list.Id) ?? Lists.LastOrDefault();
    }
}
