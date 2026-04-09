using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;

namespace XpsCreator;

public class InputItem
{
    public int Index { get; set; }
    public List<string> MainLines { get; set; } = new();
    public List<string> SideLines { get; set; } = new();

    public override string ToString()
    {
        // Show first non-empty line as the list label
        var first = MainLines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))
                 ?? SideLines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))
                 ?? $"(Item {Index + 1})";
        return $"{Index + 1}. {first}";
    }
}

public partial class TextInput : UserControl
{
    private string _filePath = "";
    private ObservableCollection<InputItem> _items = new();
    private bool _isUpdating = false;
    public TextInput()
    {
        InitializeComponent();
        ItemListBox.ItemsSource = _items;
        UpdateEditorVisibility();
    }

    // ── File open ────────────────────────────────────────────────────────────

    private void NewFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "XML Files (*.xml)|*.xml",
            Title = "Create New XML File",
            DefaultExt = ".xml"
        };
        if (dlg.ShowDialog() == true)
        {
            string currentType = (TypeComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "长生";
            var doc = new XDocument(new XElement("list", new XAttribute("type", currentType)));
            doc.Save(dlg.FileName);

            _filePath = dlg.FileName;
            FilePathTxt.Text = _filePath;
            _items.Clear();
            _isUpdating = true;
            MainTextBox.Text = "";
            SideTextBox.Text = "";
            _isUpdating = false;
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            Title = "Open XML File"
        };
        if (dlg.ShowDialog() == true)
        {
            _filePath = dlg.FileName;
            FilePathTxt.Text = _filePath;
            LoadXml();
        }
    }

    private void LoadXml()
    {
        _items.Clear();
        try
        {
            var doc = XDocument.Load(_filePath);

            // Load type attribute if present
            string typeAttr = doc.Root?.Attribute("type")?.Value;
            if (!string.IsNullOrEmpty(typeAttr))
            {
                foreach (ComboBoxItem item in TypeComboBox.Items)
                {
                    if (item.Content.ToString() == typeAttr)
                    {
                        TypeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            int index = 0;
            foreach (var itemEl in doc.Descendants("item"))
            {
                var item = new InputItem { Index = index++ };

                var mainEl = itemEl.Element("main");
                if (mainEl != null)
                    item.MainLines = ExtractLines(mainEl);

                var sideEl = itemEl.Element("side");
                if (sideEl != null)
                    item.SideLines = ExtractLines(sideEl);

                _items.Add(item);
            }

            if (_items.Count > 0)
                ItemListBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading file:\n{ex.Message}", "Error");
        }
    }

    private static List<string> ExtractLines(XElement el)
    {
        var lineEls = el.Elements("line").ToList();
        if (lineEls.Any())
            return lineEls.Select(GetInnerXml).ToList();

        // Fallback: plain text, no <line> children
        var text = el.Value.Trim();
        return string.IsNullOrEmpty(text) ? new() : new() { text };
    }

    private static string GetInnerXml(XElement el)
    {
        using var reader = el.CreateReader();
        reader.MoveToContent();
        return reader.ReadInnerXml();
    }

    // ── Selection changed ────────────────────────────────────────────────────

    private void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemListBox.SelectedItem is not InputItem item) return;

        _isUpdating = true;
        MainTextBox.Text = string.Join("\n", item.MainLines);
        SideTextBox.Text = string.Join("\n", item.SideLines);
        _isUpdating = false;
    }

    // ── Text changed (update model) ──────────────────────────────────────────

    private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        if (ItemListBox.SelectedItem is InputItem item)
            item.MainLines = SplitLines(MainTextBox.Text);
        RefreshListDisplay();
    }

    private void SideTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        if (ItemListBox.SelectedItem is InputItem item)
            item.SideLines = SplitLines(SideTextBox.Text);
    }

    private static List<string> SplitLines(string text)
        => text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
               .ToList();

    // ── Add / Delete item ────────────────────────────────────────────────────

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        var item = new InputItem { Index = _items.Count };
        _items.Add(item);
        ItemListBox.SelectedItem = item;
    }

    private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemListBox.SelectedItem is not InputItem item) return;
        int idx = _items.IndexOf(item);
        _items.Remove(item);
        // Re-index
        for (int i = 0; i < _items.Count; i++) _items[i].Index = i;
        if (_items.Count > 0)
            ItemListBox.SelectedIndex = Math.Min(idx, _items.Count - 1);
    }

    // ── Type selection (show/hide editors) ───────────────────────────────────

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateEditorVisibility();
    }

    private void UpdateEditorVisibility()
    {
        string type = (TypeComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        bool showMain = type != "冤亲";
        bool showSide = type != "长生";

        if (MainTextGroup != null)
            MainTextGroup.Visibility = showMain ? Visibility.Visible : Visibility.Collapsed;
        if (SideTextGroup != null)
            SideTextGroup.Visibility = showSide ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            MessageBox.Show("No file is open.", "Save");
            return;
        }

        try
        {
            // Commit any pending edit in the currently selected text box
            if (ItemListBox.SelectedItem is InputItem selected)
            {
                selected.MainLines = String.IsNullOrEmpty(MainTextBox.Text) ? new List<string>() : SplitLines(MainTextBox.Text);
                selected.SideLines = String.IsNullOrEmpty(SideTextBox.Text) ? new List<string>() : SplitLines(SideTextBox.Text);
            }

            string currentType = (TypeComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "长生";
            var doc = new XDocument(new XElement("list",
                new XAttribute("type", currentType),
                _items.Select(item =>
                {
                    var itemEl = new XElement("item");

                    var mainLines = item.MainLines
                        .Where(l => !string.IsNullOrWhiteSpace(l) || item.MainLines.Count == 1)
                        .ToList();
                    if (mainLines.Count > 0)
                        itemEl.Add(new XElement("main",
                            mainLines.Select(l => new XElement("line", XElement.Parse($"<line>{l}</line>").Nodes()))));

                    var sideLines = item.SideLines
                        .Where(l => !string.IsNullOrWhiteSpace(l) || item.SideLines.Count == 1)
                        .ToList();
                    if (sideLines.Count > 0)
                        itemEl.Add(new XElement("side",
                            sideLines.Select(l => new XElement("line", XElement.Parse($"<line>{l}</line>").Nodes()))));

                    return itemEl;
                })));

            doc.Save(_filePath);
            MessageBox.Show("Saved successfully.", "Save");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file:\n{ex.Message}", "Error");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshListDisplay()
    {
        // Force the ListBox to refresh item strings
        var sel = ItemListBox.SelectedItem;
        ItemListBox.Items.Refresh();
        ItemListBox.SelectedItem = sel;
    }

    private void QuickPrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            MessageBox.Show("Please save or open a file first.", "Print & Test");
            return;
        }

        try
        {
            string selectedType = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            bool printBorder = PrintBorderCheckBox.IsChecked == true;
            bool printStamp = PrintRedStampCheckBox.IsChecked == true;
            bool printNames = PrintNamesCheckBox.IsChecked == true;

            var settings = PrintLayoutSettings.Load(".\\configure\\" + PrintLayoutSettings.DefaultConfigPath);
            var typeConfig = settings.GetConfig(selectedType);

            if (typeConfig == null)
            {
                MessageBox.Show($"Configuration not found for type: {selectedType}", "Error");
                return;
            }

            string xpsPath = TwoSectionPrint.GenerateXps(_filePath, printBorder, printStamp, printNames, typeConfig);
            if (!string.IsNullOrEmpty(xpsPath))
            {
                MessageBox.Show($"XPS file generated successfully:\n{xpsPath}", "Success");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during generation:\n{ex.Message}", "Search & Test Error");
        }
    }
}
