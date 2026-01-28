namespace JournalApp.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<Dictionary<string, int>> GetMoodCategoryDistributionAsync(DateTime from, DateTime to); // Positive/Neutral/Negative
        Task<string?> GetMostFrequentMoodAsync(DateTime from, DateTime to);

        // New: counts per explicit mood (e.g., "Happy", "Thoughtful", etc.)
        Task<Dictionary<string, int>> GetMoodCountsAsync(DateTime from, DateTime to);

        Task<Dictionary<string, int>> GetMostUsedTagsAsync(DateTime from, DateTime to, int top = 10);
        Task<List<(DateTime date, int wordCount)>> GetWordCountTrendAsync(DateTime from, DateTime to);
    }
}