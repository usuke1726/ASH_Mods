
using System.Collections;
using HarmonyLib;
using ModdingAPI;
using QuickUnityTools.Input;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.Item;

internal class AdditionalFlower
{
    private const string AdditionalFlowerDataTag = "Sidequel_AdditionalFlowerData";
    private static Transform container = null!;
    private static Transform prefab = null!;
    private static int count = 0;
    private static readonly Dictionary<int, bool> watered = [];
    private static readonly Dictionary<int, Vector3> positions = [];
    private static readonly Dictionary<int, Transform> objects = [];
    private static HashSet<string> removedFlowerIDs = [];
    private static HashSet<string> originalFlowerShortenIds = [];
    private static Dictionary<string, Transform> originalFlowers = [];
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            container = new GameObject("Sidequel_AdditionalFlowers").transform;
            SetFlowerPrefab();
            Load();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            watered.Clear();
            positions.Clear();
            objects.Clear();
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
    private static bool IsThereFlowerNearby(out Transform obj, out int? index, out bool existsBitFarAway, bool isRemoving = false)
    {
        float maxSqrDistance = 35f;
        float maxSqrDistance2 = 60f;
        float maxSqrDistance3 = 1000f;
        var playerPos = Context.player.transform.position;
        var minV = objects.Select(pair =>
        {
            var d = (playerPos - pair.Value.position).sqrMagnitude;
            if (isRemoving && !watered[pair.Key]) d -= 10f;
            return new Tuple<float, int?, Transform>(d, pair.Key, pair.Value);
        }).Concat(originalFlowers.Values.Select(flower =>
        {
            var d = (playerPos - flower.position).sqrMagnitude;
            return new Tuple<float, int?, Transform>(d, null, flower);
        })).MinValue(c => c.Item1);
        obj = minV.Item3;
        index = minV.Item2;
        existsBitFarAway = minV.Item1 < maxSqrDistance3;
        return minV.Item1 < (isRemoving ? maxSqrDistance2 : maxSqrDistance);
    }
    private class FlowerSaplingItem : MonoBehaviour, IActionableItem
    {
        public List<ItemAction> GetMenuActions(bool held)
        {
            return [new(I18nLocalize("item.RubberFlowerSapling.action"), () => {
                PlantingCoroutine.StartPlanting();
                return true;
            }), new(I18nLocalize("item.RubberFlowerSapling.remove"), () => {
                PlantingCoroutine.StartRemoving();
                return true;
            })];
        }
    }
    private static void SetFlowerPrefab()
    {
        var tools = GameObject.Find("/LevelObjects/Tools").transform;
        originalFlowers = new(tools.GetChildren()
            .Where(c => c.name.StartsWith("SaplingFlower"))
            .Select(c => new KeyValuePair<string, Transform>(c.GetComponent<GameObjectID>().id, c)));
        prefab = originalFlowers.First().Value.gameObject.Clone().transform;
        prefab.name = "Sidequel_SaplingFlowerPrefab";
        prefab.gameObject.SetActive(false);
        var num = originalFlowers.Count;
        for (int count = 3; count <= 32; count++)
        {
            originalFlowerShortenIds = [.. originalFlowers.Keys.Select(s => s[0..count])];
            if (originalFlowerShortenIds.Count == num) break;
        }
    }
    private class AtmosphereChecker : MonoBehaviour
    {
        private const float ActivatingTime = 0.7f;
        private static AtmosphereController atmosphere = null!;
        private float startTime;
        private Sapling sapling = null!;
        private void Start()
        {
            if (atmosphere == null || atmosphere.gameObject == null)
            {
                atmosphere = GameObject.Find("/LevelSingletons").transform.Find("Customized/Atmosphere").GetComponent<AtmosphereController>();
            }
            if (!atmosphere.rain.isPlaying) GameObject.Destroy(this);
            startTime = Time.time;
            sapling = GetComponent<Sapling>();
        }
        private void Update()
        {
            if (Time.time - startTime >= ActivatingTime)
            {
                sapling.Water();
                GameObject.Destroy(this);
            }
        }
    }
    private class PlantingCoroutine : MonoBehaviour
    {
        private static PlantingCoroutine instance = null!;
        private static bool isActive = false;
        private static bool CannotWork(Player player)
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
                player.transform.position.y >= 600f ||
                IsThereFlowerNearby(out _, out _, out _)
            );
        }
        private IEnumerator Plant()
        {
            var player = Context.player;
            var inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
            yield return new WaitForSeconds(0.25f);
            if (CannotWork(player))
            {
                NodeData.RubberFlowerSapling.cannnotWorkActivated = true;
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
        private IEnumerator Remove()
        {
            var player = Context.player;
            var inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
            yield return new WaitForSeconds(0.25f);
            if (!IsThereFlowerNearby(out var obj, out var index, out var existsBitFarAway, isRemoving: true))
            {
                if (existsBitFarAway) NodeData.RubberFlowerSapling.shouldGetCloserActivated = true;
                else NodeData.RubberFlowerSapling.flowerNotFoundActivated = true;
                Dialogue.DialogueController.instance.StartConversation(null);
            }
            else if (CannotWork(player))
            {
                NodeData.RubberFlowerSapling.cannnotWorkActivated = true;
                Dialogue.DialogueController.instance.StartConversation(null);
            }
            else
            {
                player.body.velocity = Vector3.zero;
                player.TurnToFace(obj);
                yield return new WaitForSeconds(0.5f);
                var stopPose = player.ikAnimator.Pose(Pose.LookingDown);
                yield return new WaitForSeconds(0.75f);
                if (index != null)
                {
                    var idx = (int)index;
                    if (!watered[idx] && DataHandler.Find(Items.RubberFlowerSapling, out var saplingItem))
                    {
                        DataHandler.AddCollected(saplingItem, 1);
                        Context.levelUI.statusBar.ShowCollection(saplingItem.item).HideAndKill(1f);
                    }
                    watered.Remove(idx);
                    positions.Remove(idx);
                    objects.Remove(idx);
                }
                else
                {
                    var id = obj.GetComponent<GameObjectID>();
                    AddRemovedId(id.id);
                }
                GameObject.Destroy(obj.gameObject);
                Save();
                stopPose();
                yield return new WaitForSeconds(0.75f);
            }
            GameObject.Destroy(inputLock);
            isActive = false;
        }
        private void OnDestroy() => instance = null!;
        private static void MakeSureInstanceCreated()
        {
            instance ??= new GameObject("Sidequel_AdditionalFlower_PlantingController").AddComponent<PlantingCoroutine>();
        }
        internal static void StartRemoving()
        {
            if (isActive) return;
            MakeSureInstanceCreated();
            isActive = true;
            instance.StartCoroutine(instance.Remove());
        }
        internal static void StartPlanting()
        {
            if (isActive) return;
            MakeSureInstanceCreated();
            isActive = true;
            instance.StartCoroutine(instance.Plant());
        }
    }
    private static void Clone(int idx, Vector3 position)
    {
        var newObj = prefab.gameObject.Clone();
        newObj.gameObject.SetActive(true);
        newObj.transform.parent = container;
        newObj.name = $"{SaplingBoolTagPatcher.SaplingIDPrefix}{idx}";
        newObj.GetComponent<GameObjectID>().id = $"{SaplingBoolTagPatcher.SaplingIDPrefix}{idx}";
        newObj.transform.position = position;
        newObj.GetComponent<SphereCollider>().enabled = !watered[idx];
        newObj.AddComponent<AtmosphereChecker>();
        objects[idx] = newObj.transform;
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
        if (STags.TryGetString(AdditionalFlowerDataTag, out var data))
        {
            var d = data.Split("\n");
            if (d.Length != 3) return;
            DeserializeWatered(d[0]);
            DeserializePositions(d[1]);
            DeserializeRemovedIDs(d[2]);
            SetupPlantedFlowers();
            SetupRemovedOriginalFlowers();
        }
    }
    private static void Save()
    {
        STags.SetString(AdditionalFlowerDataTag, string.Join("\n", [
            SerializeWatered(),
            SerializePositions(),
            SerializeRemovedIDs(),
        ]));
    }
    private static string SerializeWatered() => string.Join(";", watered.Select(pair => $"{pair.Key}:{(pair.Value ? "1" : "0")}"));
    private static void SetupRemovedOriginalFlowers()
    {
        string[] ids = [.. originalFlowers.Keys];
        foreach (var id in ids)
        {
            if (removedFlowerIDs.Any(id.StartsWith))
            {
                GameObject.Destroy(originalFlowers[id].gameObject);
                originalFlowers.Remove(id);
            }
        }
    }
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
    private static void AddRemovedId(string id)
    {
        removedFlowerIDs.Add(originalFlowerShortenIds.FirstOrDefault(id.StartsWith) ?? id);
        originalFlowers.Remove(id);
    }
    private static string SerializeRemovedIDs()
    {
        return string.Join(",", removedFlowerIDs);
    }
    private static void DeserializeRemovedIDs(string data)
    {
        removedFlowerIDs = [.. data.Split(",")];
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

