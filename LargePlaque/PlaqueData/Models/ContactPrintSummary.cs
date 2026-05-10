namespace PlaqueData.Models
{
    public class ContactPrintSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsPrint { get; set; }
        
        public int LiveTotal { get; set; }
        public int LivePrint { get; set; }
        
        public int DeadTotal { get; set; }
        public int DeadPrint { get; set; }
        
        public int AncestorTotal { get; set; }
        public int AncestorPrint { get; set; }
        
        public int PropertyTotal { get; set; }
        public int PropertyPrint { get; set; }
    }
}
