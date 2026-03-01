using Hive.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hive.Controls;

public sealed partial class TaskItemControl : UserControl
{
    public static readonly DependencyProperty TaskProperty =
        DependencyProperty.Register(nameof(Task), typeof(TaskItem), typeof(TaskItemControl),
            new PropertyMetadata(null, OnTaskChanged));

    public static readonly DependencyProperty IsCompletedProperty =
        DependencyProperty.Register(nameof(IsCompleted), typeof(bool), typeof(TaskItemControl),
            new PropertyMetadata(false, OnCompletedChanged));

    public TaskItem? Task
    {
        get => (TaskItem?)GetValue(TaskProperty);
        set => SetValue(TaskProperty, value);
    }

    public bool IsCompleted
    {
        get => (bool)GetValue(IsCompletedProperty);
        set => SetValue(IsCompletedProperty, value);
    }

    public event EventHandler<TaskItem>? CompletionToggled;
    public event EventHandler<TaskItem>? TaskTapped;

    public TaskItemControl()
    {
        InitializeComponent();
    }

    private static void OnTaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskItemControl control) control.UpdateDisplay();
    }

    private static void OnCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskItemControl control)
            control.CompletionCheck.IsChecked = control.IsCompleted;
    }

    private void UpdateDisplay()
    {
        if (Task is null) return;

        EmojiText.Text = Task.Emoji ?? "📋";
        TitleText.Text = Task.Title;

        if (Task.StarValue > 0)
        {
            StarPanel.Visibility = Visibility.Visible;
            StarText.Text = Task.StarValue.ToString();
        }
        else
        {
            StarPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OnCheckboxClick(object sender, RoutedEventArgs e)
    {
        if (Task is not null)
            CompletionToggled?.Invoke(this, Task);
    }

    private void OnTitleTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (Task is not null)
            TaskTapped?.Invoke(this, Task);
    }
}
