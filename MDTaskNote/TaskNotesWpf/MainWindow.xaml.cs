using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TaskNotesWpf.Models;
using TaskNotesWpf.ViewModels;
using FluentWindow = Wpf.Ui.Controls.FluentWindow;

namespace TaskNotesWpf;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TaskActionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { ContextMenu: { } menu } button)
        {
            menu.DataContext = button.DataContext;
            menu.PlacementTarget = button;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
            e.Handled = true;
        }
    }

    private void TaskMenuView_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is TaskItem task)
            ((MainViewModel)DataContext).OpenTaskCommand.Execute(task);
    }

    private void TaskMenuDelete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not TaskItem task) return;
        var result = MessageBox.Show($"Delete \"{task.DisplayTitle}\"?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        File.Delete(task.FilePath);
        ((MainViewModel)DataContext).DeleteTask(task);
    }
}
