
global using static ClearView.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using ModdingAPI;

namespace ClearView;
internal partial class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "Remove fogs and clouds, visualize far objects";

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
        Helper.Events.Gameloop.GameLaunched += (s, e) =>
        {
            RegisterGenericModConfig();
        };
        Helper.Events.Gameloop.PlayerUpdated += (s, e) => Update();
        Helper.Events.Gameloop.ReturnedToTitle += (s, e) => Reset();
    }
}

