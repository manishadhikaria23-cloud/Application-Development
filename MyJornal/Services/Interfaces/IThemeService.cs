namespace JournalApp.Services.Interfaces
{
    public interface IThemeService
    {
        string GetTheme();          // "light" or "dark"
        void SetTheme(string theme);
    }
}
