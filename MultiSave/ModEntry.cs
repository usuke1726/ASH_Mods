
global using static MultiSave.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using ModdingAPI;

namespace MultiSave;

internal class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "Enable multisave and more saveslots on Xbox PC.";

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
        helper.Events.Gameloop.GameLaunched += (_, _) =>
        {
            UpdateTimeHandler.Setup(this);
        };
        helper.Events.System.BeforeSaving += (_, e) =>
        {
            var now = DateTime.Now;
            UpdateTimeHandler.Update(e.SaveSlot, now);
            Debug($"== now saved! ({now.ToString(I18n.STRINGS.dateFormat)})");
            Debug($"slot: {e.SaveSlot}");
        };
        MainMenu.AddMenuItem(new(
            () => I18n.STRINGS.saveData,
            menu => () => SaveMenu.ShowSaveMenu(menu)
        ), -1);
    }
}

