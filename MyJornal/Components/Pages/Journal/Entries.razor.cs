using Microsoft.AspNetCore.Components;
using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Entries : ComponentBase
    {
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected List<JournalEntry> AllEntries { get; set; } = new();
        protected List<JournalEntry> PageEntries { get; set; } = new();

        protected string SearchText { get; set; } = string.Empty;
        protected DateTime? FromDate { get; set; }
        protected DateTime? ToDate { get; set; }
        protected JournalApp.Models.Enums.MoodType? MoodFilter { get; set; }

        protected int PageSize { get; set; } = 6;
        protected int CurrentPage { get; set; } = 1;
        protected int TotalPages { get; set; } = 1;

        protected override async Task OnInitializedAsync()
        {
            var entries = await Repo.GetAllAsync();
            // Sort by newest first
            AllEntries = entries.OrderByDescending(x => x.EntryDate).ToList();
            ApplyPagination();
        }

        protected void OnSearchChanged(ChangeEventArgs e)
        {
            SearchText = e.Value?.ToString() ?? string.Empty;
            ApplyFilters();
        }

        protected void ApplyFilters()
        {
            CurrentPage = 1;
            ApplyPagination();
        }

        protected void ClearFilters()
        {
            SearchText = string.Empty;
            FromDate = null;
            ToDate = null;
            MoodFilter = null;
            ApplyFilters();
        }

        private void ApplyPagination()
        {
            var query = AllEntries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim().ToLower();
                query = query.Where(e =>
                    (e.Title ?? "").ToLower().Contains(q) ||
                    (e.Content ?? "").ToLower().Contains(q));
            }

            if (FromDate.HasValue) query = query.Where(e => e.EntryDate.Date >= FromDate.Value.Date);
            if (ToDate.HasValue) query = query.Where(e => e.EntryDate.Date <= ToDate.Value.Date);
            if (MoodFilter.HasValue) query = query.Where(e => e.PrimaryMood == MoodFilter.Value);

            var filteredList = query.ToList();
            TotalPages = Math.Max(1, (int)Math.Ceiling(filteredList.Count / (double)PageSize));

            PageEntries = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            StateHasChanged();
        }

        protected void NextPage() { if (CurrentPage < TotalPages) { CurrentPage++; ApplyPagination(); } }
        protected void PrevPage() { if (CurrentPage > 1) { CurrentPage--; ApplyPagination(); } }

        protected string GetMoodEmoji(JournalApp.Models.Enums.MoodType mood) => mood switch
        {
            JournalApp.Models.Enums.MoodType.Happy => "😊",
            JournalApp.Models.Enums.MoodType.Sad => "😢",
            JournalApp.Models.Enums.MoodType.Angry => "😠",
            JournalApp.Models.Enums.MoodType.Calm => "😌",
            _ => "😶"
        };
    }
}