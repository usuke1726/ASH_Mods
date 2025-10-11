
using System.Reflection;
using System.Text.RegularExpressions;
using ModdingAPI;
using ModdingAPI.KeyBind;
using UnityEngine;

namespace SideStory.Item;

internal static class DataHandler
{
    private static readonly Dictionary<string, ItemWrapperBase> items = [];
    private static Dictionary<string, int> collected = [];
    private static readonly IReadOnlyDictionary<string, int> initialCollected = new Dictionary<string, int>()
    {
        [Items.WristWatch] = 1,
        [Items.Coin] = 10,
        [Items.GoldenFeather] = 4,
    };
    internal static void Setup(IModHelper helper)
    {
        Data.Setup();
        helper.Events.Gameloop.GameStarted += (_, _) => EnsureDataLoaded();
        helper.Events.System.BeforeSaving += (_, _) => WriteToSaveData();
        helper.Events.System.LocaleChanged += (_, _) => OnLocaleChanged();
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            var feather = CollectableItem.Load("GoldenFeather");
            if (feather == null)
            {
                Monitor.Log($"feather is null!", LL.Warning);
                return;
            }
            Data.LoadOriginalItems();
            EnsureIconCreated(feather.icon);
            OnLocaleChanged();
            UpdateItemStates();
            if (State.IsActive && Find(Items.Coin, out var coin))
            {
                coin.item.showPrompt = CollectableItem.PickUpPrompt.Never;
            }
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            collected = [];
            hasLoaded = false;
            foreach (var item in items.Values) item.OnReturnedTitle();
            items.Clear();
        };
    }

    internal static void ValidateItemId(string id)
    {
        if (items.ContainsKey(id)) throw new Exception($"the item of id {id} already exists");
        if (Regex.IsMatch(id, @"[^a-zA-Z0-9 _-]")) throw new Exception($"invalid symbol is contained in \"{id}\"");
    }
    internal static void Register(ItemWrapperBase item) => items[item.id] = item;

    internal static IEnumerable<CollectableItem> GetAllCollected()
    {
        EnsureDataLoaded();
        return collected.Where(p => p.Value > 0 && items.ContainsKey(p.Key)).Select(p => items[p.Key].item);
    }
    internal static int GetCollected(ItemWrapperBase item) => GetCollected(item.id);
    internal static int GetCollected(string id)
    {
        EnsureDataLoaded();
        return collected.TryGetValue(id, out var v) ? v : 0;
    }
    internal static void AddCollected(ItemWrapperBase item, int amount, bool equipAction = false)
    {
        EnsureDataLoaded();
        if (!collected.ContainsKey(item.id)) collected[item.id] = 0;
        var value = Math.Max(collected[item.id] + amount, 0);
        collected[item.id] = value;
        if (item.id == Items.GoldenFeather) OnGoldenFeathersChanged(value);
        else if (item.id == Items.Coin) Context.levelUI.statusBar.ShowCollection(item.item).HideAndKill(3f);
        WriteToSaveData();
    }
    internal static void AddCollected(string id, int amount, bool equipAction = false)
    {
        if (Find(id, out var item)) AddCollected(item, amount, equipAction);
    }
    internal static void AddCollected(CollectableItem collectable, int amount, bool equipAction = false)
    {
        if (Find(collectable, out var item)) AddCollected(item, amount, equipAction);
    }
    internal static bool Find(string id, out ItemWrapperBase item) => items.TryGetValue(id, out item);
    internal static ItemWrapperBase? Find(string id) => Find(id, out var item) ? item : null;
    internal static bool Find(CollectableItem collectable, out ItemWrapperBase item)
    {
        foreach (var i in items.Values)
        {
            if (i.item == collectable)
            {
                item = i;
                return true;
            }
        }
        item = null!;
        return false;
    }

    internal static void OnLocaleChanged()
    {
        foreach (var item in items.Values) item.OnLocaleChanged();
    }
    internal static void UpdateItemStates()
    {
        foreach (var item in items.Values) item.UpdateState();
    }
    internal static void EnsureIconCreated(Sprite resource)
    {
        foreach (var item in items.Values) item.EnsureIconCreated(resource);
    }


    private static readonly string dataTag = $"ModRegistry_Quicker1726_SideStory_Items";
    private static void WriteToSaveData()
    {
        Context.globalData.gameData.tags.SetString(dataTag, Serialize());
    }
    private static bool hasLoaded = false;
    private static void EnsureDataLoaded()
    {
        if (hasLoaded) return;
        LoadFromSaveData();
        hasLoaded = true;
    }
    private static void LoadFromSaveData()
    {
        collected = State.IsNewGame
            ? new(initialCollected)
            : Deserialize(Context.globalData.gameData.tags.GetString(dataTag));
    }
    private static string Serialize()
    {
        return string.Join(";", items
            .Where(p => collected.ContainsKey(p.Key))
            .Select(p => $"{p.Key}:{collected[p.Key]}"));
    }
    private static Dictionary<string, int> Deserialize(string rawData)
    {
        Monitor.Log($"savedata loaded!!\n{rawData}", LL.Warning);
        if (rawData == null) return [];
        Dictionary<string, int> ret = [];
        var data = rawData
            .Split(";")
            .Select(item =>
            {
                var a = item.Split(":", 2);
                if (a.Length < 2) return null;
                string name = a[0];
                if (int.TryParse(a[1], out var num))
                {
                    return new Tuple<string, int>(name, num);
                }
                else return null;
            });
        foreach (var d in data)
        {
            if (d == null) continue;
            ret[d.Item1] = d.Item2;
        }
        return ret;
    }

    private static readonly MethodInfo playerOnGoldenFeathersUpdated = typeof(Player).GetMethod("OnGoldenFeathersUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo uiOnGoldenFeathersUpdated = typeof(FeatherUIController).GetMethod("OnGoldenFeathersChanged", BindingFlags.NonPublic | BindingFlags.Instance);
    private static GameObject? featherUIObj = null;
    private static FeatherUIController featherUI = null!;
    private static void OnGoldenFeathersChanged(int amount)
    {
        playerOnGoldenFeathersUpdated.Invoke(Context.player, new object[] { amount });
        if (featherUIObj == null)
        {
            featherUIObj = GameObject.Find("LevelSingletons").transform.Find("UICanvas/UIElements/FeatherBar").gameObject;
            featherUI = featherUIObj.GetComponent<FeatherUIController>();
        }
        uiOnGoldenFeathersUpdated.Invoke(featherUI, new object[] { amount });
    }
}

