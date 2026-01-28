using JournalApp.Services.Interfaces;

namespace JournalApp.Services.Implementations
{
    public class ThemeService : IThemeService
    {
        private const string ThemeKey = "app_theme";

        public string GetTheme()
        {
            var theme = Preferences.Get(ThemeKey, "light");
            return theme == "dark" ? "dark" : "light";
        }

        public void SetTheme(string theme)
        {
            var safe = theme == "dark" ? "dark" : "light";
            Preferences.Set(ThemeKey, safe);
        }
    }
}
