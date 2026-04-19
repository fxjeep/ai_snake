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

            using (var package = System.IO.Packaging.Package.Open(outputXpsPath, FileMode.Create))
            {
                var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                var config = settings.PropertyPrintConfig;

                double a4Width = UnitConverter.ToPx(21.0);
                double a4Height = UnitConverter.ToPx(29.7);

                double marginLeft = UnitConverter.ToPx(config.MarginLeftCm);
                double marginTop = UnitConverter.ToPx(config.MarginTopCm);

                double itemWidth = UnitConverter.ToPx(4.0);
                double itemHeight = UnitConverter.ToPx(13.5);

                int itemsPerPage = config.Columns * config.Rows;

                string bgImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configure", "property.jpg");
                BitmapImage bgImage = null;
                if (File.Exists(bgImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(bgImagePath);
                    bitmap.EndInit();
                    bgImage = bitmap;
                }

                for (int i = 0; i < lines.Length; i += itemsPerPage)
                {
                    var pageLines = lines.Skip(i).Take(itemsPerPage).ToList();

                    var fixedPage = new FixedPage();
                    fixedPage.Width = a4Width;
                    fixedPage.Height = a4Height;
                    var pageCanvas = new Canvas();

                    for (int j = 0; j < itemsPerPage; j++)
                    {
                        string sideText = "";
                        string mainText = "";

                        if (j < pageLines.Count && !string.IsNullOrWhiteSpace(pageLines[j]))
                        {
                            var fields = pageLines[j].Split('\t');
                            sideText = fields.Length > 0 ? fields[0].Trim() : "";
                            mainText = fields.Length > 1 ? fields[1].Trim() : "";
                        }

                        int col = j % config.Columns;
                        int row = j / config.Columns;

                        double x = marginLeft + col * itemWidth;
                        double y = marginTop + row * itemHeight;

                        var itemCanvas = new Canvas();
                        Canvas.SetLeft(itemCanvas, x);
                        Canvas.SetTop(itemCanvas, y);
                        itemCanvas.Width = itemWidth;
                        itemCanvas.Height = itemHeight;

                        if (bgImage != null)
                        {
                            var img = new Image
                            {
                                Source = bgImage,
                                Width = itemWidth,
                                Height = itemHeight,
                                Stretch = Stretch.Fill
                            };
                            itemCanvas.Children.Add(img);

                            var borderRect = new System.Windows.Shapes.Rectangle
                            {
                                Width = itemWidth,
                                Height = itemHeight,
                                Stroke = Brushes.Black,
                                StrokeThickness = 1
                            };
                            itemCanvas.Children.Add(borderRect);
                        }

                        // Side Box
                        bool sideHasAlphaNumeric = Regex.IsMatch(sideText, "[a-zA-Z0-9]");
                        PrintMode sideMode = sideHasAlphaNumeric ? PrintMode.WrapMode : PrintMode.LineMode;
                        if (!string.IsNullOrWhiteSpace(sideText))
                        {
                            if (sideMode == PrintMode.WrapMode)
                            {
                                RenderWrapMode(itemCanvas, sideText, config.Side, SideFontSize, false);
                            }
                            else
                            {
                                // LineMode uses VerticalTextRenderer. It expects ElementRect in cm, because it calls UnitConverter.ToPx(rect.Width) internally.
                                VerticalTextRenderer.RenderText(itemCanvas, new[] { sideText }, config.Side, SideFontSize, true, 2);
                            }
                        }

                        // Main Box -> Always WrapMode
                        if (!string.IsNullOrWhiteSpace(mainText))
                        {
                            int mainFontSize = config.MainFontSmallSize;
                            if (mainText.Length <= config.MainFontLevel1MaxChars)
                            {
                                mainFontSize = config.MainFontLevel1Size;
                            }
                            else if (mainText.Length <= config.MainFontLevel2MaxChars)
                            {
                                mainFontSize = config.MainFontLevel2Size;
                            }

                            RenderWrapMode(itemCanvas, mainText, config.Main, mainFontSize, true);
                        }

                        pageCanvas.Children.Add(itemCanvas);
                    }

                    fixedPage.Children.Add(pageCanvas);
                    fixedPage.Measure(new Size(a4Width, a4Height));
                    fixedPage.Arrange(new Rect(new Point(), new Size(a4Width, a4Height)));
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

    private static void RenderWrapMode(Canvas canvas, string text, ElementRect rect, double fontSize, bool centerHorizontally = false)
    {
        var tb = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = fontSize,
            FontFamily = new FontFamily("KaiTi"),
            Width = UnitConverter.ToPx(rect.Height), // Swap dimensions since it will be rotated
            TextAlignment = TextAlignment.Center
        };

        FrameworkElement container = tb;

        if (centerHorizontally)
        {
            tb.VerticalAlignment = VerticalAlignment.Center;
            container = new Border
            {
                Width = UnitConverter.ToPx(rect.Height),
                Height = UnitConverter.ToPx(rect.Width),
                Child = tb
            };
        }
        else
        {
            tb.Height = UnitConverter.ToPx(rect.Width); // Expand to full width so Top alignment pushes to the Right edge
        }

        container.LayoutTransform = new RotateTransform(90);

        Canvas.SetLeft(container, UnitConverter.ToPx(rect.Left));
        Canvas.SetTop(container, UnitConverter.ToPx(rect.Top));
        canvas.Children.Add(container);
    }
}
