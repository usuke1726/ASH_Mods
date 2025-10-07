
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

[HarmonyPatch(typeof(Chest))]
internal class ChestPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Interact")]
    public static bool Interact(Chest __instance)
    {
        if (!State.IsActive) return true;
        var id = __instance.GetComponent<GameObjectID>().id;
        NodeData.Chest.OnChestInteracted(id);
        Dialogue.DialogueController.instance.StartConversation(null);
        return false;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("opened", MethodType.Setter)]
    public static bool Start(bool value, Chest __instance)
    {
        if (!State.IsActive || value) return true;
        //Debug($"prevent closing chest (id: {__instance.GetComponent<GameObjectID>().id})");
        return false;
    }
}

