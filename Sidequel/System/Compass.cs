
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System;

internal class Compass
{
    private static CompassUI ui = null!;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            ui = GameObject.Find("LevelSingletons").transform.Find("UICanvas/UIElements/Compass").GetComponent<CompassUI>();
            OnShowCompassChange(STags.GetBool(Const.STags.ShowCompass));
        };
    }
    internal static void OnShowCompassChange(bool shown)
    {
        ui.gameObject.SetActive(shown);
    }
}

[HarmonyPatch(typeof(CompassItem))]
internal class CompassItemPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("GetMenuActions")]
    internal static bool Prefix(bool held, ref List<ItemAction> __result)
    {
        if (!State.IsActive) return true;
        var shown = STags.GetBool(Const.STags.ShowCompass);
        var name = shown ? I18n.STRINGS.hide : I18n.STRINGS.show;
        __result = [new(name, () => {
            STags.SetBool(Const.STags.ShowCompass, !shown);
            Compass.OnShowCompassChange(shown);
            return true;
        })];
        return false;
    }
}

