
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
    public static readonly string Bucket = "Bucket";
    public static readonly string WalkieTalkie = "WalkieTalkie";
    public static readonly string Compass = "Compass";
    public static readonly string RunningShoes = "RunningShoes";

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

    public static int Num(string id) => Item.DataHandler.GetCollected(id);
    public static int Num(Item.ItemWrapperBase item) => Item.DataHandler.GetCollected(item);
    public static void Add(string id, int amount = 1) => Item.DataHandler.AddCollected(id, amount);
    public static bool Has(string id) => Num(id) > 0;
    public static bool Has(Item.ItemWrapperBase item) => Num(item) > 0;
    public static int CoinsNum => Num(Coin);
    public static bool CoinsSavedUp => System.STags.GetBool(Const.STags.CoinsSavedUp);
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

