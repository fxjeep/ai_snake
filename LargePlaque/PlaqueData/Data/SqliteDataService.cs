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
        private string _dbPath;

        public async Task ChangeDatabaseAsync(string path)
        {
            if (_database != null)
            {
                await _database.CloseAsync();
                _database = null;
            }
            _dbPath = path;
            await InitializeAsync();
        }

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
        
        public async Task<List<Live>> GetWeeklyLivePrintRecordsAsync()
        {
            await InitializeAsync();
            return await _database!.QueryAsync<Live>(
                "SELECT * FROM Live WHERE IsPrint = 1 OR ContactId IN (SELECT Id FROM Contact WHERE IsPrint = 1)");
        }

        public async Task<List<Dead>> GetWeeklyDeadPrintRecordsAsync()
        {
            await InitializeAsync();
            return await _database!.QueryAsync<Dead>(
                "SELECT * FROM Dead WHERE IsPrint = 1 OR ContactId IN (SELECT Id FROM Contact WHERE IsPrint = 1)");
        }

        public async Task<List<Ancestor>> GetWeeklyAncestorPrintRecordsAsync()
        {
            await InitializeAsync();
            return await _database!.QueryAsync<Ancestor>(
                "SELECT * FROM Ancestor WHERE IsPrint = 1 OR ContactId IN (SELECT Id FROM Contact WHERE IsPrint = 1)");
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

        public async Task<Contact?> GetContactByIdAsync(int contactId)
        {
            await InitializeAsync();
            return await _database!.Table<Contact>().Where(x => x.Id == contactId).FirstOrDefaultAsync();
        }

        public async Task UnprintContactAsync(int contactId)
        {
            await InitializeAsync();
            await _database!.ExecuteAsync("UPDATE Contact SET IsPrint = 0 WHERE Id = ?", contactId);
            await _database!.ExecuteAsync("UPDATE Live SET IsPrint = 0 WHERE ContactId = ?", contactId);
            await _database!.ExecuteAsync("UPDATE Dead SET IsPrint = 0 WHERE ContactId = ?", contactId);
            await _database!.ExecuteAsync("UPDATE Ancestor SET IsPrint = 0 WHERE ContactId = ?", contactId);
            await _database!.ExecuteAsync("UPDATE Property SET IsPrint = 0 WHERE ContactId = ?", contactId);
        }

        public async Task<List<ContactPrintSummary>> GetPrintSummariesAsync()
        {
            await InitializeAsync();
            
            // Get all contacts first as requested, but we will filter them in memory 
            // or better, find which ones are relevant first to avoid massive looping.
            // However, to follow the "get all contacts first" request:
            var allContacts = await GetAllContactsAsync();
            
            // Efficiently find which contacts have any print markers
            var livePrintIds = new HashSet<int>(await _database!.QueryScalarsAsync<int>("SELECT ContactId FROM Live WHERE IsPrint = 1"));
            var deadPrintIds = new HashSet<int>(await _database!.QueryScalarsAsync<int>("SELECT ContactId FROM Dead WHERE IsPrint = 1"));
            var ancestorPrintIds = new HashSet<int>(await _database!.QueryScalarsAsync<int>("SELECT ContactId FROM Ancestor WHERE IsPrint = 1"));
            var propertyPrintIds = new HashSet<int>(await _database!.QueryScalarsAsync<int>("SELECT ContactId FROM Property WHERE IsPrint = 1"));

            var summaries = new List<ContactPrintSummary>();

            foreach (var contact in allContacts)
            {
                bool hasPrintDetail = livePrintIds.Contains(contact.Id) || 
                                      deadPrintIds.Contains(contact.Id) || 
                                      ancestorPrintIds.Contains(contact.Id) || 
                                      propertyPrintIds.Contains(contact.Id);

                if (contact.IsPrint || hasPrintDetail)
                {
                    // Only perform count queries for contacts that are actually going to be displayed
                    var livePrint = livePrintIds.Contains(contact.Id) ? await _database!.Table<Live>().Where(x => x.ContactId == contact.Id && x.IsPrint).CountAsync() : 0;
                    var deadPrint = deadPrintIds.Contains(contact.Id) ? await _database!.Table<Dead>().Where(x => x.ContactId == contact.Id && x.IsPrint).CountAsync() : 0;
                    var ancestorPrint = ancestorPrintIds.Contains(contact.Id) ? await _database!.Table<Ancestor>().Where(x => x.ContactId == contact.Id && x.IsPrint).CountAsync() : 0;
                    var propertyPrint = propertyPrintIds.Contains(contact.Id) ? await _database!.Table<Property>().Where(x => x.ContactId == contact.Id && x.IsPrint).CountAsync() : 0;

                    var liveTotal = await _database!.Table<Live>().Where(x => x.ContactId == contact.Id).CountAsync();
                    var deadTotal = await _database!.Table<Dead>().Where(x => x.ContactId == contact.Id).CountAsync();
                    var ancestorTotal = await _database!.Table<Ancestor>().Where(x => x.ContactId == contact.Id).CountAsync();
                    var propertyTotal = await _database!.Table<Property>().Where(x => x.ContactId == contact.Id).CountAsync();

                    summaries.Add(new ContactPrintSummary
                    {
                        Id = contact.Id,
                        Name = contact.Name,
                        Code = contact.Code,
                        IsPrint = contact.IsPrint,
                        LiveTotal = liveTotal,
                        LivePrint = livePrint,
                        DeadTotal = deadTotal,
                        DeadPrint = deadPrint,
                        AncestorTotal = ancestorTotal,
                        AncestorPrint = ancestorPrint,
                        PropertyTotal = propertyTotal,
                        PropertyPrint = propertyPrint
                    });
                }
            }
            return summaries;
        }

        public async Task ClearAllPrintAsync()
        {
            await InitializeAsync();
            await _database!.ExecuteAsync("UPDATE Contact SET IsPrint = 0 WHERE IsPrint = 1");
            await _database!.ExecuteAsync("UPDATE Live SET IsPrint = 0 WHERE IsPrint = 1");
            await _database!.ExecuteAsync("UPDATE Dead SET IsPrint = 0 WHERE IsPrint = 1");
            await _database!.ExecuteAsync("UPDATE Ancestor SET IsPrint = 0 WHERE IsPrint = 1");
            await _database!.ExecuteAsync("UPDATE Property SET IsPrint = 0 WHERE IsPrint = 1");
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

        public async Task DeleteLiveAsync(int id)
        {
            await InitializeAsync();
            await _database!.DeleteAsync<Live>(id);
        }

        public async Task DeleteDeadAsync(int id)
        {
            await InitializeAsync();
            await _database!.DeleteAsync<Dead>(id);
        }

        public async Task DeleteAncestorAsync(int id)
        {
            await InitializeAsync();
            await _database!.DeleteAsync<Ancestor>(id);
        }

        public async Task DeletePropertyAsync(int id)
        {
            await InitializeAsync();
            await _database!.DeleteAsync<Property>(id);
        }
    }
}
