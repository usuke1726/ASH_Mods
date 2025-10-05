
using IGenericModConfigMenu;
using ModdingAPI;
using SideStory.System;

namespace SideStory;

partial class ModEntry
{
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
            name: () => I18n_.Localize("ModConfigMenu.continue")
        );
        configMenu.AddAction(
            mod: this,
            action: () => NewGameController.StartGame(),
            closeMenu: true,
            beforeClose: action => TryToStart(action, true),
            name: () => I18n_.Localize("ModConfigMenu.newgame")
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

