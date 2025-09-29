
global using static CustomConversation.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using CustomConversation.test;
using ModdingAPI;

namespace CustomConversation;
internal class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "A simple conversation system";

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
        TestConversation.Setup(this);
        CharacterObject.Setup(helper);
    }
    public override object? GetApi() => SpecialConversationApi.instance;
}

