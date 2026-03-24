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
    public static void Print(Canvas canvas, string[] lines, bool printBorder, bool printStamp, TextPrintBoxConfigure textConfig, StampPositionConfig? stampConfig, FontSize fontSizeConfig)
    {
        if (lines == null || lines.Length == 0) return;

        int maxChars = lines.Max(l => l.Length);
        double fontSize;
        if (maxChars <= 5) fontSize = fontSizeConfig.Large;
        else if (maxChars < 10) fontSize = fontSizeConfig.Medium;
        else fontSize = fontSizeConfig.Small;

        if (printBorder)
        {
            DrawBorder(canvas, textConfig);
        }

        if (printStamp && stampConfig != null)
        {
            DrawStamp(canvas, stampConfig);
        }

        // Process all lines as individual columns
        DrawColumns(canvas, lines, fontSize, textConfig);
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

    public static void DrawColumns(Canvas canvas, string[] lines, double fontSize, TextPrintBoxConfigure config)
    {
        double[] colWidths = new double[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            colWidths[i] = MeasureColumnWidth(lines[i], fontSize, config);
        }

        double gap = 20; // Default gap between columns
        double totalWidthWithGaps = colWidths.Sum() + gap * (lines.Length - 1);

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

        for (int i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                currentXCenter -= colWidths[i] / 2;
            }
            else
            {
                currentXCenter -= (colWidths[i - 1] / 2 + gap + colWidths[i] / 2);
            }

            DrawVerticalString(canvas, lines[i], currentXCenter, colWidths[i], fontSize, config.Top, config);
        }
    }

    private static void DrawVerticalString(Canvas canvas, string text, double x, double colWidth, double fontSize, double startY, TextPrintBoxConfigure config)
    {
        double currentY = startY;
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
                    currentY = DrawTextElement(canvas, currentEnglishWord, x, colWidth, fontSize, currentY, config);
                    currentEnglishWord = "";
                }

                if (!char.IsWhiteSpace(c))
                {
                    currentY = DrawTextElement(canvas, c.ToString(), x, colWidth, fontSize, currentY, config);
                }
            }
        }

        if (!string.IsNullOrEmpty(currentEnglishWord))
        {
            DrawTextElement(canvas, currentEnglishWord, x, colWidth, fontSize, currentY, config);
        }
    }

    private static double DrawTextElement(Canvas canvas, string text, double x, double colWidth, double fontSize, double y, TextPrintBoxConfigure config)
    {
        var run = new Run(text);
        var textBlock = new TextBlock(run)
        {
            FontFamily = new FontFamily("KaiTi"),
            FontSize = fontSize,
        };

        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        if (config.ColumnAlign == EnumColumnAlign.Right)
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

    private static double MeasureColumnWidth(string text, double fontSize, TextPrintBoxConfigure config)
    {
        double maxWidth = 0;
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
                    maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(currentEnglishWord, fontSize, config));
                    currentEnglishWord = "";
                }

                if (!char.IsWhiteSpace(c))
                {
                    maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(c.ToString(), fontSize, config));
                }
            }
        }

        if (!string.IsNullOrEmpty(currentEnglishWord))
        {
            maxWidth = System.Math.Max(maxWidth, MeasureTextWidth(currentEnglishWord, fontSize, config));
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
}
