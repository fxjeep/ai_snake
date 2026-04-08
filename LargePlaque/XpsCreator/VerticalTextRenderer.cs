using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace XpsCreator;

public class TextSegment
{
    public string Text { get; set; } = "";
    public bool IsSmall { get; set; }
}

public static class VerticalTextRenderer
{
    public static System.Collections.Generic.List<TextSegment> ParseSegments(string text)
    {
        var segments = new System.Collections.Generic.List<TextSegment>();
        if (string.IsNullOrEmpty(text)) return segments;

        try
        {
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
            segments.Add(new TextSegment { Text = text, IsSmall = false });
        }

        return segments;
    }

    public static int CountLogicalLines(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        int count = 0;
        bool inEnglishWord = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool isAsciiLetterOrDigit = IsHorizontalSegmentChar(c);

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

    public static void RenderText(Canvas canvas, string[] lines, ElementRect rect, double maxAllowedFontSize, bool centerVertically, double columnGap)
    {
        if (lines == null || lines.Length == 0) return;

        var lineSegments = lines.Select(l => ParseSegments(l)).ToList();
        if (!lineSegments.Any()) return;

        double rectWidth = UnitConverter.ToPx(rect.Width);
        double rectHeight = UnitConverter.ToPx(rect.Height);

        double minFont = 5;
        double maxFont = maxAllowedFontSize;
        double fontSize = minFont;

        for (int iter = 0; iter < 15; iter++)
        {
            double mid = (minFont + maxFont) / 2.0;
            bool fits = true;

            double maxColHeight = 0;
            double totalWidth = 0;
            for (int i = 0; i < lineSegments.Count; i++)
            {
                double h = MeasureColumnHeight(lineSegments[i], mid);
                double w = MeasureColumnWidth(lineSegments[i], mid);
                if (h > maxColHeight) maxColHeight = h;
                totalWidth += w;
            }

            double totalWidthWithGaps = totalWidth + columnGap * (lineSegments.Count - 1);

            if (maxColHeight > rectHeight || totalWidthWithGaps > rectWidth)
            {
                fits = false;
            }

            if (fits)
            {
                fontSize = mid;
                minFont = mid;
            }
            else
            {
                maxFont = mid;
            }
        }

        DrawColumns(canvas, lineSegments, fontSize, rect, centerVertically, columnGap);
    }

    private static void DrawColumns(Canvas canvas, System.Collections.Generic.List<System.Collections.Generic.List<TextSegment>> lineSegments, double fontSize, ElementRect rect, bool centerVertically, double columnGap)
    {
        double[] colWidths = new double[lineSegments.Count];
        double[] colHeights = new double[lineSegments.Count];

        for (int i = 0; i < lineSegments.Count; i++)
        {
            colWidths[i] = MeasureColumnWidth(lineSegments[i], fontSize);
            colHeights[i] = MeasureColumnHeight(lineSegments[i], fontSize);
        }

        double rectLeft = UnitConverter.ToPx(rect.Left);
        double rectTop = UnitConverter.ToPx(rect.Top);
        double rectWidth = UnitConverter.ToPx(rect.Width);
        double rectHeight = UnitConverter.ToPx(rect.Height);

        double totalWidthWithGaps = colWidths.Sum() + columnGap * (lineSegments.Count - 1);

        double startRight;
        startRight = rectLeft + (rectWidth + totalWidthWithGaps) / 2;

        double currentXCenter = startRight;

        double startY = rectTop;
        if (centerVertically && colHeights.Length > 0)
        {
            double maxHeight = colHeights.Max();
            startY = rectTop + (rectHeight - maxHeight) / 2;
            if (startY < rectTop) startY = rectTop; // don't overflow top
        }

        for (int i = 0; i < lineSegments.Count; i++)
        {
            if (i == 0)
            {
                currentXCenter -= colWidths[i] / 2;
            }
            else
            {
                currentXCenter -= (colWidths[i - 1] / 2 + columnGap + colWidths[i] / 2);
            }

            DrawVerticalString(canvas, lineSegments[i], currentXCenter, colWidths[i], fontSize, startY);
        }
    }

    private static void DrawVerticalString(Canvas canvas, System.Collections.Generic.List<TextSegment> segments, double x, double colWidth, double fontSize, double startY)
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
                bool isAsciiLetterOrDigit = IsHorizontalSegmentChar(c);

                if (isAsciiLetterOrDigit)
                {
                    currentEnglishWord += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentEnglishWord))
                    {
                        currentY = DrawTextElement(canvas, currentEnglishWord, x, colWidth, segmentFontSize, currentY, segment.IsSmall);
                        currentEnglishWord = "";
                    }

                    currentY = DrawTextElement(canvas, c.ToString(), x, colWidth, segmentFontSize, currentY, segment.IsSmall);
                }
            }

            if (!string.IsNullOrEmpty(currentEnglishWord))
            {
                currentY = DrawTextElement(canvas, currentEnglishWord, x, colWidth, segmentFontSize, currentY, segment.IsSmall);
            }
        }
    }

    private static double DrawTextElement(Canvas canvas, string text, double x, double colWidth, double fontSize, double y, bool isSmall)
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

    private static double MeasureColumnWidth(System.Collections.Generic.List<TextSegment> segments, double fontSize)
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
                bool isAsciiLetterOrDigit = IsHorizontalSegmentChar(c);

                if (isAsciiLetterOrDigit)
                {
                    currentEnglishWord += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentEnglishWord))
                    {
                        maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(currentEnglishWord, segmentFontSize));
                        currentEnglishWord = "";
                    }

                    maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(c.ToString(), segmentFontSize));
                }
            }

            if (!string.IsNullOrEmpty(currentEnglishWord))
            {
                maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(currentEnglishWord, segmentFontSize));
            }
        }

        return maxWidth;
    }

    private static double MeasureColumnHeight(System.Collections.Generic.List<TextSegment> segments, double fontSize)
    {
        double height = 0;
        foreach (var segment in segments)
        {
            double segmentFontSize = segment.IsSmall ? fontSize - 5 : fontSize;
            string text = segment.Text;
            string currentEnglishWord = "";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isAsciiLetterOrDigit = IsHorizontalSegmentChar(c);

                if (isAsciiLetterOrDigit)
                {
                    currentEnglishWord += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentEnglishWord))
                    {
                        height += MeasureTextHeight(currentEnglishWord, segmentFontSize);
                        currentEnglishWord = "";
                    }

                    height += MeasureTextHeight(c.ToString(), segmentFontSize);
                }
            }

            if (!string.IsNullOrEmpty(currentEnglishWord))
            {
                height += MeasureTextHeight(currentEnglishWord, segmentFontSize);
            }
        }

        return height;
    }

    private static double MeasureTextWidth(string text, double fontSize)
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

    private static double MeasureTextHeight(string text, double fontSize)
    {
        var run = new Run(text);
        var textBlock = new TextBlock(run)
        {
            FontFamily = new FontFamily("KaiTi"),
            FontSize = fontSize,
        };
        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return textBlock.DesiredSize.Height;
    }

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
}
