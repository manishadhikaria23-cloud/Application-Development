using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services.Implementations
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IJournalEntryRepository _repo;

        public JournalEntryService(IJournalEntryRepository repo)
        {
            _repo = repo;
        }

        public async Task<JournalEntry?> GetTodayEntryAsync()
        {
            return await _repo.GetByDateAsync(DateTime.Today);
        }

        public async Task<JournalEntry> CreateOrUpdateTodayAsync(JournalEntry input)
        {
            var today = DateTime.Today;
            var existing = await _repo.GetByDateAsync(today);

            if (existing == null)
            {
                var newEntry = new JournalEntry
                {
                    EntryDate = today,
                    Title = input.Title?.Trim() ?? string.Empty,
                    Content = input.Content ?? string.Empty,
                    PrimaryMood = input.PrimaryMood,
                    SecondaryMoods = input.SecondaryMoods,
                    Category = input.Category,
                    Tags = input.Tags,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _repo.SaveAsync(newEntry);
                return newEntry;
            }

            existing.Title = input.Title?.Trim() ?? string.Empty;
            existing.Content = input.Content ?? string.Empty;
            existing.PrimaryMood = input.PrimaryMood;
            existing.SecondaryMoods = input.SecondaryMoods;
            existing.Category = input.Category;
            existing.Tags = input.Tags;
            existing.UpdatedAt = DateTime.Now;

            await _repo.SaveAsync(existing);
            return existing;
        }

        public async Task<bool> DeleteTodayAsync()
        {
            var existing = await _repo.GetByDateAsync(DateTime.Today);
            if (existing == null) return false;

            await _repo.DeleteAsync(existing);
            return true;
        }
    }
}
