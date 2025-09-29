
namespace BeachstickballPlus;

using System.Reflection;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Volleyball))]
internal class BallPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("OnWhacked")]
    internal static bool OnWhacked(GameObject heldObject, Volleyball __instance)
    {
        if (!ModEntry.config.DoubleBall) return true;
        Holdable component = heldObject.GetComponent<Holdable>();
        if ((bool)component && component.associatedItem == __instance.pickaxeItem)
        {
            __instance.GetType().GetMethod("Pop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
        }
        else if (__instance.controller != null)
        {
            DoubleVolleyball.whackedBall = __instance;
            __instance.controller.OnBallWhackedByPlayer();
        }
        return false;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("OnCollisionEnter")]
    internal static bool OnCollisionEnter(Collision collision, Volleyball __instance)
    {
        if (!ModEntry.config.DoubleBall) return true;
        if (__instance.controller != null && __instance.groundLayers.IncludesLayer(collision.gameObject.layer))
        {
            DoubleVolleyball.OnBallHitsGround(__instance.controller, __instance);
        }
        return false;
    }
}

[HarmonyPatch(typeof(VolleyballGameController))]
internal class VolleyballGameControllerPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("StartGame")]
    internal static bool StartGame(VolleyballGameController __instance, ref IInteractable ___interactable, ref Player ___player, ref int ___hits)
    {
        if (!DoubleVolleyball.Enabled) return true;
        DoubleVolleyball.StartGame(__instance, ref ___interactable, ref ___player, ref ___hits);
        return false;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("EndGame")]
    internal static bool EndGame(bool popped, VolleyballGameController __instance, ref Player ___player)
    {
        if (!DoubleVolleyball.Enabled)
        {
            if (ModEntry.config.SpecialDialogue)
            {
                var hits = Traverse.Create(__instance).Field("hits").GetValue<int>();
                DoubleVolleyball.SetHitCountResult(__instance, hits * 2);
            }
            return true;
        }
        return DoubleVolleyball.EndGame(popped, __instance, ref ___player);
    }

    [HarmonyPrefix()]
    [HarmonyPatch("OnBallWhackedByPlayer")]
    internal static bool OnBallWhackedByPlayer(VolleyballGameController __instance, ref Timer ___hitGroundTimer)
    {
        if (!DoubleVolleyball.Enabled) return true;
        DoubleVolleyball.OnBallWhackedByPlayer(__instance, ref ___hitGroundTimer);
        return false;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("ball", MethodType.Getter)]
    internal static bool GetBall(ref Volleyball __result)
    {
        if (!DoubleVolleyball.EnabledWhackNoise) return true;
        return DoubleVolleyball.GetBall(ref __result);
    }
}

