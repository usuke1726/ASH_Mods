
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ModdingAPI.IO;

public class ConfigFileBase : TextFileCore
{
    private interface IContent
    {
        public string Content { get; }
    }
    private class Prop(string name, string value, string? comment, bool forceQuotation) : IContent
    {
        public readonly string name = name;
        public string value = value;
        public readonly string comment = comment != null ? $" {comment}" : "";
        public readonly bool forceQuotation = forceQuotation;
        public string Content { get => $"{name} = {Value}{comment}"; }
        private static string Escape(string val)
        {
            return val
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\r", "\\r");
        }
        private string Value
        {
            get
            {
                var v = Escape(value);
                return NeedQuotation(v) ? $"\"{v}\"" : v;
            }
        }
        private static readonly List<char> specialChars = [';', '#', '\\'];
        private bool NeedQuotation(string v)
        {
            if (forceQuotation) return true;
            return specialChars.Any(c => v.Contains(c));
        }
    }
    private class Comment(string comment) : IContent
    {
        public readonly string comment = comment;
        public string Content { get => comment; }
    }
    private readonly Dictionary<string, Prop> props = [];
    private List<IContent> contents = [];
    private bool isRead = false;
    protected readonly Task ReadTask;
    public ConfigFileBase(IMod mod, string baseName, string? initialContents = null, string? comments = null) : base(mod, Name(baseName), isCache: false, createFileIfNotExists: true, initialContents: initialContents ?? CommentsToStr(comments))
    {
        ReadTask = Task.Run(Read);
    }
    public ConfigFileBase(IMod mod, string baseName, Func<string> initialContents, string? comments = null) : base(mod, Name(baseName), isCache: false, createFileIfNotExists: true, initialContents: initialContents)
    {
        ReadTask = Task.Run(Read);
    }
    protected static string CommentsToStr(string? comments)
    {
        if (comments == null) return "";
        return "\n" + string.Join('\n', comments.Replace("\r", "").Split('\n').Select(c => $"# {c}"));
    }
    private static string Name(string baseName)
    {
        return baseName.EndsWith(".cfg") ? baseName : $"{baseName}.cfg";
    }
    public async Task<bool> Write()
    {
        if (!isRead) return false;
        try
        {
            var s = "\n" + string.Join('\n', contents.Select(c => c.Content)).Trim();
            await WriteLines(s.Split('\n'));
            return true;
        }
        catch (Exception e)
        {
            Monitor.SLog($"Error on writing config file {FilePath}:\n{e}", LogLevel.Error);
            return false;
        }
    }
    public new void Read()
    {
        ReadContents(ReadLines());
        isRead = true;
    }
    private static readonly Regex pattern = new(@"^\s*([a-zA-Z_][a-zA-Z_0-9]*)\s*=\s*([^""][^;#]*)([;#].*)?$");
    private static readonly Regex patternS = new(@"^\s*([a-zA-Z_][a-zA-Z_0-9]*)\s*=\s*""([^""]*)""\s*([;#].*)?$");
    private void ReadContents(IEnumerable<string> lines)
    {
        contents.Clear();
        List<string> commentLines = [];
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith('#') || line.Trim().StartsWith(';'))
            {
                commentLines.Add(line);
                continue;
            }
            if (TryToCreateProp(line, out var prop))
            {
                if (commentLines.Count > 0)
                {
                    contents.Add(new Comment(string.Join('\n', commentLines)));
                    commentLines.Clear();
                }
                contents.Add(prop);
                props[prop.name] = prop;
            }
            else
            {
                commentLines.Add(line.Trim().Length > 0 ? $"# {line}" : line);
            }
        }
    }
    private static bool TryToCreateProp(string line, [NotNullWhen(true)] out Prop? prop)
    {
        var m = pattern.Match(line);
        if (m.Success)
        {
            Monitor.SLog($"found property {m.Groups[1].Value}, {m.Groups[2].Value}, {m.Groups[3].Value}", LogLevel.Debug);
            prop = new(m.Groups[1].Value, m.Groups[2].Value.Trim(), m.Groups[3].Value, false);
            return true;
        }
        m = patternS.Match(line);
        if (m.Success)
        {
            prop = new(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, true);
            return true;
        }
        prop = null;
        return false;
    }
    public bool? GetAsBool(string name)
    {
        if (!props.TryGetValue(name, out var prop)) return null;
        return bool.TryParse(prop.value, out var ret) ? ret : null;
    }
    public int? GetAsInt(string name)
    {
        if (!props.TryGetValue(name, out var prop)) return null;
        return int.TryParse(prop.value, out var ret) ? ret : null;
    }
    public float? GetAsFloat(string name)
    {
        if (!props.TryGetValue(name, out var prop)) return null;
        return float.TryParse(prop.value, out var ret) ? ret : null;
    }
    public string? GetAsString(string name)
    {
        return props.TryGetValue(name, out var ret) ? ret.value : null;
    }
    private void _Set(string name, string v, string? comment)
    {
        if (v is null) throw new Exception($"Tried to set null value (name: {name})");
        if (props.TryGetValue(name, out var prop)) prop.value = v;
        else
        {
            Prop newProp = new(name, v, comment, false);
            props[name] = newProp;
            contents.Add(newProp);
        }
    }
    public static string ToString(bool value) => value ? "true" : "false";
    public static string ToString(int value) => value.ToString();
    public static string ToString(float value) => value.ToString();
    public void Set(string name, bool value, string? comment = null) => _Set(name, ToString(value), comment);
    public void Set(string name, int value, string? comment = null) => _Set(name, ToString(value), comment);
    public void Set(string name, float value, string? comment = null) => _Set(name, ToString(value), comment);
    public void Set(string name, string value, string? comment = null) => _Set(name, value, comment);
}

partial class FileWrapper
{
    public static ConfigFileBase ConfigFileBase(this IMod mod, string baseName, string? initialContents = null, string? comments = null) => new(mod, baseName, initialContents, comments);
    public static ConfigFileBase ConfigFileBase(this IMod mod, string baseName, Func<string> initialContents, string? comments = null) => new(mod, baseName, initialContents, comments);
}

