
global using static Misc.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using ModdingAPI;

namespace Misc;

internal static class KeybindKey
{
    internal const string SuperJump_Keyboard = "SuperJump_Keyboard";
    internal const string SuperJump_Pad = "SuperJump_Pad";
    internal const string ToggleFPS = "ToggleFPS";
    internal const string ToggleCinemaCamera = "ToggleCinemaCamera";
    internal const string ToggleHideUI = "ToggleHideUI";
}

internal class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "Miscellaneous features and cheats";

    private static ModEntry instance = null!;
    internal static class Global
    {
        public static IMonitor Monitor => instance.Monitor;
        public static II18n I18n_ => instance.I18n_;

        [Conditional("DEBUG")]
        public static void Debug(string m) => Monitor.Log(m, LL.Debug);
    }
    internal static Dictionary<string, string> defaultKeybinds = new()
    {
        [KeybindKey.SuperJump_Keyboard] = ":b1(P)",
        [KeybindKey.SuperJump_Pad] = ":b1(JoystickButton5)",
        [KeybindKey.ToggleFPS] = "F(LeftControl+LeftAlt)",
        [KeybindKey.ToggleCinemaCamera] = "C(LeftControl+LeftAlt)",
        [KeybindKey.ToggleHideUI] = "U(LeftControl+LeftAlt)",
    };
    public override void Entry(IModHelper helper)
    {
        instance = this;
        KeyBindingsData.SetDefault(defaultKeybinds);
        ModConfig.Setup(this);
        TurboClaire.Setup(helper);
        SuperJump.Setup(helper);
        Util.Setup(helper);
    }
}

