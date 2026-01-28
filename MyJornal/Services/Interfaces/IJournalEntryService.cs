using JournalApp.Models.Entities;

namespace JournalApp.Services.Interfaces
{
    public interface IJournalEntryService
    {
        Task<JournalEntry?> GetTodayEntryAsync();
        Task<JournalEntry> CreateOrUpdateTodayAsync(JournalEntry input);
        Task<bool> DeleteTodayAsync();
    }
}
