using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PlaqueData.Models;
using XpsCreator.ViewModels;

namespace XpsCreator;

public static class WeeklyXpsPrinter
{


    

    /// <summary>
    /// Generates a paged XPS file for any weekly record type (<typeparamref name="T"/>).
    /// The caller supplies a <paramref name="renderItem"/> delegate that is responsible
    /// for drawing a single record (or an empty placeholder when the value is <c>null</c>)
    /// onto the provided <see cref="Canvas"/>.
    /// </summary>
    /// <typeparam name="T">The record type: <see cref="Live"/>, <see cref="PlaqueData.Models.Dead"/>, Ancestor, etc.</typeparam>
    /// <param name="outputXpsPath">Destination XPS file path.</param>
    /// <param name="records">The list of records to print.</param>
    /// <param name="config">Layout/style configuration for this print type.</param>
    /// <param name="renderItem">
    /// Callback invoked once per cell: <c>(canvas, record)</c>.
    /// <paramref name="renderItem"/> receives <c>null</c> for empty placeholder cells.
    /// </param>
    public static string GenerateWeeklyXps<T>(string outputXpsPath, System.Collections.Generic.List<T> records, WeeklyPrintTypeConfig config, Action<Canvas, T?> renderItem)
        where T : class
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

            using (var package = System.IO.Packaging.Package.Open(outputXpsPath, FileMode.Create))
            {
                var xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                var fixedDoc = new FixedDocument();

                for (int i = 0; i < records.Count; i += itemsPerPage)
                {
                    var pageRecords = records.Skip(i).Take(itemsPerPage).ToList();

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

                        renderItem(itemCanvas, j < pageRecords.Count ? pageRecords[j] : null);

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
    /// <summary>
    /// Renders a single plaque cell: background image, border, and name text.
    /// Pass <c>null</c> for <paramref name="record"/> to draw an empty placeholder cell.
    /// </summary>
    public static void PrintOnePlaque(Canvas itemCanvas, double width, double height,
        BitmapImage? bgImage, Live? record, WeeklyPrintTypeConfig config)
    {
        // 1. Draw Background Image
        if (bgImage != null)
        {
            var img = new Image
            {
                Source = bgImage,
                Width = width,
                Height = height,
                Stretch = Stretch.Fill
            };
            itemCanvas.Children.Add(img);
        }

        // 2. Draw Black Border
        var borderRect = new System.Windows.Shapes.Rectangle
        {
            Width = width,
            Height = height,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        itemCanvas.Children.Add(borderRect);

        // 3. Draw Name Text
        if (record == null) return;

        // WrapMode when Name contains English/Vietnamese characters (horizontal text, rotated CW 90°)
        // LineMode when Name is pure Chinese (one character per line)
        if (PlaqueTextPrintHelper.HasEnglishOrVietnamese(record.Name) && config.TypeName == WeeklyPrintTypes.Yuanqing)
            PlaqueTextPrintHelper.PrintNameInWrapMode(itemCanvas, record.Name, config.MainTextBox, config.FontSize);
        else
            PlaqueTextPrintHelper.PrintNameToRecord(itemCanvas, record.Name, config.MainTextBox, config.FontSize);
    }

    /// <summary>
    /// Renders a single Dead plaque cell: background, border, DeadName (main),
    /// and Relation+LiveName (side).
    /// </summary>
    public static void PrintOneDeadPlaque(Canvas itemCanvas, double width, double height,
        BitmapImage? bgImage, PlaqueData.Models.Dead? record, WeeklyPrintTypeConfig config)
    {
        // 1. Background
        if (bgImage != null)
        {
            var img = new Image { Source = bgImage, Width = width, Height = height, Stretch = Stretch.Fill };
            itemCanvas.Children.Add(img);
        }

        // 2. Border
        var borderRect = new System.Windows.Shapes.Rectangle
        {
            Width = width, Height = height,
            Stroke = Brushes.Black, StrokeThickness = 1
        };
        itemCanvas.Children.Add(borderRect);

        if (record == null) return;

        // 3. MainTextBox – DeadName in LineMode (one char per line, centered)
        PlaqueTextPrintHelper.PrintNameToRecord(itemCanvas, record.DeadName, config.MainTextBox, config.FontSize);

        // 4. SideTextBox – Relation+LiveName
        string sideText = record.Relation + record.LiveName;
        if (!string.IsNullOrWhiteSpace(sideText))
            PrintDeadSideText(itemCanvas, sideText, config.SideTextBox, config.FontSize);
    }

    /// <summary>
    /// Renders a single Ancestor plaque cell: background, border, Surname (main),
    /// and Name as the side text (LiveName equivalent).
    /// </summary>
    public static void PrintOneAncestorPlaque(Canvas itemCanvas, double width, double height,
        BitmapImage? bgImage, PlaqueData.Models.Ancestor? record, WeeklyPrintTypeConfig config)
    {
        // 1. Background
        if (bgImage != null)
        {
            var img = new Image { Source = bgImage, Width = width, Height = height, Stretch = Stretch.Fill };
            itemCanvas.Children.Add(img);
        }

        // 2. Border
        var borderRect = new System.Windows.Shapes.Rectangle
        {
            Width = width, Height = height,
            Stroke = Brushes.Black, StrokeThickness = 1
        };
        itemCanvas.Children.Add(borderRect);

        if (record == null) return;

        // 3. MainTextBox – Surname in LineMode (one char per line, centered)
        PlaqueTextPrintHelper.PrintNameToRecord(itemCanvas, record.Surname, config.MainTextBox, config.FontSize);

        // 4. SideTextBox – Name (LiveName equivalent)
        if (!string.IsNullOrWhiteSpace(record.Name))
            PrintDeadSideText(itemCanvas, record.Name, config.SideTextBox, config.FontSize);
    }

    /// <summary>
    /// Prints the side text (Relation+LiveName) into the SideTextBox.<br/>
    /// • If the text contains any English or Vietnamese characters → <b>WrapMode</b>:
    ///   the text is laid out horizontally, wrapped to fit the box width, then the
    ///   entire block is rotated clockwise 90° so it reads top-to-bottom in the
    ///   narrow side column.<br/>
    /// • Otherwise (pure Chinese) → <b>LineMode</b>: each character is placed on
    ///   its own line, top-to-bottom, centred horizontally in the side box.
    /// </summary>
    private static void PrintDeadSideText(Canvas canvas, string text, ElementRect sideBox, double fontSize)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        double boxLeftPx   = UnitConverter.ToPx(sideBox.Left);
        double boxTopPx    = UnitConverter.ToPx(sideBox.Top);
        double boxWidthPx  = UnitConverter.ToPx(sideBox.Width);
        double boxHeightPx = UnitConverter.ToPx(sideBox.Height);

        bool hasNonChinese = PlaqueTextPrintHelper.HasEnglishOrVietnamese(text);

        if (hasNonChinese)
        {
            // ── WrapMode ────────────────────────────────────────────────────
            // Build a TextBlock that wraps horizontally within boxHeightPx
            // (before rotation the "width" of the TB is the box height, because
            // after a CW 90° rotation that dimension becomes the vertical span).
            var tb = new TextBlock
            {
                Text        = text,
                FontFamily  = new FontFamily("KaiTi"),
                FontSize    = fontSize,
                TextWrapping = TextWrapping.Wrap,
                Width       = boxHeightPx,   // will become height after rotation
                TextAlignment = TextAlignment.Left
            };

            // Rotate CW 90°: the top-left corner of the (rotated) element should
            // sit at (boxLeftPx, boxTopPx + boxHeightPx).
            var rotateTransform = new System.Windows.Media.RotateTransform(90);
            tb.RenderTransform = rotateTransform;
            // After CW rotation the element's top-left maps to its new origin.
            // Offset so the rotated block sits inside the side box:
            //   X = boxLeft  (left edge of side box)
            //   Y = boxTop + boxHeight (bottom of side box in canvas coords,
            //       which becomes the left edge after CW rotation)
            Canvas.SetLeft(tb, boxLeftPx+11);
            Canvas.SetTop(tb, boxTopPx);

            canvas.Children.Add(tb);
        }
        else
        {
            // ── LineMode (pure Chinese) ──────────────────────────────────────
            // Each character gets its own TextBlock, stacked top-to-bottom,
            // centred horizontally within the side box.
            double currentY = boxTopPx;
            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c)) continue;

                var run = new Run(c.ToString());
                var tb  = new TextBlock(run)
                {
                    FontFamily = new FontFamily("KaiTi"),
                    FontSize   = fontSize,
                };
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double charX = boxLeftPx + (boxWidthPx - tb.DesiredSize.Width) / 2.0;
                Canvas.SetLeft(tb, charX);
                Canvas.SetTop(tb, currentY);
                canvas.Children.Add(tb);

                currentY += tb.DesiredSize.Height;
                if (currentY > boxTopPx + boxHeightPx) break; // guard against overflow
            }
        }
    }
}
