
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.World;

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
        Dialogue.DialogueController.instance.StartConversation(null).onConversationFinish += () =>
        {
            ChestInteractableInterval.Create(__instance);
        };
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
    private class ChestInteractableInterval : MonoBehaviour
    {
        private Vector3 position;
        private static RangedInteractable interactable = null!;
        private static ChestInteractableInterval? instance = null;
        private const float Distance = 100f;
        private void Awake()
        {
            position = Context.player.transform.position.SetY(0);
        }
        private void Update()
        {
            var pos = Context.player.transform.position.SetY(0);
            if ((position - pos).sqrMagnitude > Distance) Deactivate();
        }
        private void Deactivate()
        {
            interactable.enabled = true;
            interactable = null!;
            instance = null;
            GameObject.Destroy(gameObject);
        }
        internal static void Create(Chest chest)
        {
            instance?.Deactivate();
            interactable = chest.GetComponent<RangedInteractable>();
            interactable.enabled = false;
            instance = new GameObject("Sidequel_ChestInteractableInterval").AddComponent<ChestInteractableInterval>();
        }
    }
}

