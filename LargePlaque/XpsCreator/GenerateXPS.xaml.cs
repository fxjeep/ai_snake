using System.Windows;
using System.Windows.Controls;

namespace XpsCreator;

/// <summary>
/// Interaction logic for GenerateXPS.xaml
/// </summary>
public partial class GenerateXPS : UserControl
{
    public GenerateXPS()
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

            var settings = PrintLayoutSettings.Load(PrintLayoutSettings.DefaultConfigPath);
            var typeConfig = settings.GetConfig(selectedType);

            if (typeConfig == null)
            {
                MessageBox.Show($"Configuration not found for type: {selectedType}", "Error");
                return;
            }

            var mainConfig = new MainBoxConfigure
            {
                TextPrintBox = UnitConverter.ToTextConfig(typeConfig.MainTextBox),
                StampPosition = UnitConverter.ToStampConfig(typeConfig.Stamp) ?? new StampPositionConfig()
            };

            // Determine max font sizes based on type
            double mainMaxFontSize = typeConfig.MainMaxFontSize;
            double sideMaxFontSize = typeConfig.SideMaxFontSize;

            if (selectedType == "长生")
            {
                LivePrint.GenerateXps(openFileDialog.FileName, printBorder, printStamp, printNames, mainConfig, mainMaxFontSize);
            }
            else if (selectedType == "冤亲" || selectedType == "往生" || selectedType == "祖先")
            {
                var sideConfig = UnitConverter.ToTextConfig(typeConfig.SideTextBox);
                sideConfig.ColumnGap = 5;
                TwoSectionPrint.GenerateXps(openFileDialog.FileName, printBorder, printStamp, printNames, mainConfig, sideConfig, mainMaxFontSize, sideMaxFontSize);
            }
            else
            {
                MessageBox.Show($"Selected type: {selectedType}", "Info");
            }
        }
    }
}
