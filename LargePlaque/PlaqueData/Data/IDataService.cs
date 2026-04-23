using System.Collections.Generic;
using System.Threading.Tasks;
using PlaqueData.Models;

namespace PlaqueData.Data
{
    public interface IDataService
    {
        Task InitializeAsync();
        
        Task<List<Contact>> GetAllContactsAsync();
        Task<List<Live>> GetLiveRecordsByContactIdAsync(int contactId);
        Task<List<Dead>> GetDeadRecordsByContactIdAsync(int contactId);
        Task<List<Ancestor>> GetAncestorRecordsByContactIdAsync(int contactId);
        Task<List<Property>> GetPropertyRecordsByContactIdAsync(int contactId);
        
        Task<int> SaveContactAsync(Contact contact);
        Task<int> SaveLiveAsync(Live live);
        Task<int> SaveDeadAsync(Dead dead);
        Task<int> SaveAncestorAsync(Ancestor ancestor);
        Task<int> SavePropertyAsync(Property property);

        Task DeleteContactAsync(int contactId);
    }
}
