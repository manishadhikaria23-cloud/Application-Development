namespace JournalApp.Services.Implementations
{
    public class DatabasePathService
    {
        public string GetDatabasePath(string dbName = "journalapp.db3")
        {
            return Path.Combine(FileSystem.AppDataDirectory, dbName);
        }
    }
}
