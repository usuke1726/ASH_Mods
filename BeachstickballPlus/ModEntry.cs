
global using static BeachstickballPlus.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using ModdingAPI;

namespace BeachstickballPlus;

internal partial class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "Beachstickball with two balls";

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
        helper.Events.Gameloop.GameLaunched += (s, e) => RegisterGenericModConfig();
        DoubleVolleyball.SetI18nMessages();
        helper.Events.System.LocaleChanged += (s, e) => DoubleVolleyball.SetI18nMessages();
    }
}

