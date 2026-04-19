using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace XpsCreator
{
    public partial class PropertyPrinter : UserControl
    {
        private PrintLayoutSettings _settings;
        private PropertyPrintConfig _config => _settings?.PropertyPrintConfig;
        private string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", "PrintConfigure.xml");

        public PropertyPrinter()
        {
            InitializeComponent();
            this.Loaded += PropertyPrinter_Loaded;
        }

        private void PropertyPrinter_Loaded(object sender, RoutedEventArgs e)
        {
            var configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            _settings = PrintLayoutSettings.Load(_configPath);
            if (_settings.PropertyPrintConfig == null)
            {
                _settings.PropertyPrintConfig = new PropertyPrintConfig();
            }
            ApplyConfigToUI();

            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", "property.jpg");
            if (File.Exists(imagePath))
            {
                BgImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath));
            }
        }

        private void ApplyConfigToUI()
        {
            Canvas.SetLeft(MainBox, UnitConverter.ToPx(_config.Main.Left));
            Canvas.SetTop(MainBox, UnitConverter.ToPx(_config.Main.Top));
            MainBox.Width = UnitConverter.ToPx(_config.Main.Width);
            MainBox.Height = UnitConverter.ToPx(_config.Main.Height);

            Canvas.SetLeft(SideBox, UnitConverter.ToPx(_config.Side.Left));
            Canvas.SetTop(SideBox, UnitConverter.ToPx(_config.Side.Top));
            SideBox.Width = UnitConverter.ToPx(_config.Side.Width);
            SideBox.Height = UnitConverter.ToPx(_config.Side.Height);

            L1MaxCharTextBox.Text = _config.MainFontLevel1MaxChars.ToString();
            L1FontSizeTextBox.Text = _config.MainFontLevel1Size.ToString();
            L2MaxCharTextBox.Text = _config.MainFontLevel2MaxChars.ToString();
            L2FontSizeTextBox.Text = _config.MainFontLevel2Size.ToString();
            SmallFontSizeTextBox.Text = _config.MainFontSmallSize.ToString();
        }

        private void SaveConfig()
        {
            if (_settings == null || _config == null) return;

            _config.Main.Left = UnitConverter.ToCm(Canvas.GetLeft(MainBox));
            _config.Main.Top = UnitConverter.ToCm(Canvas.GetTop(MainBox));
            _config.Main.Width = UnitConverter.ToCm(MainBox.Width);
            _config.Main.Height = UnitConverter.ToCm(MainBox.Height);

            _config.Side.Left = UnitConverter.ToCm(Canvas.GetLeft(SideBox));
            _config.Side.Top = UnitConverter.ToCm(Canvas.GetTop(SideBox));
            _config.Side.Width = UnitConverter.ToCm(SideBox.Width);
            _config.Side.Height = UnitConverter.ToCm(SideBox.Height);

            if (int.TryParse(L1MaxCharTextBox.Text, out int l1Max)) _config.MainFontLevel1MaxChars = l1Max;
            if (int.TryParse(L1FontSizeTextBox.Text, out int l1Size)) _config.MainFontLevel1Size = l1Size;
            if (int.TryParse(L2MaxCharTextBox.Text, out int l2Max)) _config.MainFontLevel2MaxChars = l2Max;
            if (int.TryParse(L2FontSizeTextBox.Text, out int l2Size)) _config.MainFontLevel2Size = l2Size;
            if (int.TryParse(SmallFontSizeTextBox.Text, out int smallSize)) _config.MainFontSmallSize = smallSize;

            PrintLayoutSettings.Save(_configPath, _settings);
        }

        private void Box_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Parent is Grid box)
            {
                double left = Canvas.GetLeft(box) + e.HorizontalChange;
                double top = Canvas.GetTop(box) + e.VerticalChange;

                if (left < 0) left = 0;
                if (top < 0) top = 0;
                if (left + box.Width > PrinterCanvas.Width) left = PrinterCanvas.Width - box.Width;
                if (top + box.Height > PrinterCanvas.Height) top = PrinterCanvas.Height - box.Height;

                Canvas.SetLeft(box, left);
                Canvas.SetTop(box, top);
            }
        }

        private void BoxResize_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Parent is Grid box)
            {
                double newWidth = box.Width + e.HorizontalChange;
                double newHeight = box.Height + e.VerticalChange;

                if (newWidth > 10) box.Width = newWidth;
                if (newHeight > 10) box.Height = newHeight;
            }
        }

        private void Box_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SaveConfig();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathTextBox.Text) || !File.Exists(FilePathTextBox.Text))
            {
                MessageBox.Show("Please select a valid tab-delimited text file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveConfig();

            string xpsPath = PropertyXPSPrinter.GenerateXps(FilePathTextBox.Text, _settings);

            if (!string.IsNullOrEmpty(xpsPath) && File.Exists(xpsPath))
            {
                MessageBox.Show($"XPS document generated successfully at:\n{xpsPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(xpsPath) { UseShellExecute = true });
            }
        }
    }
}
