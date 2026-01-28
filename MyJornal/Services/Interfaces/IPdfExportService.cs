using JournalApp.Models.Entities;

namespace JournalApp.Services.Interfaces
{
    public interface IPdfExportService
    {
        Task<string> ExportAsync(List<JournalEntry> entries, string fileName);
    }
}
