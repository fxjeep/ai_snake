using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskNotesWpf.Models; // for TaskItem
using TaskNotesWpf.Views; // for EditorWindow

namespace TaskNotesWpf.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // Expose AddTaskCommand for UI binding
        public ICommand AddTaskCommand { get; }

        private const string TasksFolderName = "mdfiles";
        private readonly string _tasksFolderPath;

        public ObservableCollection<TaskItem> AllTasks { get; } = new ObservableCollection<TaskItem>();

        private ICollectionView _visibleTasks;
        public ICollectionView VisibleTasks => _visibleTasks;

        private bool _showCompleted = true;
        public bool ShowCompleted
        {
            get => _showCompleted;
            set
            {
                if (SetProperty(ref _showCompleted, value))
                {
                    _visibleTasks.Refresh();
                }
            }
        }

        public ICommand OpenTaskCommand { get; }

        public MainViewModel()
        {
            // Determine tasks folder (next to executable)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _tasksFolderPath = Path.Combine(baseDir, TasksFolderName);
            if (!Directory.Exists(_tasksFolderPath))
                Directory.CreateDirectory(_tasksFolderPath);

            _visibleTasks = CollectionViewSource.GetDefaultView(AllTasks);
            _visibleTasks.Filter = FilterTasks;

            LoadAllTasks();

            OpenTaskCommand = new RelayCommand<TaskItem?>(OpenTask);
            AddTaskCommand = new RelayCommand(AddTask);
        }

        // Command to add a new task file
        private void AddTask()
        {
            // Ensure mdfiles folder exists (create if missing)
            var mdFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mdfiles");
            if (!Directory.Exists(mdFolder))
                Directory.CreateDirectory(mdFolder);

            // Generate unique file name
            var fileName = Guid.NewGuid().ToString("N") + ".md";
            var fullPath = Path.Combine(mdFolder, fileName);

            // Write initial line
            File.WriteAllText(fullPath, "{ToDo}{}{New Task}\n");

            // Create and add TaskItem to collection
            var newTask = new TaskItem(fullPath);
            SubscribeToTask(newTask);
            AllTasks.Add(newTask);
            _visibleTasks.Refresh();
        }

        private void LoadAllTasks()
        {
            var mdFiles = Directory.GetFiles(_tasksFolderPath, "*.md", SearchOption.TopDirectoryOnly);
            foreach (var file in mdFiles)
            {
                var task = new TaskItem(file);
                SubscribeToTask(task);
                AllTasks.Add(task);
            }
        }

        private void SubscribeToTask(TaskItem task)
        {
            task.PropertyChanged += (s, e) => _visibleTasks.Refresh();
        }

        private bool FilterTasks(object obj)
        {
            if (obj is not TaskItem task) return false;
            if (ShowCompleted) return true;
            return !task.IsCompleted;
        }

        private void OpenTask(TaskItem? task)
        {
            if (task == null) return;
            var editor = new EditorWindow(task);
            editor.Show();
        }

        public void DeleteTask(TaskItem task)
        {
            AllTasks.Remove(task);
            _visibleTasks.Refresh();
        }
    }
}
