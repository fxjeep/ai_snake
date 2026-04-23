using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PlaqueData.Models;

namespace PlaqueData.Data
{
    public class SqliteDataService : IDataService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _dbPath;

        public SqliteDataService(string? customPath = null)
        {
            if (string.IsNullOrWhiteSpace(customPath))
            {
                var defaultFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "DefaultDataFolder");
                if (!Directory.Exists(defaultFolder))
                {
                    Directory.CreateDirectory(defaultFolder);
                }
                _dbPath = Path.Combine(defaultFolder, "pwdatabase.db3");
            }
            else
            {
                _dbPath = customPath;
                var directory = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        public async Task InitializeAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<Contact>();
            await _database.CreateTableAsync<Live>();
            await _database.CreateTableAsync<Dead>();
            await _database.CreateTableAsync<Ancestor>();
            await _database.CreateTableAsync<Property>();
        }

        public async Task<List<Contact>> GetAllContactsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<Contact>().ToListAsync();
        }

        public async Task<List<Live>> GetLiveRecordsByContactIdAsync(int contactId)
        {
            await InitializeAsync();
            return await _database!.Table<Live>().Where(x => x.ContactId == contactId).ToListAsync();
        }

        public async Task<List<Dead>> GetDeadRecordsByContactIdAsync(int contactId)
        {
            await InitializeAsync();
            return await _database!.Table<Dead>().Where(x => x.ContactId == contactId).ToListAsync();
        }

        public async Task<List<Ancestor>> GetAncestorRecordsByContactIdAsync(int contactId)
        {
            await InitializeAsync();
            return await _database!.Table<Ancestor>().Where(x => x.ContactId == contactId).ToListAsync();
        }

        public async Task<List<Property>> GetPropertyRecordsByContactIdAsync(int contactId)
        {
            await InitializeAsync();
            return await _database!.Table<Property>().Where(x => x.ContactId == contactId).ToListAsync();
        }

        public async Task<int> SaveContactAsync(Contact contact)
        {
            await InitializeAsync();
            return contact.Id != 0 ? await _database!.UpdateAsync(contact) : await _database!.InsertAsync(contact);
        }

        public async Task<int> SaveLiveAsync(Live live)
        {
            await InitializeAsync();
            return live.Id != 0 ? await _database!.UpdateAsync(live) : await _database!.InsertAsync(live);
        }

        public async Task<int> SaveDeadAsync(Dead dead)
        {
            await InitializeAsync();
            return dead.Id != 0 ? await _database!.UpdateAsync(dead) : await _database!.InsertAsync(dead);
        }

        public async Task<int> SaveAncestorAsync(Ancestor ancestor)
        {
            await InitializeAsync();
            return ancestor.Id != 0 ? await _database!.UpdateAsync(ancestor) : await _database!.InsertAsync(ancestor);
        }

        public async Task<int> SavePropertyAsync(Property property)
        {
            await InitializeAsync();
            return property.Id != 0 ? await _database!.UpdateAsync(property) : await _database!.InsertAsync(property);
        }

        public async Task DeleteContactAsync(int contactId)
        {
            await InitializeAsync();
            await _database!.DeleteAsync<Contact>(contactId);
        }
    }
}
