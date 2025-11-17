
global using static Sidequel.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using ModdingAPI;
using Sidequel.Font;

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
        public static string I18nLocalize(string id, params IEnumerable<object> args) => FontSubstituter.Replace(instance.I18n_.Localize(id, args));

        [Conditional("DEBUG")]
        public static void Debug(string m, LL level = LL.Debug) => Monitor.Log(m, level, true);
    }
    public override void Entry(IModHelper helper)
    {
        instance = this;
        State.Setup(helper);
        Cont.Setup(helper);
        Flags.Setup(helper);
        FontSubstituter.Setup(this);
        new Character.Setup(helper);
        new Item.Setup(helper);
        new Dialogue.Setup(helper);
        new System.Setup(helper);
        new World.Setup(helper);
    }
}

