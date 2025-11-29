
#define ALL_DISABLED
//#define DEBUG_POSSESS_ALL_ITEMS
#if DEBUG
using WP = Sidequel.Dialogue.Debug.WarpPoints;

namespace Sidequel;

internal class DebugInitialValues : DebugInitialValuesBase
{
#if !ALL_DISABLED
    //internal override WP? Point => WP.Peak;
    internal override bool? JADone => true;
    internal override int? Cont => 120;
    internal override int? Money => 50;
    internal override bool? MoneySavedUp => true;

    internal override Dictionary<string, int>? AdditionalItems =>
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
            [Items.Binoculars] = 1,
            [Items.Pencil] = 1,
        };
#else
        new()
        {
            [Items.Stick] = 5,
            [Items.FishingRod] = 1,
            [Items.GoldMedal] = 1,
            [Items.RunningShoes] = 1,
            //[Items.FishScale1] = 1,
            //[Items.FishScale2] = 1,
            //[Items.FishScale3] = 2,
            //[Items.Binoculars] = 1,
            //[Items.CuteEmptyCan] = 1,
            //[Items.Pencil] = 1,
        };
#endif

    internal override void OnGameStarted()
    {
    }
#endif
}
internal abstract class DebugInitialValuesBase
{
    internal static DebugInitialValuesBase instance = new DebugInitialValues();
    internal virtual WP? Point => null;
    internal virtual bool? JADone => null;
    internal virtual int? Cont => null;
    internal virtual int? Money => null;
    internal virtual bool? MoneySavedUp => null;
    internal virtual Dictionary<string, int>? AdditionalItems => null;
    internal virtual void OnGameStarted() { }
}
#endif

