using Microsoft.AspNetCore.Components;
using JournalApp.Repositories.Interfaces;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Calendar : ComponentBase
    {
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected DateTime CurrentMonth { get; set; } = DateTime.Today;
        protected HashSet<DateTime> EntryDates { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadMonthAsync();
        }

        protected async Task LoadMonthAsync()
        {
            var entries = await Repo.GetAllAsync();
            EntryDates = entries
                .Select(e => e.EntryDate.Date)
                .ToHashSet();
        }

        protected void PrevMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            _ = LoadMonthAsync();
        }

        protected void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            _ = LoadMonthAsync();
        }

        protected void OpenDay(DateTime date)
        {
            Nav.NavigateTo($"/view/{date:yyyy-MM-dd}");
        }
    }
}