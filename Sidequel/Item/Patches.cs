
using HarmonyLib;
using QuickUnityTools.Input;

namespace Sidequel.Item;

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

[HarmonyPatch(typeof(PauseMenu))]
internal class PauseMenuPatch
{
    private static string fishingRodEscapedName = "___FishingRod";
    private static string fishingRodName = "FishingRod";
    [HarmonyPrefix()]
    [HarmonyPatch("UpdateDescriptionTextForItem")]
    internal static void UpdateDescriptionTextForItem_Prefix(CollectableItem item)
    {
        if (!State.IsActive) return;
        if (item.name == fishingRodName)
        {
            item.name = fishingRodEscapedName;
            if (GameUserInput.sharedActionSet.LastInputType.IsMouseOrKeyboard())
            {
                var state = Data.FishingRodOnKeyboardState;
                var s = state == null ? "" : $"{state}";
                var text = I18n_.Localize($"item.FishingRod{s}.onMouseOrKeyboard.description");
                if (string.IsNullOrEmpty(text)) item.name = fishingRodName;
                else item.description = text;
            }
        }
    }
    [HarmonyPostfix()]
    [HarmonyPatch("UpdateDescriptionTextForItem")]
    internal static void UpdateDescriptionTextForItem_Postfix(CollectableItem item)
    {
        if (!State.IsActive) return;
        if (item.name == fishingRodEscapedName) item.name = fishingRodName;
    }
}

