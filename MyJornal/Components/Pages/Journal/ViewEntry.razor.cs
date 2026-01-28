using Microsoft.AspNetCore.Components;
using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;

namespace JournalApp.Components.Pages.Journal
{
    public partial class ViewEntry : ComponentBase
    {
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;

        [Parameter] public string Date { get; set; } = string.Empty;

        protected JournalEntry? Entry { get; set; }
        protected string Message { get; set; } = string.Empty;

        protected override async Task OnParametersSetAsync()
        {
            Message = string.Empty;

            if (!DateTime.TryParse(Date, out var parsed))
            {
                Message = "Invalid date.";
                Entry = null;
                return;
            }

            Entry = await Repo.GetByDateAsync(parsed.Date);

            if (Entry == null)
                Message = "No entry found for this date.";
        }
    }
}
