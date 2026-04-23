using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PlaqueData.Data;
using PlaqueData.Models;

namespace XpsCreator.ViewModels
{
    public partial class ContactSearchViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private List<Contact> _allContacts = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private Contact? _selectedContact;

        public ObservableCollection<Contact> FilteredContacts { get; } = new();

        public ContactSearchViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _ = LoadContactsAsync();
        }

        [RelayCommand]
        public async Task LoadContactsAsync()
        {
            _allContacts = await _dataService.GetAllContactsAsync();
            ApplyFilter();
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilter();
        }

        [RelayCommand]
        private async Task AddContactAsync()
        {
            var newContact = new Contact
            {
                Name = "Test",
                Code = "T0001",
                IsPrint = 1,
                LastPrint = ""
            };
            await _dataService.SaveContactAsync(newContact);
            await LoadContactsAsync();
        }

        [RelayCommand]
        private async Task DeleteContactAsync(Contact contact)
        {
            if (contact != null)
            {
                await _dataService.DeleteContactAsync(contact.Id);
                await LoadContactsAsync();
            }
        }

        [RelayCommand]
        private async Task TogglePrintAsync(Contact contact)
        {
            if (contact != null)
            {
                contact.IsPrint = contact.IsPrint == 1 ? 0 : 1;
                await _dataService.SaveContactAsync(contact);
                // No need to reload entire list if we just update the object in the collection
            }
        }

        [RelayCommand]
        private async Task SaveContactAsync(Contact contact)
        {
            if (contact != null)
            {
                await _dataService.SaveContactAsync(contact);
            }
        }

        [RelayCommand]
        private void Detail(Contact contact) { /* TODO */ }

        [RelayCommand]
        private void CreateList(Contact contact) { /* TODO */ }

        private void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allContacts
                : _allContacts.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                                          c.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            FilteredContacts.Clear();
            foreach (var contact in filtered)
            {
                FilteredContacts.Add(contact);
            }
        }
    }
}
