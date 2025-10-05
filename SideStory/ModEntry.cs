
global using static SideStory.ModEntry.Global;
global using LL = BepInEx.Logging.LogLevel;
using IGenericModConfigMenu;
using ModdingAPI;
using SideStory.Item;
using SideStory.System;

namespace SideStory;

internal class ModEntry : Mod
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
        Character.Core.Setup(helper);
        DataHandler.Setup(helper);
        new Dialogue.Setup(helper);
        new System.Setup(helper);
        helper.Events.Gameloop.GameLaunched += (_, _) =>
        {
            RegisterModConfigMenu();
        };
    }
    private class DummyConfig() { }
    private void RegisterModConfigMenu()
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(Info.ModID);
        if (configMenu is null) return;
        configMenu.Register(
            mod: this,
            reset: () => { },
            import: _ => { },
            export: () => new DummyConfig(),
            displayName: "SideStory"
        );
        configMenu.AddAction(
            mod: this,
            action: () => NewGameController.StartGame(),
            closeMenu: true,
            beforeClose: action => TryToStart(action, false),
            condition: () => SaveData.DoesSaveDataExists(),
            name: () => "Continue"
        );
        configMenu.AddAction(
            mod: this,
            action: () => NewGameController.StartGame(),
            closeMenu: true,
            beforeClose: action => TryToStart(action, true),
            name: () => "New game"
        );
    }
    private void TryToStart(Action action, bool isNewGame)
    {
        if (!Context.OnTitle)
        {
            MainMenu.ShowDialog(I18n_.Localize("ModConfigMenu.triedToStartOutsideMainMenu"));
            return;
        }
        if (!CrossPlatform.DoesSaveExist())
        {
            MainMenu.ShowDialog(I18n_.Localize("ModConfigMenu.notCreatedSaveData"));
            return;
        }
        if (isNewGame) State.SetNewGame();
        if (isNewGame && SaveData.DoesSaveDataExists())
        {
            MainMenu.ShowConfirm(
                I18n_.Localize("ModConfigMenu.confirmOverwritingSaveData"),
                I18n_.Localize("ModConfigMenu.confirmOverwritingSaveData.y"),
                I18n_.Localize("ModConfigMenu.confirmOverwritingSaveData.n"),
                action);
        }
        else
        {
            action();
        }
    }
}

