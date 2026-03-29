using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace XpsCreator;

public partial class PositionConfigure : UserControl
{
    private PrintLayoutSettings _settings = new PrintLayoutSettings();
    private string _configPath = "PrintConfigure.xml";
    private bool _isUpdating = false;

    public PositionConfigure()
    {
        InitializeComponent();
        this.Loaded += PositionConfigure_Loaded;
    }

    private void PositionConfigure_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = PrintLayoutSettings.Load(_configPath);
        if (_settings.TypeConfigs.Count == 0)
        {
            InitializeDefaultSettings();
        }
        TypeComboBox.SelectedIndex = 0;
        UpdateUIFromCurrentType();
    }

    private void InitializeDefaultSettings()
    {
        string[] types = { "长生", "冤亲", "往生", "祖先" };
        foreach (var type in types)
        {
            _settings.TypeConfigs.Add(new TypeLayoutConfig
            {
                TypeName = type,
                MainTextBox = new ElementRect(11.6, 16.0, 6.5, 10.5),
                SideTextBox = new ElementRect(8.1, 21.0, 2.2, 10.0),
                Stamp = new ImageElement("stamp.png", 11.8, 2.0, 6.0, 6.0),
                Background = new ImageElement("", 0, 0, 29.7, 42.0),
                FontSettings = new FontConfig { Large = 47, Medium = 37, Small = 30, LargeLinesThreshold = 7, MediumLinesThreshold = 10 }
            });
        }
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating) return;
        SaveCurrentTypeToSettings();
        UpdateUIFromCurrentType();
    }

    private void UpdateUIFromCurrentType()
    {
        _isUpdating = true;
        var currentType = GetCurrentTypeConfig();
        if (currentType != null)
        {
            FontSizeLargeTxt.Text = currentType.FontSettings.Large.ToString();
            FontSizeMediumTxt.Text = currentType.FontSettings.Medium.ToString();
            FontSizeSmallTxt.Text = currentType.FontSettings.Small.ToString();
            LargeLinesTxt.Text = currentType.FontSettings.LargeLinesThreshold.ToString();
            MediumLinesTxt.Text = currentType.FontSettings.MediumLinesThreshold.ToString();

            BgWidthTxt.Text = currentType.Background.Width.ToString();
            BgHeightTxt.Text = currentType.Background.Height.ToString();
            StampWidthTxt.Text = currentType.Stamp.Width.ToString();
            StampHeightTxt.Text = currentType.Stamp.Height.ToString();

            Designer.SetElementValues(
                ToRect(currentType.MainTextBox),
                ToRect(currentType.SideTextBox),
                ToRect(currentType.Stamp));

            Designer.UpdateBackground(currentType.Background.FileName, currentType.Background.Width, currentType.Background.Height);
            Designer.UpdateStamp(currentType.Stamp.FileName, currentType.Stamp.Width, currentType.Stamp.Height);
        }
        _isUpdating = false;
    }

    private void SaveCurrentTypeToSettings()
    {
        var currentType = GetCurrentTypeConfig();
        if (currentType != null)
        {
            currentType.FontSettings.Large = TryParse(FontSizeLargeTxt.Text, 47);
            currentType.FontSettings.Medium = TryParse(FontSizeMediumTxt.Text, 37);
            currentType.FontSettings.Small = TryParse(FontSizeSmallTxt.Text, 30);
            currentType.FontSettings.LargeLinesThreshold = (int)TryParse(LargeLinesTxt.Text, 7);
            currentType.FontSettings.MediumLinesThreshold = (int)TryParse(MediumLinesTxt.Text, 10);

            currentType.Background.Width = TryParse(BgWidthTxt.Text, 29.7);
            currentType.Background.Height = TryParse(BgHeightTxt.Text, 42.0);
            currentType.Stamp.Width = TryParse(StampWidthTxt.Text, 6.0);
            currentType.Stamp.Height = TryParse(StampHeightTxt.Text, 6.0);

            Designer.GetElementValues(out Rect main, out Rect side, out Rect stamp);
            currentType.MainTextBox = FromRect(main);
            currentType.SideTextBox = FromRect(side);
            currentType.Stamp.Left = UnitConverter.ToCm(stamp.Left);
            currentType.Stamp.Top = UnitConverter.ToCm(stamp.Top);
            currentType.Stamp.Width = UnitConverter.ToCm(stamp.Width);
            currentType.Stamp.Height = UnitConverter.ToCm(stamp.Height);
        }
    }

    private TypeLayoutConfig? GetCurrentTypeConfig()
    {
        if (TypeComboBox.SelectedItem is ComboBoxItem cbi)
        {
            string typeName = cbi.Content.ToString() ?? "";
            return _settings.TypeConfigs.FirstOrDefault(t => t.TypeName == typeName);
        }
        return null;
    }

    private void SelectBackground_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
        if (dialog.ShowDialog() == true)
        {
            var currentType = GetCurrentTypeConfig();
            if (currentType != null)
            {
                currentType.Background.FileName = dialog.FileName;
                Designer.UpdateBackground(dialog.FileName, TryParse(BgWidthTxt.Text, 29.7), TryParse(BgHeightTxt.Text, 42.0));
            }
        }
    }

    private void SelectStamp_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
        if (dialog.ShowDialog() == true)
        {
            var currentType = GetCurrentTypeConfig();
            if (currentType != null)
            {
                currentType.Stamp.FileName = dialog.FileName;
                Designer.UpdateStamp(dialog.FileName, TryParse(StampWidthTxt.Text, 6.0), TryParse(StampHeightTxt.Text, 6.0));
            }
        }
    }

    private void Bg_Changed(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        var currentType = GetCurrentTypeConfig();
        if (currentType != null)
        {
            Designer.UpdateBackground(currentType.Background.FileName, TryParse(BgWidthTxt.Text, 29.7), TryParse(BgHeightTxt.Text, 42.0));
        }
    }

    private void Stamp_Changed(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        var currentType = GetCurrentTypeConfig();
        if (currentType != null)
        {
            Designer.UpdateStamp(currentType.Stamp.FileName, TryParse(StampWidthTxt.Text, 6.0), TryParse(StampHeightTxt.Text, 6.0));
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrentTypeToSettings();
        PrintLayoutSettings.Save(_configPath, _settings);
        MessageBox.Show("Configuration saved successfully!");
    }

    private double TryParse(string text, double defaultValue)
    {
        if (double.TryParse(text, out double val)) return val;
        return defaultValue;
    }

    private Rect ToRect(ElementRect er) => new Rect(UnitConverter.ToPx(er.Left), UnitConverter.ToPx(er.Top), UnitConverter.ToPx(er.Width), UnitConverter.ToPx(er.Height));
    private ElementRect FromRect(Rect r) => new ElementRect(UnitConverter.ToCm(r.Left), UnitConverter.ToCm(r.Top), UnitConverter.ToCm(r.Width), UnitConverter.ToCm(r.Height));
}
