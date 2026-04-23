using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PlaqueData.Data;
using PlaqueData.Models;
using PropertyModel = PlaqueData.Models.Property;

namespace XpsCreator.ViewModels
{
    public partial class DetailEditorViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Contact? _currentContact;

        public ObservableCollection<Live> LiveRecords { get; } = new();
        public ObservableCollection<Dead> DeadRecords { get; } = new();
        public ObservableCollection<Ancestor> AncestorRecords { get; } = new();
        public ObservableCollection<PropertyModel> PropertyRecords { get; } = new();

        public DetailEditorViewModel(IDataService dataService)
        {
            _dataService = dataService;
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
