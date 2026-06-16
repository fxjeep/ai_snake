using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XpsCreator;

public enum PrintMode
{
    LineMode,
    WrapMode
}

public static class PropertyXPSPrinter
{
    public static string GenerateXps(string dataFilePath, PrintLayoutSettings settings)
    {
        string outputXpsPath = "";
        const int SideFontSize = 12;
        try
        {
            var lines = File.ReadAllLines(dataFilePath);
            outputXpsPath = Path.Combine(
                Path.GetDirectoryName(dataFilePath) ?? "",
                Path.GetFileNameWithoutExtension(dataFilePath) + "_property.xps");

            PropertyXpsHelper.GenerateXpsDocument(outputXpsPath, lines, settings, SideFontSize);

            return outputXpsPath;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating XPS: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
        }
    }
}
