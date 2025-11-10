
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

