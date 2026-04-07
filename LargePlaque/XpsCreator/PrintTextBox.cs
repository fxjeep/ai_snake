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

    public static void Print(Canvas canvas, string[] lines, bool printBorder, bool printStamp, bool printNames, ElementRect textRect, ImageElement? stamp, double maxAllowedFontSize, double columnGap = 20)
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

        // Process all lines as individual columns
        VerticalTextRenderer.RenderText(canvas, lines, textRect, maxAllowedFontSize, false, columnGap);
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

    // Replaced by VerticalTextRenderer
}
