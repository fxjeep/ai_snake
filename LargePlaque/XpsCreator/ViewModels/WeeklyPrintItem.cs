using CommunityToolkit.Mvvm.ComponentModel;

namespace XpsCreator.ViewModels
{
    public partial class WeeklyPrintItem : ObservableObject
    {
        [ObservableProperty]
        private string _contactName = string.Empty;

        [ObservableProperty]
        private string _contactCode = string.Empty;

        [ObservableProperty]
        private string _summary = string.Empty;

        // Additional properties can be added here, such as ContactId if action buttons need to identify the contact.
        public int ContactId { get; set; }
    }
}
