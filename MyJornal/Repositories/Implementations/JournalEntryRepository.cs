using JournalApp.Data.Database;
using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;
using SQLite;

namespace JournalApp.Repositories.Implementations
{
    public class JournalEntryRepository : IJournalEntryRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public JournalEntryRepository(AppDatabase database)
        {
            _db = database.Connection;
        }

        public async Task<JournalEntry?> GetByDateAsync(DateTime date)
        {
            var onlyDate = date.Date;

            return await _db.Table<JournalEntry>()
                            .Where(e => e.EntryDate == onlyDate)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAsync(JournalEntry entry)
        {
            if (entry.Id == 0)
                return await _db.InsertAsync(entry);

            return await _db.UpdateAsync(entry);
        }

        public async Task<int> DeleteAsync(JournalEntry entry)
        {
            return await _db.DeleteAsync(entry);
        }

        public async Task<List<JournalEntry>> GetAllAsync()
        {
            return await _db.Table<JournalEntry>()
                            .OrderByDescending(e => e.EntryDate)
                            .ToListAsync();
        }
    }
}
