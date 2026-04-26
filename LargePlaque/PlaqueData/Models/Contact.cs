using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlaqueData.Models
{
    public class Contact : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string LastPrint { get; set; } = string.Empty;
        
        private bool _isPrint = false;
        public bool IsPrint 
        { 
            get => _isPrint;
            set
            {
                if (_isPrint != value)
                {
                    _isPrint = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
