
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System.Fishing;

[HarmonyPatch(typeof(GlobalData.CollectionInventory))]
internal class CollectionInventoryPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("AddFish")]
    internal static bool AddFish(Fish fish)
    {
        if (!State.IsActive) return true;
        Registory.AddFish(fish);
        return false;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("GetAllFish")]
    internal static bool GetAllFish(ref IEnumerable<Fish> __result)
    {
        if (!State.IsActive) return true;
        __result = Registory.GetAllFish();
        return false;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("RemoveFish")]
    internal static bool RemoveFish(Fish fish)
    {
        if (!State.IsActive) return true;
        Registory.RemoveFish(fish);
        return false;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("GetCatchCount")]
    internal static bool GetCatchCount(FishSpecies fishSpecies, ref int __result)
    {
        if (!State.IsActive) return true;
        __result = Registory.GetCatchCount(fishSpecies);
        return false;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("GetBiggestFishRecord")]
    internal static bool GetBiggestFishRecord(FishSpecies fishSpecies, bool rare, ref Fish __result)
    {
        if (!State.IsActive) return true;
        __result = Registory.GetBiggestFishRecord(fishSpecies, rare);
        return false;
    }
}

[HarmonyPatch(typeof(FishItemActions))]
internal class FishItemActionsPatch
{
    private static GameObject encyclopediaObj = null!;
    [HarmonyPrefix()]
    [HarmonyPatch("ShowFishInventoryMenu")]
    internal static bool ShowFishInventoryMenu(Fish fish, CollectionListUIElement element, GameObject fishMenu)
    {
        if (!State.IsActive) return true;
        if (encyclopediaObj == null)
        {
            encyclopediaObj = Item.DataHandler.Find(Items.FishEncyclopedia)?.item.worldPrefab!;
        }
        Assert(encyclopediaObj != null, "encyclopediaObj is null!!");
        if (encyclopediaObj == null) return true;
        var encyclopedia = encyclopediaObj.GetComponent<FishEncyclopedia>();
        LinearMenu menu = null!;
        menu = Context.gameServiceLocator.ui.CreateSimpleMenu([
            I18n.STRINGS.seeNotes,
            I18n.STRINGS.release
        ], [
            () => {
                var obj = encyclopedia.dialogueBoxPrefab.Clone();
                UI.SetGenericText(obj, I18nLocalize($"fishDesc.{FishNames.ToI18nKey(fish)}"));
                Context.gameServiceLocator.ui.AddUI(obj);
                menu.Kill();
            },
            () => {
                Registory.RemoveFish(fish);
                menu.Kill();
                GameObject.Destroy(fishMenu);
            }
        ]);
        element.PositionSimpleMenuAbove(menu.gameObject);
        return false;
    }
}

[HarmonyPatch(typeof(Player))]
internal class UseItemPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("UseItem")]
    internal static bool UseItem(Player __instance)
    {
        if (!State.IsActive || BillScene.RunningCoroutine) return true;
        var heldItem = __instance.heldItem;
        if (heldItem == null) return true;
        if (!heldItem.name.StartsWith("FishingRod")) return true;
        var cooldown = Traverse.Create(__instance).Field("useItemCooldown").GetValue<Timer>();
        if (cooldown != null && !cooldown.IsDone()) return false;
        return BillScene.TryToStart();
    }
}

[HarmonyPatch(typeof(FishingActions))]
internal class FishingActionsPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("CatchFish")]
    internal static void CatchFish()
    {
        if (BillScene.RunningCoroutine) BillScene.OnGotFish();
    }
}
[HarmonyPatch(typeof(Fish))]
internal class FishPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch(MethodType.Constructor, [typeof(FishSpecies), typeof(bool)])]
    internal static bool Prefix(FishSpecies fishSpecies, bool rare, Fish __instance)
    {
        if (!BillScene.RunningCoroutine) return true;
        __instance.species = fishSpecies;
        __instance.size = fishSpecies.size.min;
        __instance.rare = false;
        Traverse.Create(__instance).Field("speciesName").SetValue(fishSpecies.name);
        return false;
    }
}

