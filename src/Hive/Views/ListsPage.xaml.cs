using Hive.Core.Models;
using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace Hive.Views;

public sealed partial class ListsPage : Page
{
    private ListsViewModel ViewModel { get; }

    public ListsPage()
    {
        ViewModel = App.Services.GetRequiredService<ListsViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        UpdateUI();
    }

    private void OnSelectList(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CustomList list)
        {
            ViewModel.SelectListCommand.Execute(list);
            UpdateUI();
        }
    }

    private async void OnToggleItem(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb && cb.Tag is Guid itemId)
        {
            await ViewModel.ToggleItemCommand.ExecuteAsync(itemId);
            UpdateUI();
        }
    }

    private async void OnDeleteItem(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid itemId)
        {
            await ViewModel.DeleteItemCommand.ExecuteAsync(itemId);
            UpdateUI();
        }
    }

    private async void OnAddItem(object sender, RoutedEventArgs e)
    {
        ViewModel.NewItemText = NewItemInput.Text;
        await ViewModel.AddItemCommand.ExecuteAsync(null);
        NewItemInput.Text = string.Empty;
        UpdateUI();
    }

    private async void OnNewItemKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ViewModel.NewItemText = NewItemInput.Text;
            await ViewModel.AddItemCommand.ExecuteAsync(null);
            NewItemInput.Text = string.Empty;
            UpdateUI();
        }
    }

    private void OnToggleCompleted(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleShowCompletedCommand.Execute(null);
        ToggleLabel.Text = ViewModel.ShowCompleted ? "Hide Completed" : "Show Completed";
        UpdateUI();
    }

    private void OnAddList(object sender, RoutedEventArgs e)
    {
        // TODO: Show add list dialog
    }

    private void UpdateUI()
    {
        ListTabsRepeater.ItemsSource = ViewModel.Lists;
        ItemsListView.ItemsSource = ViewModel.VisibleItems;
        EmptyState.Visibility = ViewModel.State == ViewState.Empty
            ? Visibility.Visible : Visibility.Collapsed;
    }
}
