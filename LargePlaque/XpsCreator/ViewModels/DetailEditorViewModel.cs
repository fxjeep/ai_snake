using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PlaqueData.Data;
using PlaqueData.Models;
using PropertyModel = PlaqueData.Models.Property;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XpsCreator.ViewModels
{
    public partial class DetailEditorViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Contact? _currentContact;

        [ObservableProperty]
        private string _filterText = string.Empty;

        public ICollectionView LiveView { get; }
        public ICollectionView DeadView { get; }
        public ICollectionView AncestorView { get; }
        public ICollectionView PropertyView { get; }

        public ObservableCollection<Live> LiveRecords { get; } = new();
        public ObservableCollection<Dead> DeadRecords { get; } = new();
        public ObservableCollection<Ancestor> AncestorRecords { get; } = new();
        public ObservableCollection<PropertyModel> PropertyRecords { get; } = new();

        public DetailEditorViewModel(IDataService dataService)
        {
            _dataService = dataService;

            LiveView = CollectionViewSource.GetDefaultView(LiveRecords);
            DeadView = CollectionViewSource.GetDefaultView(DeadRecords);
            AncestorView = CollectionViewSource.GetDefaultView(AncestorRecords);
            PropertyView = CollectionViewSource.GetDefaultView(PropertyRecords);

            LiveView.Filter = item => FilterItem(item);
            DeadView.Filter = item => FilterItem(item);
            AncestorView.Filter = item => FilterItem(item);
            PropertyView.Filter = item => FilterItem(item);
        }

        partial void OnFilterTextChanged(string value)
        {
            LiveView.Refresh();
            DeadView.Refresh();
            AncestorView.Refresh();
            PropertyView.Refresh();
        }

        private bool FilterItem(object item)
        {
            if (string.IsNullOrWhiteSpace(FilterText)) return true;

            if (item is Live live)
                return live.Name.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase);
            if (item is Dead dead)
                return dead.DeadName.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase) ||
                       dead.LiveName.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase);
            if (item is Ancestor ancestor)
                return ancestor.Name.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase) ||
                       ancestor.Surname.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase);
            if (item is PropertyModel property)
                return property.Address.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase) ||
                       property.LiveName.Contains(FilterText, System.StringComparison.OrdinalIgnoreCase);

            return true;
        }

        public async Task SetPrintStatusAsync(IList selectedItems, bool isPrint)
        {
            var items = selectedItems.Cast<object>().ToList();
            foreach (var item in items)
            {
                if (item is Live live) { live.IsPrint = isPrint; await _dataService.SaveLiveAsync(live); }
                else if (item is Dead dead) { dead.IsPrint = isPrint; await _dataService.SaveDeadAsync(dead); }
                else if (item is Ancestor ancestor) { ancestor.IsPrint = isPrint; await _dataService.SaveAncestorAsync(ancestor); }
                else if (item is PropertyModel property) { property.IsPrint = isPrint; await _dataService.SavePropertyAsync(property); }
            }
            // Refresh views to show updated status if needed, though IsPrint is likely bound
        }

        public async Task DeleteItemsAsync(IList selectedItems)
        {
            var items = selectedItems.Cast<object>().ToList();
            foreach (var item in items)
            {
                if (item is Live live) { LiveRecords.Remove(live); await _dataService.DeleteLiveAsync(live.Id); }
                else if (item is Dead dead) { DeadRecords.Remove(dead); await _dataService.DeleteDeadAsync(dead.Id); }
                else if (item is Ancestor ancestor) { AncestorRecords.Remove(ancestor); await _dataService.DeleteAncestorAsync(ancestor.Id); }
                else if (item is PropertyModel property) { PropertyRecords.Remove(property); await _dataService.DeletePropertyAsync(property.Id); }
            }
        }

        public async Task SaveItemAsync(object item)
        {
            if (item is Live live) await _dataService.SaveLiveAsync(live);
            else if (item is Dead dead) await _dataService.SaveDeadAsync(dead);
            else if (item is Ancestor ancestor) await _dataService.SaveAncestorAsync(ancestor);
            else if (item is PropertyModel property) await _dataService.SavePropertyAsync(property);
        }

        partial void OnCurrentContactChanged(Contact? value)
        {
            _ = LoadDetailsAsync();
        }

        private async Task LoadDetailsAsync()
        {
            LiveRecords.Clear();
            DeadRecords.Clear();
            AncestorRecords.Clear();
            PropertyRecords.Clear();

            if (CurrentContact == null) return;

            var live = await _dataService.GetLiveRecordsByContactIdAsync(CurrentContact.Id);
            foreach (var r in live) LiveRecords.Add(r);

            var dead = await _dataService.GetDeadRecordsByContactIdAsync(CurrentContact.Id);
            foreach (var r in dead) DeadRecords.Add(r);

            var ancestor = await _dataService.GetAncestorRecordsByContactIdAsync(CurrentContact.Id);
            foreach (var r in ancestor) AncestorRecords.Add(r);

            var property = await _dataService.GetPropertyRecordsByContactIdAsync(CurrentContact.Id);
            foreach (var r in property) PropertyRecords.Add(r);
        }
    }
}
