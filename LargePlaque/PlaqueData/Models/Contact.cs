using SQLite;

namespace PlaqueData.Models
{
    public class Contact
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string LastPrint { get; set; } = string.Empty;
        
        public int IsPrint { get; set; }
    }
}
