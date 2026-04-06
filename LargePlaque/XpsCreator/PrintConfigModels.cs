using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace XpsCreator;

public class ElementRect
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public ElementRect() { }
    public ElementRect(double l, double t, double w, double h)
    {
        Left = l; Top = t; Width = w; Height = h;
    }
}

public class ImageElement : ElementRect
{
    public string FileName { get; set; } = string.Empty;

    public ImageElement() { }
    public ImageElement(string fileName, double l, double t, double w, double h)
        : base(l, t, w, h)
    {
        FileName = fileName;
    }
}

public class TypeLayoutConfig
{
    public string TypeName { get; set; } = string.Empty;
    public ElementRect MainTextBox { get; set; } = new ElementRect();
    public ElementRect SideTextBox { get; set; } = new ElementRect();
    public ImageElement Stamp { get; set; } = new ImageElement();
    public ImageElement Background { get; set; } = new ImageElement();
    public double MainMaxFontSize { get; set; } = 1000.0;
    public double SideMaxFontSize { get; set; } = 1000.0;
}

[XmlRoot("PrintLayoutSettings")]
public class PrintLayoutSettings
{
    public List<TypeLayoutConfig> TypeConfigs { get; set; } = new List<TypeLayoutConfig>();

    public static string DefaultConfigPath => "PrintConfigure.xml";

    public static void Save(string filePath, PrintLayoutSettings settings)
    {
        var serializer = new XmlSerializer(typeof(PrintLayoutSettings));
        using (var writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, settings);
        }
    }

    public static PrintLayoutSettings Load(string filePath)
    {
        if (!File.Exists(filePath)) return new PrintLayoutSettings();
        var serializer = new XmlSerializer(typeof(PrintLayoutSettings));
        using (var reader = new StreamReader(filePath))
        {
            return (PrintLayoutSettings)serializer.Deserialize(reader)!;
        }
    }

    public TypeLayoutConfig? GetConfig(string typeName)
    {
        return TypeConfigs.FirstOrDefault(t => t.TypeName == typeName);
    }
}

public static class UnitConverter
{
    public const double CmToPx = 96.0 / 2.54; // 37.79527559055118
    public static double ToPx(double cm) => cm * CmToPx;
    public static double ToCm(double px) => px / CmToPx;

    public static TextPrintBoxConfigure ToTextConfig(ElementRect rect)
    {
        return new TextPrintBoxConfigure
        {
            Left = ToPx(rect.Left),
            Top = ToPx(rect.Top),
            Width = ToPx(rect.Width),
            Height = ToPx(rect.Height),
            ColumnAlign = EnumColumnAlign.Center // Default or add to config?
        };
    }

    public static StampPositionConfig? ToStampConfig(ImageElement stamp)
    {
        if (string.IsNullOrEmpty(stamp.FileName)) return null;
        return new StampPositionConfig
        {
            Left = ToPx(stamp.Left),
            Top = ToPx(stamp.Top),
            Size = ToPx(stamp.Width) // Assume horizontal size for now?
        };
    }

}
