using System;
using System.IO;
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
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", PrintLayoutSettings.DefaultConfigPath);
            var settings = PrintLayoutSettings.Load(settingsPath);
            if (!string.IsNullOrEmpty(settings.WeeklyDataFile) && File.Exists(settings.WeeklyDataFile))
            {
                DatabasePathTextBox.Text = settings.WeeklyDataFile;
                if (DataEntryView.DataContext is XpsCreator.ViewModels.DataEntryViewModel vm)
                {
                    _ = vm.ChangeDatabaseAsync(settings.WeeklyDataFile);
                }
                if (WeeklyPrintView.DataContext is XpsCreator.ViewModels.WeeklyPrintViewModel pvm)
                {
                    _ = pvm.ChangeDatabaseAsync(settings.WeeklyDataFile);
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

                    string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", PrintLayoutSettings.DefaultConfigPath);
                    var settings = PrintLayoutSettings.Load(settingsPath);
                    settings.WeeklyDataFile = openFileDialog.FileName;
                    PrintLayoutSettings.Save(settingsPath, settings);
                }
                if (WeeklyPrintView.DataContext is XpsCreator.ViewModels.WeeklyPrintViewModel pvm)
                {
                    await pvm.ChangeDatabaseAsync(openFileDialog.FileName);
                }
            }
        }

        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tc && tc.SelectedItem is TabItem ti && ti.Header?.ToString() == Application.Current.TryFindResource("TabWeeklyPrint")?.ToString())
            {
                if (WeeklyPrintView.DataContext is XpsCreator.ViewModels.WeeklyPrintViewModel pvm)
                {
                    await pvm.LoadItemsAsync();
                }
            }
        }
    }
}