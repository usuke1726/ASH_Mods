
global using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using ModdingAPI.Events;
using ModdingAPI.KeyBind;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModdingAPI;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Setup();
        Application.quitting += async () =>
        {
            Logger.LogInfo("quitting...");
            await MonitorServer.Deactivate();
        };
        SceneManager.sceneLoaded += SceneLoaded;
        I18n.onLanguageChanged += () => ModRegistry.OnLanguageChanged();
    }
    private async void Setup()
    {
        await ModdingAPI.Config.ReadConfig(Logger);
        ModdingApiMod.instance.I18n_ = new API_I18n(ModdingApiMod.instance);
        MonitorServer.Setup(Logger);
        ButtonMap.Setup();
        KeyBindingsData.ReadData();
        ModdingApiMod.instance.SetKeyBinds();
        ModLoader.LoadMods();
    }
    private bool initialized = false;
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Context.UpdateScene(scene);
        if (scene.name == Context.Scene._TitleScene)
        {
            if (!initialized)
            {
                GameloopEvents.OnGameLaunched();
            }
            else
            {
                Context.IsQuitting = false;
                Player_Update.isFirstCall = true;
                GameloopEvents.OnReturnedToTitle();
            }
            initialized = true;
        }
        else if (scene.name == Context.Scene._CreditsScene)
        {
            GameloopEvents.OnCreditsStarted();
        }
    }
}

[HarmonyPatch(typeof(TitleScreen), "Update")]
internal class TitleScreen_Update
{
    [HarmonyPrefix()]
    public static void Prefix(TitleScreen __instance)
    {
        if (!Context.OnTitle) return;
        GameloopEvents.OnBeforeTitleScreenUpdated(__instance);
    }
    [HarmonyPostfix()]
    public static void Postfix(TitleScreen __instance)
    {
        if (!Context.OnTitle) return;
        GameloopEvents.OnTitleScreenUpdated(__instance);
    }
}
[HarmonyPatch(typeof(Player), "Update")]
internal class Player_Update
{
    internal static bool isFirstCall = true;
    [HarmonyPrefix()]
    public static void Prefix(Player __instance)
    {
        if (!Context.GameStarted || Context.IsQuitting) return;
        if (Context.player.GetHashCode() != __instance.GetHashCode()) return;
        if (isFirstCall)
        {
            GameloopEvents.OnGameStarted();
            isFirstCall = false;
        }
        GameloopEvents.OnBeforePlayerUpdated(__instance);
    }
    [HarmonyPostfix()]
    public static void Postfix(Player __instance)
    {
        if (!Context.GameStarted || Context.IsQuitting) return;
        if (Context.player.GetHashCode() != __instance.GetHashCode()) return;
        GameloopEvents.OnPlayerUpdated(__instance);
    }
}

[HarmonyPatch(typeof(PauseMenu))]
internal class PauseMenuPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Quit")]
    internal static void Quit()
    {
        Context.IsQuitting = true;
        GameloopEvents.OnGameQuitting();
    }
}

