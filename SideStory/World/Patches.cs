
using HarmonyLib;

namespace SideStory.World;

[HarmonyPatch(typeof(LevelController))]
internal class SavePosPatch
{
    [HarmonyPostfix()]
    [HarmonyPatch("Awake")]
    internal static void Awake()
    {
        if (!State.IsActive) return;
        PlayerPosition.Spawn();
    }
    [HarmonyPrefix()]
    [HarmonyPatch("StoreLastPosition")]
    internal static bool StoreLastPosition() => !State.IsActive;
}

[HarmonyPatch(typeof(SaveRegion))]
internal class SaveRegionPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("OnTriggerEnter")]
    internal static bool Prefix() => !State.IsActive;
}

