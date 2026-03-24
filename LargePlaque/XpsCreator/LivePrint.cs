using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace XpsCreator;

public static class LivePrint
{
    public static void GenerateXps(string filePath, bool printBorder, bool printStamp, MainBoxConfigure config)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Load(filePath);
            string outputXpsPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(filePath) ?? "",
                System.IO.Path.GetFileNameWithoutExtension(filePath) + "_live.xps");

            using (var package = System.IO.Packaging.Package.Open(outputXpsPath, System.IO.FileMode.Create))
            {
                var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                var items = doc.Descendants("item");
                foreach (var item in items)
                {
                    var linesList = new System.Collections.Generic.List<string>();
                    var main = item.Element("main");
                    if (main != null)
                    {
                        AddLinesFromElement(main, linesList);
                    }

                    if (linesList.Count > 0)
                    {
                        // A3 Portrait size in pixels (1 inch = 96 pixels)
                        double a3Width = 11.69 * 96;
                        double a3Height = 16.54 * 96;

                        var fixedPage = new FixedPage();
                        fixedPage.Width = a3Width;
                        fixedPage.Height = a3Height;
                        var canvas = new Canvas();

                        PrintTextBox.Print(canvas, linesList.ToArray(), printBorder, printStamp, config.TextPrintBox, config.StampPosition, PrintConfigure.Main);

                        fixedPage.Children.Add(canvas);
                        fixedPage.Measure(new Size(a3Width, a3Height));
                        fixedPage.Arrange(new Rect(new Point(), new Size(a3Width, a3Height)));
                        fixedPage.UpdateLayout();

                        var content = new PageContent();
                        ((System.Windows.Markup.IAddChild)content).AddChild(fixedPage);
                        fixedDoc.Pages.Add(content);
                    }
                }

                writer.Write(fixedDoc);
                xpsDoc.Close();
            }

            MessageBox.Show($"XPS file created:\n{outputXpsPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error generating XPS: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void AddLinesFromElement(System.Xml.Linq.XElement element, System.Collections.Generic.List<string> linesList)
    {
        var lineElements = element.Elements("line").ToList();
        if (lineElements.Any())
        {
            foreach (var le in lineElements)
            {
                string val = GetText(le);
                linesList.Add(val);
            }
        }
    }

    private static string GetText(System.Xml.Linq.XElement element)
    {
        using (var reader = element.CreateReader())
        {
            reader.MoveToContent();
            string innerXml = reader.ReadInnerXml();
            return innerXml;
        }
    }
}
