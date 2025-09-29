
global using static ModdingAPI.ModdingApiMod.Global;
using HarmonyLib;
using ModdingAPI.KeyBind;
using UnityEngine;

namespace ModdingAPI;

public interface IManifest { }
public interface IModInfo
{
    IManifest Manifest { get; }
}
public interface II18n
{
    bool TryToLocalize(string tag, out string s, params IEnumerable<object> args);
    string Localize(string tag, params IEnumerable<object> args);
}
public interface IMod
{
    string Name { get; }
    string Author { get; }
    string UniqueID { get; }
    IModHelper Helper { get; }
    IMonitor Monitor { get; }
    II18n I18n_ { get; }
    IKeyBindingsData KeyBindingsData { get; }
    IManifest ModManifest { get; }
    string HomePath { get; }

    void Entry(IModHelper helper);
    object? GetApi();
    object? GetApi(IModInfo mod);
}

public abstract class Mod : IMod, IDisposable
{
    public abstract string Name { get; }
    public abstract string Author { get; }
    public virtual string? Description { get => null; }
    public string UniqueID { get => $"{Author}.{Name}"; }
    public IModHelper Helper { get; internal set; } = null!;
    public IMonitor Monitor { get; internal set; } = null!;
    public II18n I18n_ { get; internal set; } = null!;
    public IKeyBindingsData KeyBindingsData { get; internal set; } = null!;
    public IManifest ModManifest { get; internal set; } = null!;
    public string HomePath { get; internal set; } = "";

    public Mod()
    {
        Monitor = new Monitor(UniqueID, Name);
        Helper = new Helper(this);
        KeyBindingsData = new KeyBindingsData(UniqueID);
    }

    public abstract void Entry(IModHelper helper);

    public virtual object? GetApi() { return null; }
    public virtual object? GetApi(IModInfo mod) { return null; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public virtual void Dispose(bool disposing) { }

    ~Mod()
    {
        Dispose(false);
    }
}

internal class ModdingApiMod : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    internal static class Global
    {
        public static II18n I18n_ => instance.I18n_;
    }
    public override void Entry(IModHelper helper) { }
    static internal readonly ModdingApiMod instance = new ModdingApiMod();
    public ModdingApiMod() : base()
    {
        HomePath = Path.Combine(BepInEx.Paths.PluginPath, MyPluginInfo.PLUGIN_GUID);
        new Harmony(UniqueID).PatchAll(GetType().Assembly);
        Character.Setup(Helper);
        Helper.Events.Gameloop.ModsLoaded += (_, _) =>
        {
            if (Config.PrintKeyBindListOnGameLaunched)
            {
                KeyBind.KeyBind.ListKeyBinds();
            }
        };
        Helper.Events.Gameloop.BeforeTitleScreenUpdated += (s, e) =>
        {
            InputInterceptor.isInPrefix = true;
            InputInterceptor.Update();
            KeyWatcher.Update();
            InputInterceptor.isInPrefix = false;
        };
        Helper.Events.Gameloop.BeforePlayerUpdated += (s, e) =>
        {
            Context.UpdateCanPlayerMove();
            InputInterceptor.isInPrefix = true;
            InputInterceptor.Update();
            KeyWatcher.Update();
            FPSCounterPatch.Update();
            InputInterceptor.isInPrefix = false;
        };
        Helper.Events.Gameloop.ReturnedToTitle += (s, e) =>
        {
        };
    }
    internal void SetKeyBinds()
    {
        Dictionary<string, string> defaultKeyBinds = new()
        {
            [KeyBindsKey.StartDebugKeyWatcher] = "Escape(F12) F11"
        };
        KeyBindingsData.SetDefault(defaultKeyBinds);
        KeyBind.KeyBind.RegisterKeyBind(KeyBindingsData, KeyBindsKey.StartDebugKeyWatcher, ButtonMap.StartDebugKeyWatcher, name: "StartDebugKeyWatcher", allowDefault: false);
    }
}

internal static class KeyBindsKey
{
    public const string StartDebugKeyWatcher = "StartDebugKeyWatcher";
}

