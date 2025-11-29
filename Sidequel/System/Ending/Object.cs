
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System.Ending;

using FI = QuickUnityTools.Input.FocusableUserInput;

internal class Object
{
    internal const string DummyName = "Sidequel_Ending_DummyPlayer";
    internal static EndingPlayerReplay ActivePlayerReplay { get; private set; } = null!;
    internal static GameObject Create()
    {
        EndingPlayerInputPatch.isCreatingDummyObject = true;
        var player = Context.player.gameObject.Clone();
        player.name = DummyName;
        ActivePlayerReplay = player.AddComponent<EndingPlayerReplay>();
        ActivePlayerReplay.data = new() { frames = [.. Data.data1.Select(Race.Deserializer.DeserializeFrame)] };
        ActivePlayerReplay.SetPauseFrames(Data.pauseFrames1);
        var firstFrame = ActivePlayerReplay.data.frames[0];
        Context.player.transform.position = firstFrame.position + Vector3.left * 4;
        player.transform.position = firstFrame.position;
        player.transform.rotation = firstFrame.rotation;
        Context.player.gameObject.SetActive(false);
        return player;
    }
    internal static void DebugStartTestReplay()
    {
        ActivePlayerReplay.data = Race.Deserializer.data;
        ActivePlayerReplay.Play();
    }
}

[HarmonyPatch(typeof(Player.PlayerInput))]
internal class EndingPlayerInputPatch
{
    internal static bool isCreatingDummyObject = false;
    [HarmonyPrefix()]
    [HarmonyPatch("PlayerInput", MethodType.Constructor)]
    internal static bool Prefix()
    {
        if (isCreatingDummyObject)
        {
            isCreatingDummyObject = false;
            return false;
        }
        return true;
    }

    private static bool Vec2Patch(ref Vector2 result, ref FI input)
    {
        if (input == null) { result = Vector2.zero; return false; }
        return true;
    }
    private static bool BoolPatch(ref bool result, ref FI input)
    {
        if (input == null) { result = false; return false; }
        return true;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("GetMovement")]
    internal static bool GetMovement(ref Vector2 __result, ref FI ___input) => Vec2Patch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("GetLookDirection")]
    internal static bool GetLookDirection(ref Vector2 __result, ref FI ___input) => Vec2Patch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("hasFocus", MethodType.Getter)]
    internal static bool HasFocus(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsJumpHeld")]
    public static bool IsJumpHeld(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsJumpTapped")]
    public static bool IsJumpTapped(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsMenuTapped")]
    public static bool IsMenuTapped(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("ConsumeInteractTapped")]
    public static bool ConsumeInteractTapped(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("ConsumeUseItemTapped")]
    public static bool ConsumeUseItemTapped(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsInteractHeld")]
    public static bool IsInteractHeld(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsUseItemHeld")]
    public static bool IsUseItemHeld(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsRunHeld")]
    public static bool IsRunHeld(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);

    [HarmonyPrefix()]
    [HarmonyPatch("IsDropTapped")]
    public static bool IsDropTapped(ref bool __result, ref FI ___input) => BoolPatch(ref __result, ref ___input);
}


[HarmonyPatch(typeof(Player))]
internal class EndingPlayerPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Update")]
    internal static bool PlayerUpdate(Player __instance) => __instance.name != Object.DummyName;

    [HarmonyPrefix()]
    [HarmonyPatch("isSwimming", MethodType.Getter)]
    internal static bool IsSwimming(Player __instance, ref bool __result) => Main(__instance, ref __result, r => r.isSwimming);

    [HarmonyPrefix()]
    [HarmonyPatch("isRunning", MethodType.Getter)]
    internal static bool IsRunning(Player __instance, ref bool __result) => Main(__instance, ref __result, r => r.isRunning);

    [HarmonyPrefix()]
    [HarmonyPatch("isSliding", MethodType.Getter)]
    internal static bool IsSliding(Player __instance, ref bool __result) => Main(__instance, ref __result, r => r.isSliding);

    [HarmonyPrefix()]
    [HarmonyPatch("isGliding", MethodType.Getter)]
    internal static bool IsGliding(Player __instance, ref bool __result) => Main(__instance, ref __result, r => r.isGliding);

    [HarmonyPrefix()]
    [HarmonyPatch("isClimbing", MethodType.Getter)]
    internal static bool IsClimbing(Player __instance, ref bool __result) => Main(__instance, ref __result, r => r.isClimbing);

    private static bool Main(Player instance, ref bool result, Func<EndingPlayerReplay, bool> retriever)
    {
        if (instance.name != Object.DummyName) return true;
        result = retriever(Object.ActivePlayerReplay);
        return false;
    }
}

