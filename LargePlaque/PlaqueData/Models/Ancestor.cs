using SQLite;

namespace PlaqueData.Models
{
    public class Ancestor
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int ContactId { get; set; }
        
        [MaxLength(50)]
        public string Surname { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string LastPrint { get; set; } = string.Empty;
        
        public bool IsPrint { get; set; } = false;
    }
}
