
using ModdingAPI;

namespace Sidequel.System.Fishing;

internal static class Registory
{
    private const int MaxFish = 200;
    private static HashSet<string> soldFish = [];
    private static readonly List<Fish> heldFish = [];
    private static readonly Dictionary<string, Fish> biggestFish = [];
    private static readonly Dictionary<string, int> catchCount = [];
    internal static void AddFish(Fish fish)
    {
        if (Item.DataHandler.GetCollected(Items.Fish) >= MaxFish) return;
        heldFish.Add(fish);
        var key = FishToStr(fish);
        if (!biggestFish.TryGetValue(key, out var fish2) || fish2.size < fish.size)
        {
            biggestFish[key] = fish;
        }
        var name = fish.species.name;
        catchCount.TryAdd(name, 0);
        catchCount[name]++;
        Item.DataHandler.AddCollected(Items.Fish, 1);
        Save();
    }
    internal static void RemoveFish(Fish fish)
    {
        heldFish.Remove(fish);
        Item.DataHandler.AddCollected(Items.Fish, -1);
        Save();
    }
    internal static int GetCatchCount(FishSpecies fishSpecies) => catchCount.TryGetValue(fishSpecies.name, out var n) ? n : 0;
    internal static Fish GetBiggestFishRecord(FishSpecies fishSpecies, bool rare)
    {
        var key = FishToStr(fishSpecies, rare);
        return biggestFish.TryGetValue(key, out var fish) ? fish : null!;
    }
    internal static bool HasSold(Fish fish) => soldFish.Contains(FishToStr(fish));
    internal static void OnSold(Fish fish)
    {
        soldFish.Add(FishToStr(fish));
        Save();
    }
    internal static IEnumerable<Fish> GetAllFish() => heldFish;
    private static string FishToStr(Fish fish) => FishToStr(fish.species, fish.rare);
    private static string FishToStr(FishSpecies fishSpecies, bool rare) => $"{fishSpecies.name}{(rare ? "_Rare" : "")}";
    internal static void Load()
    {
        soldFish.Clear();
        heldFish.Clear();
        biggestFish.Clear();
        catchCount.Clear();
        var soldData = STags.GetString(Const.STags.SoldFish);
        if (soldData != null)
        {
            soldFish = [.. soldData.Split(",")];
        }
        var heldData = STags.GetString(Const.STags.HeldFish);
        if (heldData != null)
        {
            foreach (var d in heldData.Split(";"))
            {
                if (TryToParseFish(d, out var fish)) heldFish.Add(fish);
            }
        }
        var biggestData = STags.GetString(Const.STags.BiggestFish);
        if (biggestData != null)
        {
            foreach (var d in biggestData.Split(";"))
            {
                var a = d.Split(":", 2);
                if (a.Length == 2 && TryToParseFish(a[1], out var fish))
                {
                    biggestFish[a[0]] = fish;
                }
            }
        }
        var catchCountData = STags.GetString(Const.STags.FishCatchCount);
        if (catchCountData != null)
        {
            foreach (var d in catchCountData.Split(";"))
            {
                var a = d.Split(":", 2);
                if (a.Length == 2 && int.TryParse(a[1], out var count))
                {
                    catchCount[a[0]] = count;
                }
            }
        }
    }
    private static void Save()
    {
        var soldData = string.Join(",", soldFish);
        STags.SetString(Const.STags.SoldFish, soldData);
        var heldData = string.Join(";", heldFish.Select(SerializeFish));
        STags.SetString(Const.STags.HeldFish, heldData);
        var biggestData = string.Join(";", biggestFish.Select(pair => $"{pair.Key}:{SerializeFish(pair.Value)}"));
        STags.SetString(Const.STags.BiggestFish, biggestData);
        var catchCountData = string.Join(";", catchCount.Select(pair => $"{pair.Key}:{pair.Value}"));
        STags.SetString(Const.STags.FishCatchCount, catchCountData);
    }
    private static string SerializeFish(Fish fish)
    {
        var rare = fish.rare ? "1" : "0";
        var size = $"{fish.size}";
        var name = fish.species.name;
        return $"{rare},{size},{name}";
    }
    private static bool TryToParseFish(string data, out Fish fish)
    {
        fish = null!;
        var ar = data.Split(",", 3);
        if (ar.Length != 3) return false;
        var rare = ar[0] == "1";
        if (!float.TryParse(ar[1], out var size)) return false;
        var name = ar[2];
        var species = FishSpecies.Load(name);
        if (species == null) return false;
        fish = new(species, rare) { size = size };
        return true;
    }
    [Conditional("DEBUG")]
    internal static void DebugAddAllFishes()
    {
        Timer.Register(1.0f, () =>
        {
            List<string> fishes = [
                FishNames.Bluegill,
                FishNames.BrookTrout,
                FishNames.Burbot,
                FishNames.Carp,
                FishNames.Catfish,
                FishNames.ChinookSalmon,
                FishNames.Crayfish,
                FishNames.NorthernPike,
                FishNames.PumpkinSeed,
                FishNames.RainbowTrout,
                FishNames.SpottedBrookTrout,
                FishNames.WhiteBass,
                FishNames.WhitePerch,
                FishNames.YellowPerch,
            ];
            foreach (var f in fishes)
            {
                var s = FishSpecies.Load(f);
                Assert(s != null, $"species is null (name: {f})");
                Context.globalData.gameData.inventory.AddFish(new Fish(s, false));
                Context.globalData.gameData.inventory.AddFish(new Fish(s, true));
            }
        });
    }
}

