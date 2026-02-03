
namespace Sidequel;

internal static class Items
{
    public static readonly string Coin = "Coin";
    public static readonly string GoldenFeather = "GoldenFeather";
    public static readonly string WristWatch = "Watch";
    public static readonly string FishingRod = "FishingRod";
    public static readonly string Stick = "Stick";
    public static readonly string Pickaxe = "Pickaxe";
    public static readonly string Fish = "Fish";
    public static readonly string Bait = "Bait";
    public static readonly string FishEncyclopedia = "FishEncyclopedia";
    public static readonly string Bucket = "Bucket";
    public static readonly string WalkieTalkie = "WalkieTalkie";
    public static readonly string Compass = "Compass";
    public static readonly string RunningShoes = "RunningShoes";
    public static readonly string CampingPermit = "CampingPermit";

    // New items
    public static readonly string GoldMedal = "GoldMedal";
    public static readonly string SouvenirMedal = "SouvenirMedal";
    public static readonly string FishHook = "FishHook";
    public static readonly string AntiqueFigure = "AntiqueFigure";
    public static readonly string OldPicture = "OldPicture";
    public static readonly string CuteEmptyCan = "CuteEmptyCan";
    public static readonly string TradingCard = "TradingCard";
    public static readonly string StrongSunscreen = "StrongSunscreen";
    public static readonly string WeakSunscreen = "WeakSunscreen";
    public static readonly string Sunscreen = "Sunscreen";
    public static readonly string HalfUsedSunscreen = "HalfUsedSunscreen";
    public static readonly string JimsAddressNote = "JimsAddressNote";
    public static readonly string FishScale1 = "FishScale1";
    public static readonly string FishScale2 = "FishScale2";
    public static readonly string FishScale3 = "FishScale3";
    public static readonly string Binoculars = "Binoculars";
    public static readonly string Pencil = "Pencil";
    public static readonly string EternalFeather = "EternalFeather";
    public static readonly string RubberFlowerSapling = "RubberFlowerSapling";

    public static int Num(string id) => Item.DataHandler.GetCollected(id);
    public static int Num(Item.ItemWrapperBase item) => Item.DataHandler.GetCollected(item);
    public static void Add(string id, int amount = 1) => Item.DataHandler.AddCollected(id, amount);
    public static bool Has(string id) => Num(id) > 0;
    public static bool Has(Item.ItemWrapperBase item) => Num(item) > 0;
    public static int CoinsNum => Num(Coin);
    public static bool CoinsSavedUp => System.STags.GetBool(Const.STags.CoinsSavedUp);
    public static int ApproxDiff(int? coins = null)
    {
        if (CoinsSavedUp) return 0;
        var diff = 400 - (coins ?? CoinsNum) + 5;
        diff -= diff % 10;
        return diff;
    }
    public static string ReplaceApproxDiff(string s) => ReplaceApproxDiff(CoinsNum, s);
    public static string ReplaceApproxDiff(int coins, string s) => s.Replace("{{ApproxDiff}}", $"{ApproxDiff(coins)}");
}

internal static class FishNames
{
    internal const string Bluegill = "BluegillFish";
    internal const string BrookTrout = "BrookTroutFish";
    internal const string Burbot = "BurbotFish";
    internal const string Carp = "CarpFish";
    internal const string Catfish = "CatfishFish";
    internal const string ChinookSalmon = "ChinookSalmonFish";
    internal const string Crayfish = "CrayfishFish";
    internal const string NorthernPike = "NorthernPikeFish";
    internal const string PumpkinSeed = "PumpkinSeedFish";
    internal const string RainbowTrout = "RainbowTroutFish";
    internal const string SpottedBrookTrout = "SpottedBrookTroutFish";
    internal const string WhiteBass = "WhiteBassFish";
    internal const string WhitePerch = "WhitePerchFish";
    internal const string YellowPerch = "YellowPerchFish";
    internal static string ToI18nKey(Fish fish) => $"{ToI18nKey(fish.species)}{(fish.rare ? ".rare" : "")}";
    internal static string ToI18nKey(FishSpecies fishSpecies)
    {
        Assert(fishSpecies.name.EndsWith("Fish"), $"fishSpecies name does not end with \"Fish\" ({fishSpecies.name})");
        return fishSpecies.name[0..^4];
    }
}

public enum Emotes
{
    Normal,
    PlayerWideEyes,
    Happy,
    Surprise,
    EyesClosed,
}
internal enum Poses
{
    Sitting, Walking, Running, Standing, ReallyAnxious,
}

internal enum TagActions { Add, Toggle, }

