using SQLite;
using JournalApp.Models.Enums;

namespace JournalApp.Models.Entities
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public DateTime EntryDate { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty; // Markdown text

        public MoodType PrimaryMood { get; set; }

        public string? SecondaryMoods { get; set; }
        // Stored as comma-separated values

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public string? Tags { get; set; } // stored as comma separated

    }
}
