using JournalApp.Models.Entities;

namespace JournalApp.Repositories.Interfaces
{
    public interface IJournalEntryRepository
    {
        Task<JournalEntry?> GetByDateAsync(DateTime date);
        Task<int> SaveAsync(JournalEntry entry);
        Task<int> DeleteAsync(JournalEntry entry);
        Task<List<JournalEntry>> GetAllAsync();
    }
}
