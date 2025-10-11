
global using static Sidequel.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using ModdingAPI;

namespace Sidequel;

internal partial class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => null;

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
        State.Setup(helper);
        new Character.Setup(helper);
        new Item.Setup(helper);
        new Dialogue.Setup(helper);
        new System.Setup(helper);
        new World.Setup(helper);
        helper.Events.Gameloop.GameLaunched += (_, _) =>
        {
            RegisterModConfigMenu();
        };
    }
}

