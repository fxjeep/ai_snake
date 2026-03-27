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
    private class TextSegment
    {
        public string Text { get; set; } = "";
        public bool IsSmall { get; set; }
    }

    private static System.Collections.Generic.List<TextSegment> ParseSegments(string text)
    {
        var segments = new System.Collections.Generic.List<TextSegment>();
        if (string.IsNullOrEmpty(text)) return segments;

        try
        {
            // Wrap in a root element to make it valid XML
            string xml = $"<root>{text}</root>";
            var root = System.Xml.Linq.XElement.Parse(xml);
            foreach (var node in root.Nodes())
            {
                if (node is System.Xml.Linq.XText textNode)
                {
                    segments.Add(new TextSegment { Text = textNode.Value, IsSmall = false });
                }
                else if (node is System.Xml.Linq.XElement element && element.Name == "s")
                {
                    segments.Add(new TextSegment { Text = element.Value, IsSmall = true });
                }
            }
        }
        catch (Exception)
        {
            // Fallback to plain text if parsing fails
            segments.Add(new TextSegment { Text = text, IsSmall = false });
        }

        return segments;
    }

    public static void Print(Canvas canvas, string[] lines, bool printBorder, bool printStamp, bool printNames, TextPrintBoxConfigure textConfig, StampPositionConfig? stampConfig, FontSize fontSizeConfig)
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

        var lineSegments = lines.Select(l => ParseSegments(l)).ToList();

        int maxUnits = lineSegments.Max(ls => ls.Sum(s => CountLogicalLines(s.Text)));
        double fontSize;
        if (maxUnits <= fontSizeConfig.LargeLines) fontSize = fontSizeConfig.Large;
        else if (maxUnits < fontSizeConfig.MediumLines) fontSize = fontSizeConfig.Medium;
        else fontSize = fontSizeConfig.Small;
        // Process all lines as individual columns
        DrawColumns(canvas, lineSegments, fontSize, textConfig);
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

    private static void DrawColumns(Canvas canvas, System.Collections.Generic.List<System.Collections.Generic.List<TextSegment>> lineSegments, double fontSize, TextPrintBoxConfigure config)
    {
        double[] colWidths = new double[lineSegments.Count];
        for (int i = 0; i < lineSegments.Count; i++)
        {
            colWidths[i] = MeasureColumnWidth(lineSegments[i], fontSize, config);
        }

        double gap = config.ColumnGap; // Use configured gap
        double totalWidthWithGaps = colWidths.Sum() + gap * (lineSegments.Count - 1);

        // Calculate starting center for the rightmost column
        double startRight;
        if (config.ColumnAlign == EnumColumnAlign.Right)
        {
            startRight = config.Left + config.Width;
        }
        else
        {
            startRight = config.Left + (config.Width + totalWidthWithGaps) / 2;
        }
        double currentXCenter = startRight;

        for (int i = 0; i < lineSegments.Count; i++)
        {
            if (i == 0)
            {
                currentXCenter -= colWidths[i] / 2;
            }
            else
            {
                currentXCenter -= (colWidths[i - 1] / 2 + gap + colWidths[i] / 2);
            }

            DrawVerticalString(canvas, lineSegments[i], currentXCenter, colWidths[i], fontSize, config.Top, config);
        }
    }

    private static void DrawVerticalString(Canvas canvas, System.Collections.Generic.List<TextSegment> segments, double x, double colWidth, double fontSize, double startY, TextPrintBoxConfigure config)
    {
        double currentY = startY;

        foreach (var segment in segments)
        {
            double segmentFontSize = segment.IsSmall ? fontSize - 5 : fontSize;
            string text = segment.Text;
            string currentEnglishWord = "";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isAsciiLetterOrDigit = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c);

                if (isAsciiLetterOrDigit)
                {
                    currentEnglishWord += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentEnglishWord))
                    {
                        currentY = DrawTextElement(canvas, currentEnglishWord, x, colWidth, segmentFontSize, currentY, config, segment.IsSmall);
                        currentEnglishWord = "";
                    }

                    currentY = DrawTextElement(canvas, c.ToString(), x, colWidth, segmentFontSize, currentY, config, segment.IsSmall);
                }
            }

            if (!string.IsNullOrEmpty(currentEnglishWord))
            {
                currentY = DrawTextElement(canvas, currentEnglishWord, x, colWidth, segmentFontSize, currentY, config, segment.IsSmall);
            }
        }
    }

    private static double DrawTextElement(Canvas canvas, string text, double x, double colWidth, double fontSize, double y, TextPrintBoxConfigure config, bool isSmall)
    {
        var run = new Run(text);
        var textBlock = new TextBlock(run)
        {
            FontFamily = new FontFamily("KaiTi"),
            FontSize = fontSize,
        };

        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        if (isSmall)
        {
            // Right align in the column
            Canvas.SetLeft(textBlock, x + colWidth / 2 - textBlock.DesiredSize.Width);
        }
        else
        {
            Canvas.SetLeft(textBlock, x - textBlock.DesiredSize.Width / 2);
        }

        Canvas.SetTop(textBlock, y);
        canvas.Children.Add(textBlock);

        return y + textBlock.DesiredSize.Height;
    }

    private static double MeasureColumnWidth(System.Collections.Generic.List<TextSegment> segments, double fontSize, TextPrintBoxConfigure config)
    {
        double maxWidth = 0;
        foreach (var segment in segments)
        {
            double segmentFontSize = segment.IsSmall ? fontSize - 5 : fontSize;
            string text = segment.Text;
            string currentEnglishWord = "";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isAsciiLetterOrDigit = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c);

                if (isAsciiLetterOrDigit)
                {
                    currentEnglishWord += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentEnglishWord))
                    {
                        maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(currentEnglishWord, segmentFontSize, config));
                        currentEnglishWord = "";
                    }

                    maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(c.ToString(), segmentFontSize, config));
                }
            }

            if (!string.IsNullOrEmpty(currentEnglishWord))
            {
                maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(currentEnglishWord, segmentFontSize, config));
            }
        }

        return maxWidth;
    }

    private static double MeasureTextWidth(string text, double fontSize, TextPrintBoxConfigure config)
    {
        var run = new Run(text);
        var textBlock = new TextBlock(run)
        {
            FontFamily = new FontFamily("KaiTi"),
            FontSize = fontSize,
        };
        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return textBlock.DesiredSize.Width;
    }

    private static int CountLogicalLines(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        int count = 0;
        bool inEnglishWord = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool isAsciiLetterOrDigit = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c);

            if (isAsciiLetterOrDigit)
            {
                if (!inEnglishWord)
                {
                    count++;
                    inEnglishWord = true;
                }
            }
            else
            {
                inEnglishWord = false;
                count++;
            }
        }
        return count;
    }
}
