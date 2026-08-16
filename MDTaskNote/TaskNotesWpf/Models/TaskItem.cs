using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TaskNotesWpf.Models;

public class TaskItem : ObservableObject
{
    // Properties come from the first line: {Status}{Group}{Title}
    private string _group = string.Empty;
    public string Group
    {
        get => _group;
        set
        {
            if (SetProperty(ref _group, value))
                OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string[] StatusOptions { get; } = { "ToDo", "Doing", "Completed" };

    private string _status = "ToDo";
    public string Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(Opacity));
                OnPropertyChanged(nameof(DisplayTitle));
                if (!_loading) SaveToFile();
            }
        }
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
                OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string FilePath { get; init; }

    public bool IsCompleted => string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase);

    public string DisplayTitle => $"{(string.IsNullOrWhiteSpace(Group) ? "" : $"[{Group}] ")}{Title}";

    public double Opacity => IsCompleted ? 0.55 : 1.0;

    private bool _loading;

    public TaskItem(string filePath)
    {
        FilePath = filePath;
        LoadFromFile();
    }

    public void LoadFromFile()
    {
        if (!File.Exists(FilePath)) return;
        var firstLine = File.ReadLines(FilePath).FirstOrDefault() ?? string.Empty;
        // Expected format {Status}{Group}{Title}
        var parts = ParseFirstLine(firstLine);
        _loading = true;
        try
        {
            Status = parts.status;
            Group = parts.group;
            Title = parts.title;
        }
        finally
        {
            _loading = false;
        }
    }

    public void SaveToFile()
    {
        var firstLine = $"{{{Status}}}{{{Group}}}{{{Title}}}";
        var otherLines = File.Exists(FilePath) ? File.ReadAllLines(FilePath).Skip(1) : Array.Empty<string>();
        var allLines = new[] { firstLine }.Concat(otherLines);
        File.WriteAllLines(FilePath, allLines);
    }

    private static (string status, string group, string title) ParseFirstLine(string line)
    {
        // Simple parser assuming exactly three {...} sections.
        var parts = new string[3];
        int idx = 0;
        int start = 0;
        while (idx < 3 && start < line.Length)
        {
            int open = line.IndexOf('{', start);
            int close = line.IndexOf('}', open + 1);
            if (open == -1 || close == -1) break;
            parts[idx] = line.Substring(open + 1, close - open - 1);
            start = close + 1;
            idx++;
        }
        // Fill missing with empty strings.
        for (int i = idx; i < 3; i++) parts[i] = string.Empty;
        return (parts[0], parts[1], parts[2]);
    }
}
