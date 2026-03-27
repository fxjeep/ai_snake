using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XpsCreator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog();
        openFileDialog.Filter = "XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        openFileDialog.Title = "Select a file to convert to XPS";

        if (openFileDialog.ShowDialog() == true)
        {
            string selectedType = TypeComboBox.Text;
            if (TypeComboBox.SelectedItem is ComboBoxItem cbi)
            {
                selectedType = cbi.Content.ToString() ?? "";
            }

            bool printBorder = PrintBorderCheckBox.IsChecked == true;
            bool printStamp = PrintRedStampCheckBox.IsChecked == true;
            bool printNames = PrintNamesCheckBox.IsChecked == true;

            if (selectedType == "长生")
            {
                LivePrint.GenerateXps(openFileDialog.FileName, printBorder, printStamp, printNames, PrintConfigure.ChangeShengConfig, PrintConfigure.Main);
            }
            else if (selectedType == "冤亲")
            {
                LivePrint.GenerateXps(openFileDialog.FileName, printBorder, printStamp, printNames, PrintConfigure.YuanQinConfig, PrintConfigure.SideYuanQin);
            }
            else if (selectedType == "往生")
            {
                TwoSectionPrint.GenerateXps(openFileDialog.FileName, printBorder, printStamp, printNames, PrintConfigure.WangShengMain, PrintConfigure.WangShengSide);
            }
            else if (selectedType == "祖先")
            {
                TwoSectionPrint.GenerateXps(openFileDialog.FileName, printBorder, printStamp, printNames, PrintConfigure.ZhuXianMain, PrintConfigure.ZhuXianSide);
            }
            else
            {
                MessageBox.Show($"Selected type: {selectedType}", "Info");
            }
        }
    }
}