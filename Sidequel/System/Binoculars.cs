
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System;

internal class Binoculars : MonoBehaviour
{
    private static Binoculars? instance = null;
    internal const string ObjectName = "Sidequel_Binoculars";
    internal class BinocularsItem : MonoBehaviour, IActionableItem
    {
        public List<ItemAction> GetMenuActions(bool held)
        {
            return [new(I18nLocalize("item.Binoculars.action"), () => {
                TryToActivate();
                return true;
            })];
        }
        internal static GameObject CreateWorldPrefab()
        {
            var obj = new GameObject("BinocularItemPrefab");
            obj.AddComponent<BinocularsItem>();
            return obj;
        }
    }
    private static void TryToActivate()
    {
        if (instance == null) return;
        if (instance.active) return;
        var player = Context.player;
        if (player.isGrounded || player.isSwimming)
        {
            instance.Activate();
        }
        else
        {
            NodeData.Binoculars.activated = true;
            Dialogue.DialogueController.instance.StartConversation(null);
        }
    }
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            var towerViewer = GameObject.Find("/LevelObjects/Tools").transform.Find("TowerViewer");
            Assert(towerViewer != null, "towerViewer is null!");
            if (towerViewer == null) return;
            instance = towerViewer.gameObject.Clone().AddComponent<Binoculars>();
        };
        helper.Events.Gameloop.GameLaunched += (_, _) => instance = null;
    }
    internal static void NoticeDisabled() => instance?.Deactivate();

    private TowerViewer viewer = null!;
    private Traverse offsetAngle = null!;
    private bool active = false;
    private const float YOffset = 3f;
    private Vector2 OffsetAngle { get => offsetAngle.GetValue<Vector2>(); set => offsetAngle.SetValue(value); }
    private void Awake()
    {
        gameObject.name = ObjectName;
        transform.Find("TowerViewerModel").gameObject.SetActive(false);
        GetComponent<RangedInteractable>().enabled = false;
        viewer = GetComponent<TowerViewer>();
        Traverse.Create(viewer).Field("originalNearPlane").SetValue(1f);
        viewer.minOffsetAngle = new(-360, -360);
        viewer.maxOffsetAngle = new(360, 360);
        offsetAngle = Traverse.Create(viewer).Field("offsetAngle");
    }
    private void Activate()
    {
        var player = Context.player;
        var waterRegion = player.waterRegion;
        if (waterRegion != null) player.UnregisterWaterRegion(waterRegion);
        transform.position = player.transform.position + Vector3.up * YOffset;
        var rotY = player.transform.localRotation.eulerAngles.y;
        var angle = rotY - 200;
        if (angle < -180) angle += 360;
        if (angle > 180) angle -= 360;
        OffsetAngle = new(angle, 0);
        player.gameObject.SetActive(false);
        viewer.Interact();
        active = true;
    }
    private void Update()
    {
        if (!active) return;
        var angle = OffsetAngle;
        if (angle.x < -180) angle.x += 360;
        if (angle.x > 180) angle.x -= 360;
        if (angle.y < -180) angle.y += 360;
        if (angle.y > 180) angle.y -= 360;
        OffsetAngle = angle;
    }
    private void Deactivate()
    {
        active = false;
        Context.player.gameObject.SetActive(true);
    }
}

[HarmonyPatch(typeof(TowerViewer))]
internal class TowerViwerPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Disable")]
    internal static void PreDisable(TowerViewer __instance)
    {
        if (__instance.gameObject.name != Binoculars.ObjectName) return;
        Binoculars.NoticeDisabled();
    }
}

