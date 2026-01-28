using Microsoft.AspNetCore.Components;
using JournalApp.Repositories.Interfaces;
using JournalApp.Services.Interfaces;
using JournalApp.Models.Entities;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Export : ComponentBase
    {
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;
        [Inject] public IPdfExportService PdfExport { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        // Form Bindings
        protected DateTime FromDate { get; set; } = DateTime.Today.AddDays(-7);
        protected DateTime ToDate { get; set; } = DateTime.Today;
        protected string FileName { get; set; } = "my_journal_export";

        // UI State
        protected string Message { get; set; } = string.Empty;
        protected string? ExportedPath { get; set; }
        protected bool IsProcessing { get; set; } = false;

        protected async Task ExportAsync()
        {
            // Reset state
            Message = "Preparing your journal entries...";
            ExportedPath = null;
            IsProcessing = true;

            try
            {
                // 1. Validate Date Logic
                var start = FromDate.Date;
                var end = ToDate.Date;
                if (end < start) (start, end) = (end, start);

                // 2. Fetch and Filter
                // Note: Consider adding GetByDateRange to your Repo for better performance
                var allEntries = await Repo.GetAllAsync();
                var filtered = allEntries
                    .Where(x => x.EntryDate.Date >= start && x.EntryDate.Date <= end)
                    .OrderBy(x => x.EntryDate)
                    .ToList();

                if (!filtered.Any())
                {
                    Message = "No entries found for the selected dates.";
                    IsProcessing = false;
                    return;
                }

                // 3. Clean Filename (Remove invalid characters)
                var cleanName = string.Concat(FileName.Split(Path.GetInvalidFileNameChars()));
                if (string.IsNullOrWhiteSpace(cleanName)) cleanName = "journal_export";

                // 4. Execute Export (This calls the PdfSharp logic)
                var resultPath = await PdfExport.ExportAsync(filtered, cleanName);

                // 5. Success State
                ExportedPath = resultPath;
                Message = "Export successful!";
            }
            catch (Exception ex)
            {
                Message = "An error occurred during export. Please try again.";
                Console.WriteLine($"PDF Export Error: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged(); // Force UI to update
            }
        }
    }
}