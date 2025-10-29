
using System.Reflection;
using ModdingAPI;
using Sidequel.Item;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.World;
internal class ChestController
{
    private static readonly Dictionary<string, string> items = [];
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => OnGameStarted();
    }
    internal static bool TryGet(string id, out ItemWrapperBase item)
    {
        item = null!;
        if (!items.Any()) return false;
        int len = items.First().Key.Length;
        if (id.Length < len) return false;
        if (!items.TryGetValue(id[0..len], out var itemId)) return false;
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
        if (State.IsNewGame) SetInitialItemsData(ShortenIds(ids));
        else LoadFromTags();
    }
    private static HashSet<string> ShortenIds(HashSet<string> ids)
    {
        int len = ids.Count;
        for (int i = 3; i < 32; i++)
        {
            HashSet<string> newIds = [.. ids.Select(s => s[0..i])];
            if (newIds.Count == len) return newIds;
        }
        return ids;
    }
    private static void SetInitialItemsData(HashSet<string> ids)
    {
#if DEBUG
        int num = Math.Max(0, ids.Count - 15);
        Monitor.Log($"{num}/{ids.Count} chests are containing item!!", LL.Warning);
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
#if DEBUG
        Monitor.Log($"Items in chests: {string.Join(";", items.Select(pair => $"{pair.Key}:{pair.Value}"))}", LL.Debug, true);
#endif
        STags.SetString(Const.STags.ChestItems, data);
    }
    private static void LoadFromTags()
    {
        if (!STags.TryGetString(Const.STags.ChestItems, out var data)) return;
        foreach (var item in data.Split("\n"))
        {
            var a = item.Split(":", 2);
            if (a.Length < 2) continue;
            items[a[0]] = a[1];
        }
    }
}

