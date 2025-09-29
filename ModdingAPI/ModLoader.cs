
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using ModdingAPI.Events;
using ModdingAPI.IO;

namespace ModdingAPI;

internal static class ModLoader
{
    public static readonly string PreloadFolderName = ".preload";
    public static readonly string PreloadDllNameSuffix = $"{PreloadFolderName}.dll";
    public static readonly string RootPath = BepInEx.Paths.GameRootPath;
    public static string ModsPath => modsPath ??= GetModsPath();
    private static string modsPath = null!;
    public static string ModsFolderName { get; private set; } = null!;
    public static string DefaultModsPath()
    {
        string folderName = "Mods";
        string path = Path.Combine(RootPath, folderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        modsPath = path;
        ModsFolderName = folderName;
        ModdingApiInfo.ModsPath = Path.Combine(ModdingApiInfo.GAMEROOT_PATH, folderName);
        return path;
    }
    public static string GetModsPath()
    {
        try
        {
            var folder = Config.ModsPath;
            if (folder == PreloadFolderName)
            {
                throw new Exception(I18n_.Localize("ModLoader.Error.ModsPathConflictingPreload", PreloadFolderName));
            }
            var path = Path.Combine(RootPath, folder);
            if (!path.StartsWith(RootPath))
            {
                throw new Exception(I18n_.Localize("ModLoader.Error.ModsPathNotOnGameRootPath", RootPath));
            }
            if (Directory.Exists(path))
            {
                modsPath = path;
                ModsFolderName = folder;
                ModdingApiInfo.ModsPath = Path.Combine(ModdingApiInfo.GAMEROOT_PATH, folder);
                return path;
            }
            else
            {
                throw new Exception(I18n_.Localize("ModLoader.Error.InvalidModsPathName", folder, path));
            }
        }
        catch (Exception e)
        {
            Monitor.SLog(I18n_.Localize("ModLoader.Error.GetModsPathException", e.Message));
            return DefaultModsPath();
        }
    }

    public static async void LoadMods()
    {
        var path = ModsPath ?? GetModsPath();
        Monitor.SLog(I18n_.Localize("ModLoader.Info.ModsFolderName", ModsFolderName));
        if (!Directory.Exists(ModdingApiInfo.ModsPath))
        {
            Monitor.SLog(I18n_.Localize("ModLoader.Error.ModsPathNotFound", ModdingApiInfo.ModsPath), LogLevel.Warning);
        }
        await Monitor.SLogAsync(I18n_.Localize("ModLoader.Info.StartLoadingPreload"));
        foreach (var preloadFile in EnumerateDllFiles(path, true))
        {
            var displayPath = ExtractPath(preloadFile, path);
            try
            {
                var _ = Assembly.LoadFrom(preloadFile);
                await Monitor.SLogAsync(I18n_.Localize("ModLoader.Info.LoadedOnePreload", displayPath));
            }
            catch (Exception e)
            {
                await Monitor.SLogAsync(I18n_.Localize("ModLoader.Error.FailedOnLoadingPreload", displayPath, e), LogLevel.Error);
            }
        }
        await Monitor.SLogAsync(I18n_.Localize("ModLoader.Info.EndLoadingPreload"));
        await Monitor.SLogAsync(I18n_.Localize("ModLoader.Info.StartLoadingMod"));
        foreach (var modFile in EnumerateDllFiles(path, false))
        {
            var displayPath = ExtractPath(modFile, path);
            try
            {
                var modasm = Assembly.LoadFrom(modFile);
                if (TryLoadModEntry(modasm, out Mod? mod, out string? error))
                {
                    mod.HomePath = Path.GetDirectoryName(modFile);
                    try
                    {
                        mod.I18n_ = new API_I18n(mod);
                        mod.Entry(mod.Helper);
                        new Harmony(mod.UniqueID).PatchAll(modasm);
                        ModRegistry.instance.Add(mod);
                        await PrintModInfo(modasm, mod, displayPath);
                    }
                    catch (Exception e)
                    {
                        Monitor.SLog(I18n_.Localize("ModLoader.Error.FailedOnLoadingMod", mod.UniqueID, e), LogLevel.Error);
                    }
                }
                else
                {
                    Monitor.SLog(error, LogLevel.Error);
                }
            }
            catch (Exception e)
            {
                await Monitor.SLogAsync(I18n_.Localize("ModLoader.Error.FailedOnLoadingModAssembly", displayPath, e));
            }
        }
        await Monitor.SLogAsync(I18n_.Localize("ModLoader.Info.EndLoadingMod"));
        GameloopEvents.OnModsLoaded();
    }
    private static IEnumerable<string> EnumerateDllFiles(string path, bool searchingPreload, bool onThePreloadFolder = false)
    {
        var files = Enumerable.Empty<string>();
        try
        {
            files = Directory.EnumerateFiles(path, "*.dll").Where(p =>
            {
                if (onThePreloadFolder) return searchingPreload;
                else return searchingPreload == IsPreloadFile(p);
            });
        }
        catch (UnauthorizedAccessException)
        {
            Monitor.SLog(I18n_.Localize("ModLoader.Error.FolderUnauthorized", path));
        }
        catch (Exception e)
        {
            Monitor.SLog(I18n_.Localize("ModLoader.Error.FolderException", e));
        }
        try
        {
            files = Directory.EnumerateDirectories(path)
                .Aggregate(files, (a, b) => a.Union(EnumerateDllFiles(
                    b,
                    searchingPreload: searchingPreload,
                    onThePreloadFolder: onThePreloadFolder || BaseName(b) == PreloadFolderName
                )));
        }
        catch (UnauthorizedAccessException) { }
        return files;
    }
    private static bool IsPreloadFile(string filename)
    {
        return filename.EndsWith(PreloadDllNameSuffix);
    }
    private static string BaseName(string path)
    {
        var s = path.Replace('\\', '/');
        if (s.EndsWith('/')) s = s[..^1];
        var idx = s.LastIndexOf('/');
        return idx >= 0 ? s[(idx + 1)..] : s;
    }
    private static string ExtractPath(string path, string fromPath)
    {
        if (!path.StartsWith(fromPath) || path == fromPath) return path;
        return path.Substring(fromPath.Length);
    }
    private static void TryGetVersion(Assembly asm, out string version)
    {
        version = "";
        var types = asm.DefinedTypes.Where(type => type.Name == "MyPluginInfo" && type.IsClass).Take(2).ToArray();
        if (types.Length != 1) return;
        var type = types[0];
        var field = type.GetField("PLUGIN_VERSION", BindingFlags.Public | BindingFlags.Static);
        var v = field?.GetRawConstantValue();
        if (v != null && v is string _version) version = $" {_version}";
    }
    private static async Task PrintModInfo(Assembly asm, Mod mod, string displayPath)
    {
        TryGetVersion(asm, out var version);
        var desc = mod.Description;
        var descStr = desc != null ? $" | {desc}" : "";
        var authorStr = $" by {mod.Author}";
        await Monitor.SLogAsync($"    {mod.Name}{version}{authorStr}{descStr}");
    }

    private static bool TryLoadModEntry(Assembly modAssembly, [NotNullWhen(true)] out Mod? mod, [NotNullWhen(false)] out string? error)
    {
        mod = null;
        TypeInfo[] modEntries = modAssembly.DefinedTypes
            .Where(type => typeof(Mod).IsAssignableFrom(type) && !type.IsAbstract).Take(2).ToArray();
        if (modEntries.Length == 0)
        {
            error = I18n_.Localize("ModLoader.Error.ModClassNotFound");
            return false;
        }
        if (modEntries.Length > 1)
        {
            error = I18n_.Localize("ModLoader.Error.MultipleModClasses");
            return false;
        }
        string? errorOnInstantiating = null;
        try
        {
            mod = (Mod?)modAssembly.CreateInstance(modEntries[0].ToString());
        }
        catch (Exception e) { errorOnInstantiating = e.Message; }
        if (mod == null)
        {
            error = I18n_.Localize("ModLoader.Error.FailedInstanciatingModClass");
            if (errorOnInstantiating != null)
            {
                error += $":\n{errorOnInstantiating}";
            }
            return false;
        }
        if (!ModRegistry.CanAdd(mod))
        {
            error = I18n_.Localize("ModLoader.Error.DuplicatedModUniqueID", mod.UniqueID);
            return false;
        }
        error = null;
        return true;
    }
}

