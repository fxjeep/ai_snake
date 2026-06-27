using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace XpsCreator;

public static class PrintTextBox
{
    // Helper logic moved to VerticalTextRenderer

    public static void Print(Canvas canvas, string[] lines, bool printBorder, bool printStamp, bool printNames, ElementRect textRect, ImageElement? stamp, double maxAllowedFontSize, double columnGap = 20, bool wrapModeIfLatin = false)
    {
        if (printBorder)
        {
            DrawBorder(canvas, textRect);
        }

        if (printStamp && stamp != null && !string.IsNullOrEmpty(stamp.FileName))
        {
            DrawStamp(canvas, stamp);
        }

        if (lines == null || lines.Length == 0 || !printNames) return;

        if (wrapModeIfLatin && lines.Any(l => PlaqueTextPrintHelper.HasEnglishOrVietnamese(l)))
        {
            RenderWrapMode(canvas, lines, textRect, maxAllowedFontSize);
        }
        else
        {
            // Process all lines as individual columns
            VerticalTextRenderer.RenderText(canvas, lines, textRect, maxAllowedFontSize, false, columnGap);
        }
    }

    public static void DrawBorder(Canvas canvas, ElementRect rect)
    {
        var r = new Rectangle();
        r.Width = UnitConverter.ToPx(rect.Width);
        r.Height = UnitConverter.ToPx(rect.Height);
        r.Stroke = Brushes.Black;
        r.StrokeThickness = 1;
        Canvas.SetLeft(r, UnitConverter.ToPx(rect.Left));
        Canvas.SetTop(r, UnitConverter.ToPx(rect.Top));
        canvas.Children.Add(r);
    }

    public static void DrawStamp(Canvas canvas, ImageElement stamp)
    {
        var stampImage = new Image();
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        string stampPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\configure", stamp.FileName);
        if (System.IO.File.Exists(stampPath))
        {
            bitmap.UriSource = new Uri(stampPath);
            bitmap.EndInit();
            stampImage.Source = bitmap;
            double size = UnitConverter.ToPx(stamp.Width);
            stampImage.Width = size;
            stampImage.Height = size;
            Canvas.SetLeft(stampImage, UnitConverter.ToPx(stamp.Left));
            Canvas.SetTop(stampImage, UnitConverter.ToPx(stamp.Top));
            canvas.Children.Add(stampImage);
        }
    }

    private static void RenderWrapMode(Canvas canvas, string[] lines, ElementRect rect, double maxAllowedFontSize)
    {
        double boxLeftPx = UnitConverter.ToPx(rect.Left);
        double boxTopPx = UnitConverter.ToPx(rect.Top);
        double boxWidthPx = UnitConverter.ToPx(rect.Width);
        double boxHeightPx = UnitConverter.ToPx(rect.Height);

        string concatenatedText = string.Join(" ", lines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()));

        double minFont = 5;
        double maxFont = maxAllowedFontSize;
        double fontSize = minFont;

        for (int iter = 0; iter < 15; iter++)
        {
            double mid = (minFont + maxFont) / 2.0;
            var tempBlock = new TextBlock(new Run(concatenatedText))
            {
                FontFamily = new FontFamily("KaiTi"),
                FontSize = mid,
                TextWrapping = TextWrapping.Wrap,
                Width = boxHeightPx,
                TextAlignment = TextAlignment.Left
            };
            tempBlock.Measure(new Size(boxHeightPx, double.PositiveInfinity));

            double h_before = tempBlock.DesiredSize.Height;
            double w_before = tempBlock.DesiredSize.Width;

            if (h_before <= boxWidthPx && w_before <= boxHeightPx)
            {
                fontSize = mid;
                minFont = mid;
            }
            else
            {
                maxFont = mid;
            }
        }

        var tb = new TextBlock(new Run(concatenatedText))
        {
            FontFamily = new FontFamily("KaiTi"),
            FontSize = fontSize,
            TextWrapping = TextWrapping.Wrap,
            Width = boxHeightPx,
            TextAlignment = TextAlignment.Left
        };
        tb.Measure(new Size(boxHeightPx, double.PositiveInfinity));
        double preRotHeight = tb.DesiredSize.Height;

        tb.RenderTransform = new System.Windows.Media.RotateTransform(90);

        // Offset so the rotated block is centered horizontally and top-aligned vertically
        double left = boxLeftPx + (boxWidthPx + preRotHeight) / 2.0;
        double top = boxTopPx;

        Canvas.SetLeft(tb, left);
        Canvas.SetTop(tb, top);
        canvas.Children.Add(tb);
    }
}
