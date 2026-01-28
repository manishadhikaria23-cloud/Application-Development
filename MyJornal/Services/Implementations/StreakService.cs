using JournalApp.Repositories.Interfaces;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services.Implementations
{
    public class StreakService : IStreakService
    {
        private readonly IJournalEntryRepository _repo;

        public StreakService(IJournalEntryRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            var entries = await _repo.GetAllAsync();
            var dates = entries.Select(e => e.EntryDate.Date).Distinct().ToHashSet();

            var streak = 0;
            var day = DateTime.Today.Date;

            while (dates.Contains(day))
            {
                streak++;
                day = day.AddDays(-1);
            }

            return streak;
        }

        public async Task<int> GetLongestStreakAsync()
        {
            var entries = await _repo.GetAllAsync();
            var ordered = entries.Select(e => e.EntryDate.Date).Distinct().OrderBy(d => d).ToList();

            if (ordered.Count == 0) return 0;

            var longest = 1;
            var current = 1;

            for (int i = 1; i < ordered.Count; i++)
            {
                if ((ordered[i] - ordered[i - 1]).TotalDays == 1)
                {
                    current++;
                    if (current > longest) longest = current;
                }
                else
                {
                    current = 1;
                }
            }

            return longest;
        }

        public async Task<List<DateTime>> GetMissedDaysAsync(DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date;

            if (end < start) (start, end) = (end, start);

            var entries = await _repo.GetAllAsync();
            var dates = entries.Select(e => e.EntryDate.Date).Distinct().ToHashSet();

            var missed = new List<DateTime>();
            var day = start;

            while (day <= end)
            {
                if (!dates.Contains(day))
                    missed.Add(day);

                day = day.AddDays(1);
            }

            return missed;
        }
    }
}
