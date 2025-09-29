
namespace ModdingAPI;

using T1 = string;
using T2 = IEnumerable<string>;
using T3 = IEnumerable<StyleText>;
using T4 = IEnumerable<IEnumerable<string>>;

public enum EColor
{
    None,
    Black,
    Red,
    Green,
    Yellow,
    Blue,
    Magenta,
    Cyan,
    Gray,
    White,
}

public class Decoration
{
    public bool Bold = false;
    public bool Dim = false;
    public bool Italic = false;
    public bool Underline = false;
    public bool Invert = false;

    public Decoration() { }
    public bool IsEmpty() => !Bold && !Dim && !Italic && !Underline && !Underline && !Invert;
    public IEnumerable<string> ToList()
    {
        IEnumerable<string> list = [];
        if (Bold) list = list.Append("1");
        if (Dim) list = list.Append("2");
        if (Italic) list = list.Append("3");
        if (Underline) list = list.Append("4");
        if (Invert) list = list.Append("7");
        return list;
    }
}

public static class EStyle
{
    public static class Decoration
    {
        public const string Bold = "bold";
        public const string Dim = "dim";
        public const string Italic = "italic";
        public const string Underline = "underline";
        public const string Invert = "invert";
    }
    public static class Color
    {
        public const string None = "none";
        public const string Black = "black";
        public const string Red = "red";
        public const string Green = "green";
        public const string Yellow = "yellow";
        public const string Blue = "blue";
        public const string Magenta = "magenta";
        public const string Cyan = "cyan";
        public const string Gray = "gray";
        public const string White = "white";
    }
}

public class Style
{
    public EColor Color = EColor.None;
    public Decoration Decoration = new();
    private string Str()
    {
        var list = Decoration.ToList();
        if (Color == EColor.Gray) list = list.Append("2");
        var color = Color switch
        {
            EColor.Black => "30",
            EColor.Red => "31",
            EColor.Green => "32",
            EColor.Yellow => "33",
            EColor.Blue => "34",
            EColor.Magenta => "35",
            EColor.Cyan => "36",
            _ => ""
        };
        if (color != "") list = list.Append(color);
        return "\\x1b[" + string.Join(';', list) + "m";
    }
    public string Str(string mes)
    {
        if (Color == EColor.None && Decoration.IsEmpty()) return mes;
        return $"{Str()}{mes}\\x1b[0m";
    }

    public static string Str(string mes, string style) => From(style).Str(mes);
    public static Style From(IEnumerable<string> styles)
    {
        Style ret = new();
        foreach (var entry in styles)
        {
            switch (entry.Trim().ToLower())
            {
                case EStyle.Color.Black: ret.Color = EColor.Black; break;
                case EStyle.Color.Red: ret.Color = EColor.Red; break;
                case EStyle.Color.Green: ret.Color = EColor.Green; break;
                case EStyle.Color.Yellow: ret.Color = EColor.Yellow; break;
                case EStyle.Color.Blue: ret.Color = EColor.Blue; break;
                case EStyle.Color.Magenta: ret.Color = EColor.Magenta; break;
                case EStyle.Color.Cyan: ret.Color = EColor.Cyan; break;
                case EStyle.Color.White: ret.Color = EColor.White; break;
                case EStyle.Color.Gray: ret.Color = EColor.Gray; break;
                case EStyle.Decoration.Bold: ret.Decoration.Bold = true; break;
                case EStyle.Decoration.Dim: ret.Decoration.Dim = true; break;
                case EStyle.Decoration.Italic: ret.Decoration.Italic = true; break;
                case EStyle.Decoration.Underline: ret.Decoration.Underline = true; break;
                case EStyle.Decoration.Invert: ret.Decoration.Invert = true; break;
            }
        }
        return ret;
    }
    public static Style From(string style) => From(style.Replace(';', ',').Split(','));
    public static Style From(LogLevel level)
    {
        return level switch
        {
            LogLevel.Fatal => new() { Color = EColor.Red, Decoration = new() { Bold = true } },
            LogLevel.Error => new() { Color = EColor.Red, Decoration = new() { Bold = true } },
            LogLevel.Warning => new() { Color = EColor.Yellow, Decoration = new() { Bold = true } },
            LogLevel.Debug => new() { Color = EColor.Gray },
            _ => new() { Color = EColor.None },
        };
    }

    public static string Format(T2 texts) => string.Join("", texts);
    public static string Format(T3 styleTexts) => Format(styleTexts.Select(d => d.Style.Str(d.Text)));
    public static string Format(T4 styleTexts)
    {
        return Format(styleTexts.Select(arr =>
        {
            var mes = arr.FirstOrDefault() ?? "";
            var style = arr.ElementAtOrDefault(1);
            return string.IsNullOrEmpty(style) ? mes : From(style).Str(mes);
        }));
    }
    public static string Format(object data)
    {
        if (data is T1 v1) return v1;
        if (data is T2 v2) return Format(v2);
        if (data is T3 v3) return Format(v3);
        if (data is T4 v4) return Format(v4);
        return "";
    }
    public static string NoStyle(T2 texts) => Format(texts);
    public static string NoStyle(T3 styleTexts) => Format(styleTexts.Select(d => d.Text));
    public static string NoStyle(T4 styleTexts) => Format(styleTexts.Select(d => d.FirstOrDefault() ?? ""));
    public static string NoStyle(object data)
    {
        if (data is T1 v1) return v1;
        if (data is T2 v2) return NoStyle(v2);
        if (data is T3 v3) return NoStyle(v3);
        if (data is T4 v4) return NoStyle(v4);
        return "";
    }
}

public struct StyleText
{
    public string Text;
    public Style Style;
}

