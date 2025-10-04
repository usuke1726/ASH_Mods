
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
        if (item is ExtendedItem eItem)
        {
            __result = DataHandler.GetCollected(eItem);
            return false;
        }
        else if (State.IsActive && item.saveTag == "ITEM_GoldenFeather")
        {
            __result = DataHandler.GetCollected("GoldenFeather");
            return false;
        }
        return true;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("AddCollected")]
    internal static bool AddCollected(CollectableItem item, int amount, bool equipAction)
    {
        if (item is ExtendedItem eItem)
        {
            DataHandler.AddCollected(eItem, amount, equipAction);
            return false;
        }
        return true;
    }
}

