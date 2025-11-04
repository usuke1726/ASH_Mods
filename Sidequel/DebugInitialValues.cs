
//#define DEBUG_POSSESS_ALL_ITEMS
#if DEBUG
using WP = Sidequel.Dialogue.Debug.WarpPoints;

namespace Sidequel;

internal class DebugInitialValues
{
    public static readonly WP? Point = null;
    public static readonly bool? JADone = null;
    public static readonly int? Cont = null;
    public static readonly int? Money = null;
    public static readonly bool? MoneySavedUp = null;

    public static readonly Dictionary<string, int>? AdditionalItems =
#if DEBUG_POSSESS_ALL_ITEMS
        new()
        {
            [Items.WalkieTalkie] = 1,
            [Items.Compass] = 1,
            [Items.Pickaxe] = 1,
            [Items.FishingRod] = 1,
            [Items.Stick] = 1,
            [Items.Fish] = 1,
            [Items.RunningShoes] = 1,

            [Items.GoldMedal] = 1,
            [Items.SouvenirMedal] = 1,
            [Items.FishHook] = 1,
            [Items.AntiqueFigure] = 1,
            [Items.OldPicture] = 1,
            [Items.CuteEmptyCan] = 1,
            [Items.TradingCard] = 1,
            [Items.StrongSunscreen] = 1,
            [Items.WeakSunscreen] = 1,
            [Items.Sunscreen] = 1,
            [Items.HalfUsedSunscreen] = 1,
            [Items.JimsAddressNote] = 1,
            [Items.FishScale1] = 1,
            [Items.FishScale2] = 1,
            [Items.FishScale3] = 1,
        };
#else
        new()
        {
            [Items.Stick] = 1,
        };
#endif
}
#endif

