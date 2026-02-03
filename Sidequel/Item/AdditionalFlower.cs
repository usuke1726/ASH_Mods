
using System.Collections;
using HarmonyLib;
using ModdingAPI;
using QuickUnityTools.Input;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.Item;

internal class AdditionalFlower
{
    private const string WateredTag = "Sidequel_AdditionalSaplingsWatered";
    private const string PositionsTag = "Sidequel_AdditionalSaplingsPositions";
    private static Transform prefab = null!;
    private static int count = 0;
    private static readonly Dictionary<int, bool> watered = [];
    private static readonly Dictionary<int, Vector3> positions = [];
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            SetFlowerPrefab();
            Load();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            watered.Clear();
            positions.Clear();
            count = 0;
            prefab = null!;
        };
    }
    internal static bool Watered(int index) => watered.TryGetValue(index, out bool result) && result;
    internal static void SetWatered(int index)
    {
        //Debug($"AdditionalFlower_{index} WATERED", LL.Warning);
        watered[index] = true;
        Save();
    }
    internal static GameObject CreateWorldPrefab()
    {
        var obj = new GameObject("Sidequel_AdditionalFlowerSaplingItemPrefab");
        obj.AddComponent<FlowerSaplingItem>();
        return obj;
    }
    private class FlowerSaplingItem : MonoBehaviour, IActionableItem
    {
        public List<ItemAction> GetMenuActions(bool held)
        {
            return [new(I18nLocalize("item.RubberFlowerSapling.action"), () => {
                PlantingCoroutine.StartPlanting();
                return true;
            })];
        }
    }
    private static void SetFlowerPrefab()
    {
        prefab = GameObject.Find("LevelObjects/Tools").transform.Find("SaplingFlower (2)");
        Assert(prefab != null, "Not found the object \"SaplingFlower (2)\"");
    }
    private class PlantingCoroutine : MonoBehaviour
    {
        private static PlantingCoroutine? instance = null;
        private static bool isActive = false;
        private static bool CannotPlant(Player player)
        {
            return (
                !player.isGrounded ||
                player.isSwimming ||
                player.isClimbing ||
                player.isGliding ||
                player.isSliding
            );
        }
        private static bool ShouldNotPlant(Player player)
        {
            return (
                player.transform.position.y >= 600f
            );
        }
        private IEnumerator Plant()
        {
            var player = Context.player;
            var inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
            yield return new WaitForSeconds(0.25f);
            if (CannotPlant(player))
            {
                NodeData.RubberFlowerSapling.cannnotPlantActivated = true;
                Dialogue.DialogueController.instance.StartConversation(null);
            }
            else if (ShouldNotPlant(player))
            {
                NodeData.RubberFlowerSapling.shouldNotPlantActivated = true;
                Dialogue.DialogueController.instance.StartConversation(null);
            }
            else
            {
                player.body.velocity = Vector3.zero;
                if (DataHandler.Find(Items.RubberFlowerSapling, out var saplingItem))
                {
                    DataHandler.AddCollected(saplingItem, -1);
                    Context.levelUI.statusBar.ShowCollection(saplingItem.item).HideAndKill(1f);
                }
                var stopPose = player.ikAnimator.Pose(Pose.LookingDown);
                yield return new WaitForSeconds(0.75f);
                Clone(player);
                stopPose();
                yield return new WaitForSeconds(0.75f);
            }
            GameObject.Destroy(inputLock);
            isActive = false;
        }
        private void OnDestroy() => instance = null;
        internal static void StartPlanting()
        {
            if (isActive) return;
            instance ??= new GameObject("Sidequel_AdditionalFlower_PlantingController").AddComponent<PlantingCoroutine>();
            isActive = true;
            instance.StartCoroutine(instance.Plant());
        }
    }
    private static void Clone(int idx, Vector3 position)
    {
        var newObj = prefab.gameObject.Clone();
        newObj.name = $"{SaplingBoolTagPatcher.SaplingIDPrefix}{idx}";
        newObj.GetComponent<GameObjectID>().id = $"{SaplingBoolTagPatcher.SaplingIDPrefix}{idx}";
        newObj.transform.position = position;
        newObj.GetComponent<SphereCollider>().enabled = !watered[idx];
    }
    private static void Clone(Player player)
    {
        var idx = ++count;
        var position = player.transform.position + 1.5f * Vector3.up;
        watered[idx] = false;
        positions[idx] = position;
        Clone(idx, position);
        //Debug($"planted sapling index {idx}", LL.Warning);
        Save();
    }
    private static void Load()
    {
        if (STags.TryGetString(WateredTag, out var wateredData) && STags.TryGetString(PositionsTag, out var positionsData))
        {
            DeserializeWatered(wateredData);
            DeserializePositions(positionsData);
            SetupPlantedFlowers();
        }
    }
    private static void Save()
    {
        STags.SetString(WateredTag, SerializeWatered());
        STags.SetString(PositionsTag, SerializePositions());
    }
    private static string SerializeWatered() => string.Join(";", watered.Select(pair => $"{pair.Key}:{(pair.Value ? "1" : "0")}"));
    private static void SetupPlantedFlowers()
    {
        var indexes = positions.Keys.ToArray();
        int maxIdx = 0;
        foreach (var idx in indexes)
        {
            if (watered.ContainsKey(idx))
            {
                maxIdx = Math.Max(maxIdx, idx);
                Clone(idx, positions[idx]);
            }
            else positions.Remove(idx);
        }
        count = maxIdx;
    }
    private static void DeserializeWatered(string data)
    {
        watered.Clear();
        foreach (var val in data.Split(";"))
        {
            var d = val.Split(":");
            if (d.Length != 2) continue;
            if (int.TryParse(d[0], out var idx))
            {
                watered[idx] = d[1] == "1";
            }
        }
    }
    private static string SerializePositions()
    {
        return string.Join(";", positions.Select(pair =>
        {
            var idx = pair.Key;
            var pos = pair.Value;
            return $"{idx}:{pos.x},{pos.y},{pos.z}";
        }));
    }
    private static void DeserializePositions(string data)
    {
        positions.Clear();
        foreach (var val in data.Split(";"))
        {
            var d = val.Split(":");
            if (d.Length != 2) continue;
            var pos = d[1].Split(",");
            if (pos.Length != 3) continue;
            if (
                int.TryParse(d[0], out var idx) &&
                float.TryParse(pos[0], out var x) &&
                float.TryParse(pos[1], out var y) &&
                float.TryParse(pos[2], out var z)
            )
            {
                positions[idx] = new(x, y, z);
            }
        }
    }
}

[HarmonyPatch(typeof(GameObjectID))]
internal class SaplingBoolTagPatcher
{
    internal const string SaplingIDPrefix = "SidequelSapling_";
    [HarmonyPrefix()]
    [HarmonyPatch("GetBoolForID")]
    internal static bool GetBoolForID(string prefix, GameObjectID __instance, ref bool __result)
    {
        if (IsAdditionalFlower(__instance.id, out var idx))
        {
            __result = AdditionalFlower.Watered(idx);
            return false;
        }
        return true;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("SaveBoolForID")]
    internal static bool SaveBoolForID(string prefix, bool value, GameObjectID __instance)
    {
        if (IsAdditionalFlower(__instance.id, out var idx))
        {
            AdditionalFlower.SetWatered(idx);
            return false;
        }
        return true;
    }
    private static bool IsAdditionalFlower(string id, out int idx)
    {
        idx = -1;
        if (id.StartsWith(SaplingIDPrefix))
        {
            int.TryParse(id.Split("_")[1], out idx);
            return true;
        }
        return false;
    }
}

