
using System.Reflection;
using ModdingAPI;
using Sidequel.Item;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.World;
internal class ChestController
{
    private static readonly Dictionary<string, string> items = [];
    private static readonly string chestItemsTag = "ChestItemsData";
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => OnGameStarted();
    }
    internal static bool TryGet(string id, out ItemWrapperBase item)
    {
        item = null!;
        if (!items.TryGetValue(id, out var itemId)) return false;
        return DataHandler.Find(itemId, out item);
    }
    internal static void OnChestInteracted(string id)
    {
        if (!items.ContainsKey(id)) return;
        items.Remove(id);
        WriteToTags();
    }
    private static void OnGameStarted()
    {
        if (!State.IsActive) return;
        items.Clear();
        var buriedChestIDs = GameObject.FindObjectsOfType<BuriedChest>().Select(c => c.chest.GetComponent<GameObjectID>().id).ToHashSet();
        var chests = GameObject.FindObjectsOfType<Chest>().Select(chest =>
        {
            var id = chest.GetComponent<GameObjectID>();
            return new Tuple<GameObjectID, Chest>(id, chest);
        }).OrderBy(item => item.Item1.id);
        HashSet<string> ids = [];
        var setOpened = typeof(Chest).GetProperty("opened", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance);
        foreach (var item in chests)
        {
            var id = item.Item1;
            var chest = item.Item2;
            setOpened.SetValue(chest, true);
            chest.GetComponent<RangedInteractable>().enabled = true;
            if (buriedChestIDs.Contains(id.id))
            {
                //Debug($"chest with id {id.id} is buried chest");
                continue;
            }
            ids.Add(id.id);
        }
        if (State.IsNewGame) SetInitialItemsData(ids);
        else LoadFromTags();
    }
    private static void SetInitialItemsData(HashSet<string> ids)
    {
#if DEBUG
        int num = Math.Max(0, ids.Count - 15);
        Monitor.Log($"{num} chests are containing item!!", LL.Warning);
        List<string> itemNames = [
            Items.GoldenFeather,
            Items.Stick,
            Items.Coin,
        ];
        for (int i = 0; i < num; i++)
        {
            var id = ids.PickRandom();
            items[id] = itemNames.PickRandom();
            ids.Remove(id);
        }
#endif
        WriteToTags();
    }
    private static void WriteToTags()
    {
        var data = string.Join("\n", items.Select(pair => $"{pair.Key}:{pair.Value}"));
        STags.SetString(chestItemsTag, data);
    }
    private static void LoadFromTags()
    {
        if (!STags.TryGetString(chestItemsTag, out var data)) return;
        foreach (var item in data.Split("\n"))
        {
            var a = item.Split(":", 2);
            if (a.Length < 2) continue;
            items[a[0]] = a[1];
        }
    }
}

