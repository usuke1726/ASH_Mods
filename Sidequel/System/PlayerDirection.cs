
using HarmonyLib;
using ModdingAPI;
using QuickUnityTools.Input;
using UnityEngine;

namespace Sidequel.System;

[HarmonyPatch(typeof(Player))]
internal class TurnToFacePatch
{
    private static Vector3 previousFaceToward;
    [HarmonyPrefix()]
    [HarmonyPatch("TurnToFace", [typeof(Transform)])]
    internal static void TurnToFace(Transform target, Player __instance)
    {
        if (!State.IsActive) return;
        previousFaceToward = __instance.transform.position + __instance.transform.forward.normalized * 10;
    }
    internal static void ResetFaceDirection()
    {
        Context.player.TurnToFace(previousFaceToward);
    }
}

[HarmonyPatch(typeof(ItemPrompt))]
internal class ItemPromptPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Update")]
    internal static bool OnUpdate(ItemPrompt __instance)
    {
        if (!State.IsActive) return true;
        var input = Traverse.Create(__instance).Field("input").GetValue<FocusableUserInput>();
        if (input.WasDismissPressed())
        {
            TurnToFacePatch.ResetFaceDirection();
            GameObject.Destroy(__instance.gameObject);
        }
        return false;
    }
}

