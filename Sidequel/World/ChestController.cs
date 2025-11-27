
using System.Reflection;
using ModdingAPI;
using Sidequel.Item;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.World;
internal class ChestController
{
    private static readonly Dictionary<string, ChestRewardInternal> items = [];
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => OnGameStarted();
    }
    internal static bool TryGet(string id, out ChestReward item)
    {
        item = null!;
        if (!items.Any()) return false;
        int len = items.First().Key.Length;
        if (id.Length < len) return false;
        if (!items.TryGetValue(id[0..len], out var reward)) return false;
        return ChestRewardInternal.TryGet(reward, out item);
    }
    internal static void OnChestInteracted(string id)
    {
        if (!items.Any()) return;
        int len = items.First().Key.Length;
        if (id.Length < len) return;
        var key = id[0..len];
        if (!items.ContainsKey(key)) return;
        items.Remove(key);
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
        List<ChestRewardInternal> rewards = [
            new(Items.OldPicture),
            new(Items.SouvenirMedal),
            new(Items.CuteEmptyCan),
            new(Items.FishHook),
            new(Items.GoldMedal),
            new(Items.AntiqueFigure),
            new(Items.TradingCard),
            new(Items.Coin, 4),
            new(Items.Coin, 7),
            new(Items.Coin, 8),
            new(Items.Coin, 9),
            new(Items.Coin, 12),
        ];
        int num = rewards.Count;
        Assert(ids.Count >= num, "too few chests");
        Debug($"{num}/{ids.Count} chests are containing item!!", LL.Warning);
        for (int i = 0; i < num; i++)
        {
            var id = ids.PickRandom();
            var reward = rewards.PickRandom();
            items[id] = reward;
            ids.Remove(id);
            rewards.Remove(reward);
        }
        WriteToTags();
    }
    private static void WriteToTags()
    {
        var data = string.Join("\n", items.Select(pair => $"{pair.Key}:{pair.Value}"));
#if DEBUG
        Debug($"Items in chests: {string.Join(";", items.Select(pair => $"{pair.Key}:{pair.Value}"))}", LL.Debug);
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
            items[a[0]] = ChestRewardInternal.From(a[1]);
        }
    }
    internal class ChestReward(ItemWrapperBase item, int amount)
    {
        internal ItemWrapperBase Item { get; private set; } = item;
        internal int Amount { get; private set; } = amount;
    }
    private class ChestRewardInternal(string id, int amount = 1)
    {
        internal string Id { get; private set; } = id;
        internal int Amount { get; private set; } = amount;
        public override string ToString() => $"{Id},{Amount}";
        internal static ChestRewardInternal From(string data)
        {
            var list = data.Split(",", 2);
            if (list.Length != 2) throw new Exception("invalid data");
            if (!int.TryParse(list[1], out var amount)) throw new Exception("second value is not an integer");
            return new(list[0], amount);
        }
        internal static bool TryGet(ChestRewardInternal from, out ChestReward reward)
        {
            reward = null!;
            if (DataHandler.Find(from.Id, out var item))
            {
                reward = new(item, from.Amount);
                return true;
            }
            return false;
        }
    }
}

