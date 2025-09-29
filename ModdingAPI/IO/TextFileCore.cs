
using System.Text;

namespace ModdingAPI.IO;

public abstract class TextFileCore : Core
{
    protected override string BaseName { get; }
    public bool FileCreated { get; private set; } = false;
    public TextFileCore(IMod mod, string baseName, bool isCache = false, bool createFileIfNotExists = true, Func<string>? initialContents = null) : base(mod, isCache)
    {
        BaseName = baseName;
        if (createFileIfNotExists) CreateFileIfNotExists(initialContents ?? (() => ""));
    }
    public TextFileCore(IMod mod, string baseName, bool isCache = false, bool createFileIfNotExists = true, string initialContents = "") : this(mod, baseName, isCache, createFileIfNotExists, () => initialContents) { }
    private void CreateFileIfNotExists(Func<string> initialContents)
    {
        if (!Exists())
        {
            FileCreated = true;
            File.WriteAllLines(FilePath, initialContents().Split('\n'));
        }
    }
    protected async Task<string> Read() => await File.ReadAllTextAsync(FilePath, Encoding.UTF8);
    protected async Task<string[]> ReadAllLines() => await File.ReadAllLinesAsync(FilePath, Encoding.UTF8);
    protected IEnumerable<string> ReadLines() => File.ReadLines(FilePath, Encoding.UTF8);
    protected async Task Write(string contents) => await File.WriteAllTextAsync(FilePath, contents, Encoding.UTF8);
    protected async Task WriteLines(IEnumerable<string> contents) => await File.WriteAllLinesAsync(FilePath, contents, Encoding.UTF8);
    protected async Task Add(string contents) => await File.AppendAllTextAsync(FilePath, contents, Encoding.UTF8);
    protected async Task AddLines(IEnumerable<string> contents) => await File.AppendAllLinesAsync(FilePath, contents, Encoding.UTF8);
}

