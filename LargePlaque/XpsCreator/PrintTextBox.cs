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

    public static void Print(Canvas canvas, string[] lines, bool printBorder, bool printStamp, bool printNames, TextPrintBoxConfigure textConfig, StampPositionConfig? stampConfig, double maxAllowedFontSize)
    {
        if (printBorder)
        {
            DrawBorder(canvas, textConfig);
        }

        if (printStamp && stampConfig != null)
        {
            DrawStamp(canvas, stampConfig);
        }

        if (lines == null || lines.Length == 0 || !printNames) return;

        var lineSegments = lines.Select(l => VerticalTextRenderer.ParseSegments(l)).ToList();

        // Process all lines as individual columns
        VerticalTextRenderer.RenderText(canvas, lines, textConfig, maxAllowedFontSize, false);
    }

    public static void DrawBorder(Canvas canvas, TextPrintBoxConfigure config)
    {
        var rect = new Rectangle();
        rect.Width = config.Width;
        rect.Height = config.Height;
        rect.Stroke = Brushes.Black;
        rect.StrokeThickness = 1;
        Canvas.SetLeft(rect, config.Left);
        Canvas.SetTop(rect, config.Top);
        canvas.Children.Add(rect);
    }

    public static void DrawStamp(Canvas canvas, StampPositionConfig config)
    {
        var stampImage = new Image();
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        string stampPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stamp.png");
        if (System.IO.File.Exists(stampPath))
        {
            bitmap.UriSource = new Uri(stampPath);
            bitmap.EndInit();
            stampImage.Source = bitmap;
            stampImage.Width = config.Size;
            stampImage.Height = config.Size;
            Canvas.SetLeft(stampImage, config.Left);
            Canvas.SetTop(stampImage, config.Top);
            canvas.Children.Add(stampImage);
        }
    }

    // Replaced by VerticalTextRenderer
}
