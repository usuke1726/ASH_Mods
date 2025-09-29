namespace ModdingAPI.IO;

public abstract class Core
{
    protected abstract string BaseName { get; }

    protected readonly string ID;
    protected readonly string HomePath;
    private string fileName = null!;
    private string filePath = null!;
    private string FileNameForIsCache() => IsCache ? $"{ID}.{BaseName}" : BaseName;
    public string FileName { get => fileName ??= FileNameForIsCache(); }
    public string FilePath { get => filePath ??= $"{HomePath}/{FileName}"; }
    public bool IsCache { get; }
    public Core(IMod mod, bool isCache = false)
    {
        ID = mod.UniqueID;
        IsCache = isCache;
        HomePath = isCache ? BepInEx.Paths.CachePath : mod.HomePath;
    }
    public bool Exists() => File.Exists(FilePath);
    public static List<string> GetFiles(IMod mod, string globPattern, bool isCache = false)
    {
        globPattern = globPattern.Replace("\\", "/");
        if (globPattern.Contains("**"))
        {
            Monitor.SLog(I18n_.Localize("IO.Core.Warn.GlobIsUnsupported"), LogLevel.Warning);
            globPattern = globPattern.Replace("**/", "");
        }
        var homePath = isCache ? BepInEx.Paths.CachePath : mod.HomePath;
        homePath = homePath.Replace('\\', '/');
        try
        {
            return [.. Directory.EnumerateFiles(homePath, globPattern)
            .Select(p => p.Replace('\\', '/'))
            .Where(p => p.StartsWith($"{homePath}/"))
            .Select(p => p[(homePath.Length + 1)..])];
        }
        catch { return []; }
    }
}

public static partial class FileWrapper;

