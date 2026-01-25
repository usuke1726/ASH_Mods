
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
    internal const string OldBuildingRaceId = "OldBuildingRace";
    internal const string MountainTopRaceId = "MountainTopRace";
    [HarmonyPrefix()]
    [HarmonyPatch("PlaceRacer")]
    internal static bool PlaceRacer(RaceData raceData, RaceCoordinator __instance)
    {
        if (!State.IsActive) return true;
        if (NodeData.WalkieTalkieEntry.IsAtStartPosition) return false;
        if (raceData == null || raceData.id != LighthouseRaceId)
        {
            var race = __instance.raceData.Find(race => race.id == LighthouseRaceId);
            if (race != null) __instance.PlaceRacer(race);
            return false;
        }
        Debug($"Moving Avery to the race starting position");
        return true;
    }
    internal static void SetupCoordinator(RaceCoordinator instance)
    {
        if (!State.IsActive) return;
        foreach (var d in instance.raceData) d.requireTag = d.id != MountainTopRaceId ? "" : "Sidequel_AlwaysFalseTag";
        instance.PlaceRacer(null);
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

