
using HarmonyLib;
using ModdingAPI;

namespace Sidequel.System.Race;

[HarmonyPatch(typeof(RaceController))]
internal class RaceControllerPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("BeginRace")]
    internal static void BeforeBeginRace(RaceController __instance)
    {
        Context.globalData.gameData.tags.SetBool(__instance.finishedRaceTag, false);
        Context.globalData.gameData.tags.SetBool(__instance.startedRaceTag, false);
    }
}

[HarmonyPatch(typeof(RaceCoordinator))]
internal class RaceCoordPatch
{
    internal const string LighthouseRaceId = "LighthouseRace";
    [HarmonyPrefix()]
    [HarmonyPatch("PlaceRacer")]
    internal static bool PlaceRacer(RaceData raceData, RaceCoordinator __instance)
    {
        if (!State.IsActive) return true;
        Debug($"PlaceRacer called (raceData.id: {raceData?.id})");
        if (raceData == null || raceData.id != LighthouseRaceId)
        {
            var race = __instance.raceData.Find(race => race.id == LighthouseRaceId);
            if (race != null) __instance.PlaceRacer(race);
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(RaceData))]
internal class RaceDataPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("GetReplayData")]
    internal static bool GetReplayData(RaceData __instance, ref PlayerReplayData __result)
    {
        if (!State.IsActive) return true;
        if (__instance.id == RaceCoordPatch.LighthouseRaceId)
        {
            __result = Deserializer.data;
            return false;
        }
        return true;
    }
}

