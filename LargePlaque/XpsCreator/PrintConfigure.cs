namespace XpsCreator;

public enum EnumColumnAlign
{
    Center,
    Right
}

public class TextPrintBoxConfigure
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public EnumColumnAlign ColumnAlign { get; set; }
    public double ColumnGap { get; set; } = 20;
}

public class MainBoxConfigure
{
    public TextPrintBoxConfigure TextPrintBox { get; set; } = new TextPrintBoxConfigure();
    public StampPositionConfig StampPosition { get; set; } = new StampPositionConfig();
}

public class StampPositionConfig
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Size { get; set; }
}

public class FontSize
{
    public double Large { get; set; }
    public double Medium { get; set; }
    public double Small { get; set; }
    public int LargeLines { get; set; }
    public int MediumLines { get; set; }
}

public class PrintConfigure
{
    private const double cmToPx = 37.79527559;

    public static FontSize Main { get; } = new FontSize { Large = 47, Medium = 37, Small = 30, LargeLines = 7, MediumLines = 10 };
    public static FontSize Side { get; } = new FontSize { Large = 25, Medium = 25, Small = 20, LargeLines = 5, MediumLines = 9 };

    public static FontSize SideYuanQin { get; } = new FontSize { Large = 33, Medium = 28, Small = 25, LargeLines = 5, MediumLines = 8 };

    public static MainBoxConfigure ChangeShengConfig { get; } = new MainBoxConfigure
    {
        TextPrintBox = new TextPrintBoxConfigure
        {
            Left = 11.6 * cmToPx,
            Top = 16 * cmToPx,
            Width = 6.5 * cmToPx,
            Height = 10.5 * cmToPx,
            ColumnAlign = EnumColumnAlign.Center
        },
        StampPosition = new StampPositionConfig
        {
            Left = 11.8 * cmToPx,
            Top = 2.0 * cmToPx,
            Size = 6.0 * cmToPx
        }
    };

    public static MainBoxConfigure YuanQinConfig { get; } = new MainBoxConfigure
    {
        TextPrintBox = new TextPrintBoxConfigure
        {
            Left = 8.1 * cmToPx,
            Top = 21 * cmToPx,
            Width = 2.2 * cmToPx,
            Height = 10 * cmToPx,
            ColumnAlign = EnumColumnAlign.Right,
            ColumnGap = 5
        },
        StampPosition = new StampPositionConfig
        {
            Left = 11.5 * cmToPx,
            Top = 1.0 * cmToPx,
            Size = 6.0 * cmToPx
        }
    };

    public static MainBoxConfigure WangShengMain { get; } = new MainBoxConfigure
    {
        TextPrintBox = new TextPrintBoxConfigure
        {
            Left = 11.6 * cmToPx,
            Top = 16.5 * cmToPx,
            Width = 6.5 * cmToPx,
            Height = 10 * cmToPx,
            ColumnAlign = EnumColumnAlign.Center
        },
        StampPosition = new StampPositionConfig
        {
            Left = 12 * cmToPx,
            Top = 2.5 * cmToPx,
            Size = 6.0 * cmToPx
        }
    };

    public static TextPrintBoxConfigure WangShengSide { get; } = new TextPrintBoxConfigure
    {
        Left = 8.1 * cmToPx,
        Top = 23.1 * cmToPx,
        Width = 2.5 * cmToPx,
        Height = 7 * cmToPx,
        ColumnAlign = EnumColumnAlign.Right,
        ColumnGap = 5
    };

    public static MainBoxConfigure ZhuXianMain { get; } = new MainBoxConfigure
    {
        TextPrintBox = new TextPrintBoxConfigure
        {
            Left = 11.6 * cmToPx,
            Top = 15.5 * cmToPx,
            Width = 6.5 * cmToPx,
            Height = 10.5 * cmToPx,
            ColumnAlign = EnumColumnAlign.Center
        },
        StampPosition = new StampPositionConfig
        {
            Left = 11.5 * cmToPx,
            Top = 1.0 * cmToPx,
            Size = 6.0 * cmToPx
        }
    };

    public static TextPrintBoxConfigure ZhuXianSide { get; } = new TextPrintBoxConfigure
    {
        Left = 5.6 * cmToPx,
        Top = 20.1 * cmToPx,
        Width = 4.7 * cmToPx,
        Height = 10.5 * cmToPx,
        ColumnAlign = EnumColumnAlign.Right,
        ColumnGap = 5
    };
}
