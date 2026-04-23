using System.Windows;
using System.Windows.Controls;
using XpsCreator.ViewModels;
using PlaqueData.Models;

namespace XpsCreator.Views
{
    public partial class ContactSearch : UserControl
    {
        public ContactSearch()
        {
            InitializeComponent();
        }

        private async void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var contact = e.Row.Item as Contact;
                if (contact != null && DataContext is ContactSearchViewModel vm)
                {
                    // Delay slightly to let the binding update the object
                    await System.Threading.Tasks.Task.Delay(100);
                    await vm.SaveContactCommand.ExecuteAsync(contact);
                }
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}
