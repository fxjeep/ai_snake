using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using MdXaml;
using TaskNotesWpf.Models;
using FluentWindow = Wpf.Ui.Controls.FluentWindow;

namespace TaskNotesWpf.Views;

public partial class EditorWindow : FluentWindow
{
    private readonly TaskItem _task;
    private readonly System.Timers.Timer _autoSaveTimer;
    private readonly string _filePath;

    public EditorWindow(TaskItem task)
    {
        _task = task;
        _filePath = task.FilePath;
        InitializeComponent();
        // Load full content of markdown file
        EditorBox.Text = File.Exists(_filePath) ? File.ReadAllText(_filePath) : string.Empty;
        // Set window title to task title
        this.Title = _task.Title;
        // Update preview initially
        UpdatePreview();
        // Hook text changed for live preview
        EditorBox.TextChanged += (s, e) => UpdatePreview();
        // Auto‑save every minute
        _autoSaveTimer = new System.Timers.Timer(60_000) { AutoReset = true };
        _autoSaveTimer.Elapsed += (s, e) => Dispatcher.Invoke(SaveFile);
        _autoSaveTimer.Start();
        // Save when window is closing
        this.Closing += (s, e) => SaveFile();
    }

    private void UpdatePreview()
    {
        if (PreviewViewer is MarkdownScrollViewer viewer)
        {
            viewer.Markdown = EditorBox.Text;
        }
    }

    private void SaveFile()
    {
        try
        {
            File.WriteAllText(_filePath, EditorBox.Text);
            // Update task model (first line parsing) after save
            _task.LoadFromFile();
            // Refresh main window title if it changed
            this.Title = _task.Title;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save task file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
