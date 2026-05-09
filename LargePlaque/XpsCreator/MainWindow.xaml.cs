using System.Windows;
using System.Windows.Controls;
using XpsCreator.Localization;

namespace XpsCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = SettingsManager.LoadSettings();
            if (!string.IsNullOrEmpty(settings.LastDatabasePath) && System.IO.File.Exists(settings.LastDatabasePath))
            {
                DatabasePathTextBox.Text = settings.LastDatabasePath;
                if (DataEntryView.DataContext is XpsCreator.ViewModels.DataEntryViewModel vm)
                {
                    _ = vm.ChangeDatabaseAsync(settings.LastDatabasePath);
                }
            }
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string lang)
            {
                LanguageManager.SetLanguage(lang);
            }
        }

        private async void SelectDatabase_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SQLite Database (*.db;*.sqlite;*.db3)|*.db;*.sqlite;*.db3|All files (*.*)|*.*",
                Title = "Select SQLite Database"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                DatabasePathTextBox.Text = openFileDialog.FileName;
                if (DataEntryView.DataContext is XpsCreator.ViewModels.DataEntryViewModel vm)
                {
                    await vm.ChangeDatabaseAsync(openFileDialog.FileName);
                    
                    var settings = SettingsManager.LoadSettings();
                    settings.LastDatabasePath = openFileDialog.FileName;
                    SettingsManager.SaveSettings(settings);
                }
            }
        }
    }
}