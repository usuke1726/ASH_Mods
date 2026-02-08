
using HarmonyLib;

namespace Sidequel.Font;

[HarmonyPatch(typeof(ItemPrompt))]
internal class ItemPromptFontPatch
{
    [HarmonyPostfix()]
    [HarmonyPatch("Setup")]
    internal static void Patch(CollectableItem item, ItemPrompt __instance)
    {
        if (!State.IsActive) return;
        __instance.itemName.text = FontSubstituter.ReverseReplace(__instance.itemName.text);
    }
}

