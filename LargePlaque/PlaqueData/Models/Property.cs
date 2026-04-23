using SQLite;

namespace PlaqueData.Models
{
    public class Property
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ContactId { get; set; }

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(10)]
        public string LastPrint { get; set; } = string.Empty;

        public int IsPrint { get; set; }
    }
}
