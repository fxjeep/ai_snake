using CommunityToolkit.Mvvm.ComponentModel;
using PlaqueData.Data;

namespace XpsCreator.ViewModels
{
    public partial class DataEntryViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private ContactSearchViewModel _contactSearch;

        [ObservableProperty]
        private DetailEditorViewModel _detailEditor;

        public DataEntryViewModel()
        {
            // Initialize the data service (optionally passing a path if needed)
            _dataService = new SqliteDataService();

            _contactSearch = new ContactSearchViewModel(_dataService);
            _detailEditor = new DetailEditorViewModel(_dataService);

            // Sync selection from search to editor
            _contactSearch.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ContactSearchViewModel.SelectedContact))
                {
                    _detailEditor.CurrentContact = _contactSearch.SelectedContact;
                }
            };
        }

        public async System.Threading.Tasks.Task ChangeDatabaseAsync(string path)
        {
            await _dataService.ChangeDatabaseAsync(path);
            await _contactSearch.LoadContactsAsync();
            _detailEditor.CurrentContact = null;
        }
    }
}
