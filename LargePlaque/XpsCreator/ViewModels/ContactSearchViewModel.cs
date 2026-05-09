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
                IsPrint = false,
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
                var title = System.Windows.Application.Current.TryFindResource("Delete") as string ?? "Delete";
                var format = System.Windows.Application.Current.TryFindResource("ConfirmDeleteContactFormat") as string ?? "Are you sure you want to delete the contact '{0}'?";
                var message = string.Format(format, contact.Name);
                
                var result = System.Windows.MessageBox.Show(
                    message,
                    title,
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    await _dataService.DeleteContactAsync(contact.Id);
                    await LoadContactsAsync();
                }
            }
        }

        [RelayCommand]
        private async Task PrintContactAsync(Contact contact)
        {
            if (contact != null)
            {
                contact.IsPrint = true;
                await _dataService.SaveContactAsync(contact);
            }
        }

        [RelayCommand]
        private async Task UnprintContactAsync(Contact contact)
        {
            if (contact != null)
            {
                contact.IsPrint = false;
                await _dataService.SaveContactAsync(contact);

                var lives = await _dataService.GetLiveRecordsByContactIdAsync(contact.Id);
                foreach (var live in lives)
                {
                    live.IsPrint = false;
                    await _dataService.SaveLiveAsync(live);
                }

                var deads = await _dataService.GetDeadRecordsByContactIdAsync(contact.Id);
                foreach (var dead in deads)
                {
                    dead.IsPrint = false;
                    await _dataService.SaveDeadAsync(dead);
                }

                var ancestors = await _dataService.GetAncestorRecordsByContactIdAsync(contact.Id);
                foreach (var ancestor in ancestors)
                {
                    ancestor.IsPrint = false;
                    await _dataService.SaveAncestorAsync(ancestor);
                }

                var properties = await _dataService.GetPropertyRecordsByContactIdAsync(contact.Id);
                foreach (var property in properties)
                {
                    property.IsPrint = false;
                    await _dataService.SavePropertyAsync(property);
                }

                if (SelectedContact?.Id == contact.Id)
                {
                    var temp = SelectedContact;
                    SelectedContact = null;
                    SelectedContact = temp;
                }
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
