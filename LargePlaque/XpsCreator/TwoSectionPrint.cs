using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace XpsCreator;

public static class TwoSectionPrint
{
    public static void GenerateXps(string filePath, bool printBorder, bool printStamp, bool printNames, TypeLayoutConfig config)
    {
        try
        {
            var doc = XDocument.Load(filePath);
            string outputXpsPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(filePath) ?? "",
                System.IO.Path.GetFileNameWithoutExtension(filePath) + "_two_section.xps");

            using (var package = System.IO.Packaging.Package.Open(outputXpsPath, System.IO.FileMode.Create))
            {
                var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                var items = doc.Descendants("item");
                foreach (var item in items)
                {
                    // A3 Portrait size in pixels (1 inch = 96 pixels)
                    double a3Width = 11.69 * 96;
                    double a3Height = 16.54 * 96;

                    var fixedPage = new FixedPage();
                    fixedPage.Width = a3Width;
                    fixedPage.Height = a3Height;
                    var canvas = new Canvas();

                    // 1. Draw Main Section
                    var mainElement = item.Element("main");
                    if (mainElement != null)
                    {
                        var mainLines = printNames ? GetLinesFromElement(mainElement) : new System.Collections.Generic.List<string>();
                        PrintTextBox.Print(canvas, mainLines.ToArray(), printBorder, printStamp, printNames, config.MainTextBox, config.Stamp, config.MainMaxFontSize);
                    }

                    // 2. Draw Side Section
                    var sideElement = item.Element("side");
                    if (sideElement != null)
                    {
                        var sideLines = printNames ? GetLinesFromElement(sideElement) : new System.Collections.Generic.List<string>();
                        PrintTextBox.Print(canvas, sideLines.ToArray(), printBorder, mainElement == null && printStamp, printNames, config.SideTextBox, config.Stamp, config.SideMaxFontSize, 5);
                    }

                    fixedPage.Children.Add(canvas);
                    fixedPage.Measure(new Size(a3Width, a3Height));
                    fixedPage.Arrange(new Rect(new Point(), new Size(a3Width, a3Height)));
                    fixedPage.UpdateLayout();

                    var content = new PageContent();
                    ((System.Windows.Markup.IAddChild)content).AddChild(fixedPage);
                    fixedDoc.Pages.Add(content);
                }

                writer.Write(fixedDoc);
                xpsDoc.Close();
            }

            MessageBox.Show($"XPS file created:\n{outputXpsPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating XPS: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static System.Collections.Generic.List<string> GetLinesFromElement(XElement element)
    {
        var lines = new System.Collections.Generic.List<string>();
        var lineElements = element.Elements("line").ToList();
        if (lineElements.Any())
        {
            foreach (var le in lineElements)
            {
                lines.Add(GetText(le));
            }
        }
        else if (!string.IsNullOrWhiteSpace(element.Value))
        {
            lines.Add(GetText(element));
        }
        return lines;
    }

    private static string GetText(XElement element)
    {
        using (var reader = element.CreateReader())
        {
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }
    }
}
