using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PlaqueData.Models;

namespace XpsCreator;

public static class WeeklyXpsPrinter
{
    private static bool IsHorizontalSegmentChar(char c)
    {
        if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c))
            return true;

        // Vietnamese characters (Latin characters with diacritics)
        if (char.IsLetter(c))
        {
            if ((c >= '\u00C0' && c <= '\u017F') || // Latin-1 Supplement & Extended-A
                (c >= '\u0180' && c <= '\u024F') || // Latin Extended-B
                (c >= '\u1E00' && c <= '\u1EFF'))   // Latin Extended Additional
                return true;
        }

        return false;
    }

    private static System.Collections.Generic.List<string> ParseNameIntoLines(string name)
    {
        var lines = new System.Collections.Generic.List<string>();
        if (string.IsNullOrWhiteSpace(name)) return lines;

        string currentWord = "";
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (IsHorizontalSegmentChar(c))
            {
                currentWord += c;
            }
            else
            {
                if (!string.IsNullOrEmpty(currentWord))
                {
                    lines.Add(currentWord);
                    currentWord = "";
                }
                if (!char.IsWhiteSpace(c))
                {
                    lines.Add(c.ToString());
                }
            }
        }
        if (!string.IsNullOrEmpty(currentWord))
        {
            lines.Add(currentWord);
        }
        return lines;
    }

    public static string GenerateWeeklyLiveXps(string outputXpsPath, System.Collections.Generic.List<Live> liveRecords, WeeklyPrintTypeConfig config)
    {
        try
        {
            // Ensure directory exists
            string? dir = Path.GetDirectoryName(outputXpsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Dimensions in Pixels
            double a4WidthPx = UnitConverter.ToPx(21.0);
            double a4HeightPx = UnitConverter.ToPx(29.7);

            double topMarginPx = UnitConverter.ToPx(2.0);
            double leftMarginPx = UnitConverter.ToPx(0.5);

            double imageWidthPx = UnitConverter.ToPx(2.85);
            double imageHeightPx = UnitConverter.ToPx(6.8);

            int cols = config.Column > 0 ? config.Column : 7;
            int rows = config.Row > 0 ? config.Row : 4;
            int itemsPerPage = cols * rows;

            // Load background image
            string bgImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", config.Background.FileName);
            BitmapImage? bgImage = null;
            if (File.Exists(bgImagePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(bgImagePath);
                bitmap.EndInit();
                bgImage = bitmap;
            }

            using (var package = System.IO.Packaging.Package.Open(outputXpsPath, FileMode.Create))
            {
                var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                for (int i = 0; i < liveRecords.Count; i += itemsPerPage)
                {
                    var pageRecords = liveRecords.Skip(i).Take(itemsPerPage).ToList();

                    var fixedPage = new FixedPage
                    {
                        Width = a4WidthPx,
                        Height = a4HeightPx
                    };
                    var pageCanvas = new Canvas();

                    for (int j = 0; j < itemsPerPage; j++)
                    {
                        int col = j % cols;
                        int row = j / cols;

                        double cellLeft = leftMarginPx + col * imageWidthPx;
                        double cellTop = topMarginPx + row * imageHeightPx;

                        var itemCanvas = new Canvas
                        {
                            Width = imageWidthPx,
                            Height = imageHeightPx
                        };
                        Canvas.SetLeft(itemCanvas, cellLeft);
                        Canvas.SetTop(itemCanvas, cellTop);

                        // 1. Draw Background Image
                        if (bgImage != null)
                        {
                            var img = new Image
                            {
                                Source = bgImage,
                                Width = imageWidthPx,
                                Height = imageHeightPx,
                                Stretch = Stretch.Fill
                            };
                            itemCanvas.Children.Add(img);
                        }

                        // 2. Draw Black Border
                        var borderRect = new System.Windows.Shapes.Rectangle
                        {
                            Width = imageWidthPx,
                            Height = imageHeightPx,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        itemCanvas.Children.Add(borderRect);

                        // 3. Draw Name Text (only if within records count)
                        if (j < pageRecords.Count)
                        {
                            var record = pageRecords[j];
                            if (!string.IsNullOrWhiteSpace(record.Name))
                            {
                                double textBoxLeft = UnitConverter.ToPx(config.MainTextBox.Left);
                                double textBoxTop = UnitConverter.ToPx(config.MainTextBox.Top);
                                double textBoxWidth = UnitConverter.ToPx(config.MainTextBox.Width);
                                double textBoxHeight = UnitConverter.ToPx(config.MainTextBox.Height);

                                var parsedLines = ParseNameIntoLines(record.Name);
                                if (parsedLines.Any())
                                {
                                    var textBlocks = new System.Collections.Generic.List<TextBlock>();
                                    double totalHeight = 0;

                                    foreach (var line in parsedLines)
                                    {
                                        var run = new Run(line);
                                        var tb = new TextBlock(run)
                                        {
                                            FontFamily = new FontFamily("KaiTi"),
                                            FontSize = config.FontSize,
                                        };
                                        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                                        textBlocks.Add(tb);
                                        totalHeight += tb.DesiredSize.Height;
                                    }

                                    // Center all lines vertically
                                    double startY = 0;

                                    double currentY = textBoxTop + startY;
                                    foreach (var tb in textBlocks)
                                    {
                                        // Center each line horizontally
                                        double lineX = textBoxLeft + (textBoxWidth - tb.DesiredSize.Width) / 2;
                                        Canvas.SetLeft(tb, lineX);
                                        Canvas.SetTop(tb, currentY);
                                        itemCanvas.Children.Add(tb);

                                        currentY += tb.DesiredSize.Height;
                                    }
                                }
                            }
                        }

                        pageCanvas.Children.Add(itemCanvas);
                    }

                    fixedPage.Children.Add(pageCanvas);
                    fixedPage.Measure(new Size(a4WidthPx, a4HeightPx));
                    fixedPage.Arrange(new Rect(new Point(), new Size(a4WidthPx, a4HeightPx)));
                    fixedPage.UpdateLayout();

                    var content = new PageContent();
                    ((System.Windows.Markup.IAddChild)content).AddChild(fixedPage);
                    fixedDoc.Pages.Add(content);
                }

                writer.Write(fixedDoc);
                xpsDoc.Close();
            }

            return outputXpsPath;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating XPS: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
        }
    }
}
