
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace SideStory.Item;

[HarmonyPatch(typeof(GlobalData.GameData))]
internal class GameDataPatch
{
    [HarmonyPostfix()]
    [HarmonyPatch("GetAllCollected")]
    internal static void GetAllCollected(ref IEnumerable<CollectableItem> __result)
    {
        if (!State.IsActive) return;
        __result = [.. DataHandler.GetAllCollected()];
    }
    [HarmonyPrefix()]
    [HarmonyPatch("GetCollected")]
    internal static bool GetCollected(CollectableItem item, ref int __result)
    {
        if (!State.IsActive) return true;
        if (DataHandler.Find(item, out var wItem))
        {
            Debug($"Get item as BaseItem: {item.name}");
            __result = DataHandler.GetCollected(wItem);
        }
        else
        {
            Debug($"Get item {item.name}");
            __result = DataHandler.GetCollected(item.name);
        }
        return false;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("AddCollected")]
    internal static bool AddCollected(CollectableItem item, int amount, bool equipAction)
    {
        if (!State.IsActive) return true;
        Debug($"Add item as BaseItem: {item.name}");
        DataHandler.AddCollected(item, amount, equipAction);
        return false;
    }
}

