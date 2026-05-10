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
            // TODO: Implement XPS Generation logic
            await Task.CompletedTask;
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
