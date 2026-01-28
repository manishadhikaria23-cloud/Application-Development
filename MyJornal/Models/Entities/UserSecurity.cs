using SQLite;

namespace JournalApp.Models.Entities
{
    public class UserSecurity
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;

        public string PinHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
