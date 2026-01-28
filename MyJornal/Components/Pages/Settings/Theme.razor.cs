using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using JournalApp.Services.Interfaces;

namespace JournalApp.Components.Pages.Settings
{
    public partial class Theme : ComponentBase
    {
        [Inject] public IThemeService ThemeService { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        protected string SelectedTheme { get; set; } = "light";
        protected string Message { get; set; } = string.Empty;

        protected override void OnInitialized()
        {
            SelectedTheme = ThemeService.GetTheme();
        }

        protected async void SaveTheme()
        {
            ThemeService.SetTheme(SelectedTheme);
            // apply immediately
            try
            {
                await JS.InvokeVoidAsync("setTheme", SelectedTheme);
                Message = "Theme saved and applied.";
            }
            catch
            {
                Message = "Theme saved. Restart app to fully apply.";
            }
        }
    }
}