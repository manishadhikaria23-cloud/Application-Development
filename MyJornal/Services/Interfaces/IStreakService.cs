namespace JournalApp.Services.Interfaces
{
    public interface IStreakService
    {
        Task<int> GetCurrentStreakAsync();
        Task<int> GetLongestStreakAsync();
        Task<List<DateTime>> GetMissedDaysAsync(DateTime from, DateTime to);
    }
}
