using Microsoft.Extensions.Logging;
using JournalApp.Data.Database;
using JournalApp.Services.Implementations;
using JournalApp.Repositories.Interfaces;
using JournalApp.Repositories.Implementations;
using JournalApp.Services.Interfaces;


namespace MyJornal
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<DatabasePathService>();

            builder.Services.AddSingleton<AppDatabase>(sp =>
            {
                var pathService = sp.GetRequiredService<DatabasePathService>();
                var dbPath = pathService.GetDatabasePath();
                return new AppDatabase(dbPath);
            });
            builder.Services.AddSingleton<IJournalEntryRepository, JournalEntryRepository>();
            builder.Services.AddSingleton<IJournalEntryService, JournalEntryService>();
            builder.Services.AddSingleton<IStreakService, StreakService>();
            builder.Services.AddSingleton<IThemeService, ThemeService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
            builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();



#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
