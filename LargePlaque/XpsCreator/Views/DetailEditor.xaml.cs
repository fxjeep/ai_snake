using System.Windows;
using System.Windows.Controls;
using XpsCreator.ViewModels;
using PlaqueData.Models;
using System.Linq;

namespace XpsCreator.Views
{
    public partial class DetailEditor : UserControl
    {
        public DetailEditor()
        {
            InitializeComponent();
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (DataContext is DetailEditorViewModel vm && vm.CurrentContact != null)
            {
                int contactId = vm.CurrentContact.Id;
                if (e.NewItem is Live live) { live.IsPrint = true; live.ContactId = contactId; }
                else if (e.NewItem is Dead dead) { dead.IsPrint = true; dead.ContactId = contactId; }
                else if (e.NewItem is Ancestor ancestor) { ancestor.IsPrint = true; ancestor.ContactId = contactId; }
                else if (e.NewItem is PlaqueData.Models.Property property) { property.IsPrint = true; property.ContactId = contactId; }
            }
        }

        private async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && DataContext is DetailEditorViewModel vm)
            {
                // We need to wait for the binding to update the object
                await System.Threading.Tasks.Task.Delay(100);
                await vm.SaveItemAsync(e.Row.Item);
            }
        }

        private DataGrid? GetActiveDataGrid()
        {
            if (DetailTabControl.SelectedItem is TabItem selectedTab)
            {
                return selectedTab.Content as DataGrid;
            }
            return null;
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = GetActiveDataGrid();
            if (grid != null && DataContext is DetailEditorViewModel vm)
            {
                await vm.SetPrintStatusAsync(grid.SelectedItems, true);
                grid.Items.Refresh();
            }
        }

        private async void NotPrintButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = GetActiveDataGrid();
            if (grid != null && DataContext is DetailEditorViewModel vm)
            {
                await vm.SetPrintStatusAsync(grid.SelectedItems, false);
                grid.Items.Refresh();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = GetActiveDataGrid();
            if (grid != null && grid.SelectedItems.Count > 0 && DataContext is DetailEditorViewModel vm)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete {grid.SelectedItems.Count} selected records?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await vm.DeleteItemsAsync(grid.SelectedItems);
                }
            }
        }
    }
}
