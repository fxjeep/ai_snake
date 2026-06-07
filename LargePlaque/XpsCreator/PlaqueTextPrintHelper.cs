using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using XpsCreator;

public class PlaqueTextPrintHelper
{
    public static void PrintNameInWrapMode(Canvas canvas, string name, ElementRect textBox, double fontSize)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        double boxLeftPx   = UnitConverter.ToPx(textBox.Left);
        double boxTopPx    = UnitConverter.ToPx(textBox.Top);
        double boxHeightPx = UnitConverter.ToPx(textBox.Height);
        double boxWidthPx  = UnitConverter.ToPx(textBox.Width);

        // Before rotation the TextBlock is horizontal:
        //   - Width  = box height (becomes the vertical span after CW 90° rotation)
        //   - Height is unconstrained (wrapping controls it; becomes the horizontal
        //     width consumed in the plaque cell after rotation)
        var tb = new TextBlock
        {
            Text          = name,
            FontFamily    = new FontFamily("KaiTi"),
            FontSize      = fontSize,
            TextWrapping  = TextWrapping.Wrap,
            Width         = boxHeightPx,
            TextAlignment = TextAlignment.Left,
        };

        // Rotate CW 90°.
        // After rotation the element's pre-rotation top-left corner maps to the
        // point (canvas-X, canvas-Y + boxHeight), so place it at the bottom-left
        // of the target box to make the rotated block sit inside the side box.
        tb.RenderTransform = new System.Windows.Media.RotateTransform(90);
        Canvas.SetLeft(tb, boxLeftPx+boxWidthPx+2.5);
        Canvas.SetTop(tb,  boxTopPx+0.4);

        canvas.Children.Add(tb);
    }

    public static void PrintNameToRecord(Canvas itemCanvas, string name, ElementRect textBox, double fontSize)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        double textBoxLeft = UnitConverter.ToPx(textBox.Left);
        double textBoxTop = UnitConverter.ToPx(textBox.Top);
        double textBoxWidth = UnitConverter.ToPx(textBox.Width);

        var parsedLines = ParseNameIntoLines(name);
        if (!parsedLines.Any()) return;

        var textBlocks = new System.Collections.Generic.List<TextBlock>();
        foreach (var line in parsedLines)
        {
            var run = new Run(line);
            var tb = new TextBlock(run)
            {
                FontFamily = new FontFamily("KaiTi"),
                FontSize = fontSize,
            };
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlocks.Add(tb);
        }

        double currentY = textBoxTop;
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

    public static System.Collections.Generic.List<string> ParseNameIntoLines(string name)
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

    /// <summary>
    /// Returns true if <paramref name="text"/> contains at least one English
    /// or Vietnamese (Latin-extended) character.
    /// </summary>
    public static bool HasEnglishOrVietnamese(string text)
    {
        foreach (char c in text)
        {
            if (IsHorizontalSegmentChar(c)) return true;
        }
        return false;
    }

    public static bool IsHorizontalSegmentChar(char c)
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