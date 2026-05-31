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
    public double MainMaxFontSize { get; set; } = 60.0;
    public double SideMaxFontSize { get; set; } = 30.0;
}

public class WeeklyPrintTypeConfig
{
    public string TypeName { get; set; } = string.Empty;
    public ElementRect MainTextBox { get; set; } = new ElementRect();
    public ElementRect SideTextBox { get; set; } = new ElementRect();
    public ImageElement Background { get; set; } = new ImageElement();
    public double FontSize { get; set; } = 12.0;
    public int Row { get; set; } = 4;
    public int Column { get; set; } = 7;
}

public class PropertyPrintConfig
{
    public double MarginLeftCm { get; set; } = 0.5;
    public double MarginRightCm { get; set; } = 0.5;
    public double MarginTopCm { get; set; } = 1.5;
    public double MarginBottomCm { get; set; } = 0.7;
    
    public int Columns { get; set; } = 5;
    public int Rows { get; set; } = 2;

    public ElementRect Main { get; set; } = new ElementRect(0.5, 0.5, 3.0, 8.0);
    public ElementRect Side { get; set; } = new ElementRect(1.0, 9.0, 2.0, 4.0);

    public int MainFontLevel1MaxChars { get; set; } = 50;
    public int MainFontLevel1Size { get; set; } = 18;
    public int MainFontLevel2MaxChars { get; set; } = 100;
    public int MainFontLevel2Size { get; set; } = 16;
    public int MainFontSmallSize { get; set; } = 14;
}

[XmlRoot("PrintLayoutSettings")]
public class PrintLayoutSettings
{
    public List<TypeLayoutConfig> TypeConfigs { get; set; } = new List<TypeLayoutConfig>();
    
    public PropertyPrintConfig PropertyPrintConfig { get; set; } = new PropertyPrintConfig();

    public List<WeeklyPrintTypeConfig> WeeklyPrintSettings { get; set; } = new List<WeeklyPrintTypeConfig>();

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

    public WeeklyPrintTypeConfig? GetWeeklyConfig(string typeName)
    {
        return WeeklyPrintSettings.FirstOrDefault(t => t.TypeName == typeName);
    }
}

public static class UnitConverter
{
    public const double CmToPx = 96.0 / 2.54; // 37.79527559055118
    public static double ToPx(double cm) => cm * CmToPx;
    public static double ToCm(double px) => px / CmToPx;
}
