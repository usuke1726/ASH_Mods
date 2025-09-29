
global using static CoinChallenge.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using CoinChallenge.Samples;
using IGenericModConfigMenu;
using ModdingAPI;

namespace CoinChallenge;
internal class ModEntry : Mod
{
    public override string Name => MyPluginInfo.PLUGIN_GUID;
    public override string Author => "Quicker1726";
    public override string? Description => "A minigame collecting coins on randomly-generated map";

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
        SpecialCoin.Setup(helper);
        GameController.Setup(helper);
        helper.Events.Gameloop.GameLaunched += (_, _) => RegisterModConfig();
    }
    private class DummyConfig() { }
    private void RegisterModConfig()
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(Info.ModID);
        if (configMenu is null) return;
        configMenu.Register(
            mod: this,
            reset: () => { },
            import: c => { },
            export: () => new DummyConfig()
        );
        configMenu.AddAction(
            mod: this,
            name: () => "start Map1",
            closeMenu: true,
            action: () => GameController.Start(new Map1()),
            beforeClose: action =>
            {
                if (!Context.GameStarted)
                {
                    MainMenu.ShowDialog("Start the game\nbefore running this command!");
                    return;
                }
                MainMenu.ClosePauseMenu();
                action();
            }
        );
        configMenu.AddAction(
            mod: this,
            name: () => "Give up race",
            closeMenu: true,
            action: () => GameController.ConfirmToAbandon(),
            beforeClose: action =>
            {
                if (!GameController.IsRaceActive)
                {
                    MainMenu.ShowDialog("This command is only\navailable in the race");
                    return;
                }
                MainMenu.ClosePauseMenu();
                action();
            }
        );
    }
}

