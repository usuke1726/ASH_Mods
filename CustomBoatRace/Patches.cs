
using HarmonyLib;
using ModdingAPI;

namespace CustomBoatRace;

[HarmonyPatch(typeof(BoatScripting))]
internal class BoatRacePatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("StartChallenge")]
    internal static bool StartChallenge(BoatScripting __instance)
    {
        if (!HasBoatFixEventFinished()) return true;
        var config = ModEntry.config;
        CustomBoatRace.SetCourse(config.CourseId, __instance);
        if (!config.Enabled || !CustomBoatRace.Enabled) return true;
        __instance.StartCoroutine(CustomBoatRace.ChallengeCoroutine(__instance));
        return false;
    }
    private static bool HasBoatFixEventFinished()
    {
        return Context.globalData.gameData.tags.GetBool("BoatIsFixed");
    }
}

[HarmonyPatch(typeof(BoatCameraTarget))]
internal class BoatCameraTargetPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("OnDestroy")]
    internal static bool OnDestroy() => !Context.IsQuitting;
}

