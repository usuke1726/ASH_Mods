
using ModdingAPI;
using ModdingAPI.KeyBind;

namespace Misc;
internal class Util
{
    internal static void Setup(IModHelper helper)
    {
        KeyBind.RegisterKeyBind(helper.KeyBindingsData, KeybindKey.ToggleFPS, ToggleFPS, name: "ToggleFPS");
        KeyBind.RegisterKeyBind(helper.KeyBindingsData, KeybindKey.ToggleCinemaCamera, ToggleCamera, name: "cinemaplz");
        KeyBind.RegisterKeyBind(helper.KeyBindingsData, KeybindKey.ToggleHideUI, ToggleUI, name: "ToggleUI");
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => isUIActive = true;
    }
    private static void ToggleFPS()
    {
        GameSettings.showFPS = !GameSettings.showFPS;
        Monitor.Log($"FPS {(GameSettings.showFPS ? "enabled" : "disabled")}");
    }
    private static void ToggleCamera()
    {
        if (!Context.GameStarted) return;
        var cinemaCamera = Context.levelController.cinemaCamera;
        cinemaCamera.SetActive(!cinemaCamera.activeSelf);
        Monitor.Log($"Cinema {(cinemaCamera.activeSelf ? "on" : "off")}");
    }
    private static bool isUIActive = true;
    private static void ToggleUI()
    {
        if (!Context.GameStarted) return;
        isUIActive = !isUIActive;
        Context.levelUI.HideUI(hidden: !isUIActive);
        Monitor.Log($"UI {(isUIActive ? "visible" : "invisible")}");
    }
}

