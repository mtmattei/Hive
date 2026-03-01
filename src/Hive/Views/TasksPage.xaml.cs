using Hive.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hive.Views;

public sealed partial class TasksPage : Page
{
    private TasksViewModel ViewModel { get; }

    public TasksPage()
    {
        ViewModel = App.Services.GetRequiredService<TasksViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        UpdateUI();
    }

    private void OnAddTask(object sender, RoutedEventArgs e)
    {
        // TODO: Show add task dialog
    }

    private void UpdateUI()
    {
        DateLabel.Text = ViewModel.SelectedDate.ToString("dddd, MMMM d, yyyy");
        ProfileColumnsRepeater.ItemsSource = ViewModel.Profiles;

        EmptyState.Visibility = ViewModel.State == ViewState.Empty
            ? Visibility.Visible : Visibility.Collapsed;
    }
}
