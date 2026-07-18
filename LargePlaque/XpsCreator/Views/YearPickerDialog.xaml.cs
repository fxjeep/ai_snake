using System;
using System.Windows;

namespace XpsCreator.Views
{
    /// <summary>
    /// Interaction logic for YearPickerDialog.xaml
    /// </summary>
    public partial class YearPickerDialog : Window
    {
        public int SelectedYear { get; private set; }

        public YearPickerDialog()
        {
            InitializeComponent();
            PopulateYears();
        }

        private void PopulateYears()
        {
            int currentYear = DateTime.Now.Year;
            for (int year = 2020; year <= currentYear; year++)
            {
                YearComboBox.Items.Add(year);
            }
            // Select 2020 by default
            YearComboBox.SelectedItem = 2020;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (YearComboBox.SelectedItem is int year)
            {
                SelectedYear = year;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a year.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
