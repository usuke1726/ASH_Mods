
global using static GenericModConfigMenu.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using IGenericModConfigMenu;
using ModdingAPI;

namespace GenericModConfigMenu;
internal class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "Mod configuration framework";

    private static ModEntry instance = null!;
    internal static class Global
    {
        public static IMonitor Monitor => instance.Monitor;
        public static II18n I18n_ => instance.I18n_;

        [Conditional("DEBUG")]
        public static void Debug(string m) => Monitor.Log(m, LL.Debug);
    }
    public override void Entry(IModHelper helper)
    {
        instance = this;
        helper.Events.Gameloop.GameLaunched += (s, e) => ModConfig.Register(this, api);
    }
    private static readonly IGenericModConfigMenuApi api = new Api();
    public override object? GetApi() => api;
}

