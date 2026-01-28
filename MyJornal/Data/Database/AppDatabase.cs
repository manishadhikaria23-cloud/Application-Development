using SQLite;
using JournalApp.Models.Entities;

namespace JournalApp.Data.Database
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public AppDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTableAsync<JournalEntry>().Wait();
            _database.CreateTableAsync<Tag>().Wait();
            _database.CreateTableAsync<UserSecurity>().Wait();

        }

        public SQLiteAsyncConnection Connection => _database;
    }
}
