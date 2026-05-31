using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PlaqueData.Data;
using PlaqueData.Models;

namespace XpsCreator.ViewModels
{
    public partial class WeeklyPrintViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        public ObservableCollection<WeeklyPrintItem> PrintItems { get; } = new();

        public WeeklyPrintViewModel()
        {
            _dataService = new SqliteDataService();
        }

        public async Task ChangeDatabaseAsync(string path)
        {
            await _dataService.ChangeDatabaseAsync(path);
            await LoadItemsAsync();
        }

        [RelayCommand]
        public async Task LoadItemsAsync()
        {
            PrintItems.Clear();
            var summaries = await _dataService.GetPrintSummariesAsync();

            var liveLabel = Application.Current.TryFindResource("TabLive") as string ?? "Live";
            var deadLabel = Application.Current.TryFindResource("TabDead") as string ?? "Dead";
            var ancestorLabel = Application.Current.TryFindResource("TabAncestor") as string ?? "Ancestor";
            var propertyLabel = Application.Current.TryFindResource("TabProperty") as string ?? "Property";

            foreach (var summary in summaries)
            {
                int livePrint = summary.IsPrint ? summary.LiveTotal : summary.LivePrint;
                int deadPrint = summary.IsPrint ? summary.DeadTotal : summary.DeadPrint;
                int ancestorPrint = summary.IsPrint ? summary.AncestorTotal : summary.AncestorPrint;
                int propertyPrint = summary.IsPrint ? summary.PropertyTotal : summary.PropertyPrint;

                string summaryText = $"{liveLabel}: {summary.LiveTotal}/{livePrint}, " +
                                   $"{deadLabel}: {summary.DeadTotal}/{deadPrint}, " +
                                   $"{ancestorLabel}: {summary.AncestorTotal}/{ancestorPrint}, " +
                                   $"{propertyLabel}: {summary.PropertyTotal}/{propertyPrint}";

                PrintItems.Add(new WeeklyPrintItem
                {
                    ContactId = summary.Id,
                    ContactName = summary.Name,
                    ContactCode = summary.Code,
                    Summary = summaryText
                });
            }
        }

        [RelayCommand]
        private async Task GenerateXpsAsync()
        {
            try
            {
                var liveRecords = await _dataService.GetWeeklyLivePrintRecordsAsync();
                if (liveRecords == null || liveRecords.Count == 0)
                {
                    var noRecordsMsg = Application.Current.TryFindResource("NoRecordsToPrint") as string ?? "No records found to print.";
                    MessageBox.Show(noRecordsMsg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string settingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", PrintLayoutSettings.DefaultConfigPath);
                if (!System.IO.File.Exists(settingsPath))
                {
                    MessageBox.Show($"Configuration file not found at: {settingsPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var settings = PrintLayoutSettings.Load(settingsPath);
                var config = settings.GetWeeklyConfig("长生");
                if (config == null)
                {
                    MessageBox.Show("Weekly print configuration for '长生' not found in configuration file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                string targetDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WeeklyPrint", dateStr);
                string outputXpsPath = System.IO.Path.Combine(targetDir, $"{dateStr}_changsheng.xps");

                string generatedPath = WeeklyXpsPrinter.GenerateWeeklyLiveXps(outputXpsPath, liveRecords, config);

                if (!string.IsNullOrEmpty(generatedPath))
                {
                    var successMsg = Application.Current.TryFindResource("WeeklyXpsSuccess") as string ?? "XPS file generated successfully:\n{0}";
                    // Clean up formatting of newline character in resource string if it was loaded literal
                    successMsg = successMsg.Replace("\\n", Environment.NewLine);
                    MessageBox.Show(string.Format(successMsg, generatedPath), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = Application.Current.TryFindResource("WeeklyXpsError") as string ?? "Error generating XPS: {0}";
                errorMsg = errorMsg.Replace("\\n", Environment.NewLine);
                MessageBox.Show(string.Format(errorMsg, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ClearPrintAsync()
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all print markers and set the last print date to today?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _dataService.ClearAllPrintAsync();
                await LoadItemsAsync();
            }
        }

        [RelayCommand]
        private async Task Action(object? item)
        {
            if (item is WeeklyPrintItem weeklyItem)
            {
                await _dataService.UnprintContactAsync(weeklyItem.ContactId);
                PrintItems.Remove(weeklyItem);
            }
        }
    }
}
