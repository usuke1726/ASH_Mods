
namespace ModdingAPI.IO;

public class TextFile : TextFileCore
{
    public TextFile(IMod mod, string baseName, bool isCache = false, bool createFileIfNotExists = true, string initialContents = "") : base(mod, baseName, isCache, createFileIfNotExists, initialContents) { }
    public new async Task<string> Read() => await base.Read();
    public new async Task<string[]> ReadAllLines() => await base.ReadAllLines();
    public new IEnumerable<string> ReadLines() => base.ReadLines();
    public new async Task Write(string contents) => await base.Write(contents);
    public new async Task WriteLines(IEnumerable<string> contents) => await base.WriteLines(contents);
    public new async Task Add(string contents) => await base.Add(contents);
    public new async Task AddLines(IEnumerable<string> contents) => await base.AddLines(contents);
}

public class TextCache(IMod mod, string baseName, bool createFileIfNotExists = true, string initialContents = "") : TextFile(mod, baseName, true, createFileIfNotExists, initialContents) { }

partial class FileWrapper
{
    public static TextFile TextFile(this IMod mod, string baseName, bool isCache = false, bool createFileIfNotExists = true, string initialContents = "") => new(mod, baseName, isCache, createFileIfNotExists, initialContents);
    public static TextCache TextCache(this IMod mod, string baseName, bool createFileIfNotExists = true, string initialContents = "") => new(mod, baseName, createFileIfNotExists, initialContents);
}

