
using Sidequel.NodeData;
using Sidequel.System;

namespace Sidequel.Item;

internal static class Data
{
    internal static void Setup()
    {
        List<ExtendedItem> _ = [
            new(
                id: Items.Sunscreen,
                iconData: """
                000000000000
                011111111110
                011111111110
                000000000000
                011111111110
                011111111110
                011111111110
                011111111110
                011111111110
                011111111110
                011111111110
                001111111100
                """
            ),
            new(
                id: Items.WeakSunscreen,
                iconData: """
                000000000000
                011111111110
                011111111110
                000000000000
                011111111110
                011111111110
                011111111110
                011111111110
                011111111110
                011111100010
                011111111110
                001111111100
                """
            ),
            new(
                id: Items.StrongSunscreen,
                iconData: """
                000000000000
                011111111110
                011111111110
                000000000000
                011111111110
                011111111110
                011111111110
                011111111110
                011111110110
                011111100010
                011111110110
                001111111100
                """
            ),
            new(
                id: Items.HalfUsedSunscreen,
                iconData: """
                000000111110
                011111111110
                011111000000
                000000000000
                011111111110
                010000000010
                010000000010
                011111111110
                011111110110
                011111100010
                011111110110
                001111111100
                """
            ),
            new(
                id: Items.GoldMedal,
                iconData: """
                001110011100
                001110011100
                000111111000
                000100001000
                001000010100
                010000100010
                010001001010
                010000010010
                010000000010
                001000000100
                000100001000
                000011110000
                """
            ),
            new(
                id: Items.OldPicture,
                iconData: """
                000000000000
                111111111111
                111110000001
                111111100111
                111111101111
                111110001111
                111110011111
                111110001111
                111100000111
                111110001111
                111111111111
                000000000000
                """,
                GetOldPictureState
            ),
            new(
                id: Items.AntiqueFigure,
                iconData: """
                000011110000
                000001100000
                000001100000
                000011110000
                000111111000
                000111111000
                000111111000
                000111111000
                000111111000
                000011110000
                000011110000
                000001100000
                """
            ),
            new(
                id: Items.CuteEmptyCan,
                iconData: """
                000000000000
                000110000000
                001111000111
                001111011111
                011111111111
                011111111110
                011111111100
                011111110011
                001111001111
                010000011110
                010111111100
                010111110000
                """
            ),
            new(
                id: Items.SouvenirMedal,
                iconData: """
                000000000000
                000000000000
                000111111000
                011111111110
                101011101111
                100011010111
                101011010111
                111110011011
                011100111110
                000111111000
                000000000000
                000000000000
                """,
                GetSouvenirMedalState
            ),
            new(
                id: Items.FishHook,
                iconData: """
                000000000000
                000000000000
                000000000110
                000000000110
                000000000110
                001000001110
                001000001100
                001100011000
                001100111000
                001111110000
                000111100000
                000000000000
                """
            ),
            new(
                id: Items.JimsAddressNote,
                iconData: """
                000000000000
                000000000000
                111111111110
                100000000010
                010011101010
                100000000010
                010010111010
                100000000010
                010011011010
                100000000010
                111111111110
                000000000000
                """
            ),
            new(
                id: Items.FishScale1,
                iconData: """
                000000000000
                000000000000
                000000000000
                000011000000
                000101100000
                001100110000
                001000011000
                001000001100
                001000000100
                001000001100
                000111111000
                000000000000
                """
            ){ showPrompt = CollectableItem.PickUpPrompt.Always },
            new(
                id: Items.FishScale2,
                iconData: """
                000000001000
                000000010100
                000000001000
                000011001000
                000101100000
                001100110000
                001000011000
                001000001100
                001000000100
                001000001100
                000111111000
                000000000000
                """
            ) { showPrompt = CollectableItem.PickUpPrompt.Always },
            new(
                id: Items.FishScale3,
                iconData: """
                001000001000
                010100010100
                001000001000
                001011001010
                000101100101
                001100110010
                001000011010
                001000001100
                001000000100
                001000001100
                000111111000
                000000000000
                """
            ) { showPrompt = CollectableItem.PickUpPrompt.Always },
            new(
                id: Items.TradingCard,
                iconData: """
                001111111100
                001000000100
                001011110100
                001010010100
                001010010100
                001011110100
                001000000100
                001011110100
                001000000100
                001011010100
                001000000100
                001111111100
                """,
                GetTradingCardState
            ),
            new(
                id: Items.Pencil,
                iconData: """
                000000111101
                000001111011
                000011110111
                000111101110
                001111011101
                011110111011
                100011110110
                100011101100
                100000111000
                110000110000
                111000100000
                111111000000
                """
            ),
            new(
                id: Items.Binoculars,
                iconData: """
                001111110000
                000111111000
                011011111100
                101101111110
                110101111000
                111000110111
                111100001101
                111000001111
                110111001110
                101101000000
                011111000000
                011110000000
                """
            ){
                createWorldPrefab = System.Binoculars.BinocularsItem.CreateWorldPrefab,
                priority = 8,
            },
        ];
    }
    internal static void LoadOriginalItems()
    {
        ItemWrapperBase.TryLoad(Items.FishingRod);
        ItemWrapperBase.TryLoad(Items.Stick, GetStickState);
        ItemWrapperBase.TryLoad(Items.Pickaxe);
        ItemWrapperBase.TryLoad(Items.Coin, GetCoinState);
        ItemWrapperBase.TryLoad(Items.GoldenFeather, GetFeatherState);
        ItemWrapperBase.TryLoad(Items.WristWatch, GetWatchState);
        ItemWrapperBase.TryLoad(Items.Fish);
        ItemWrapperBase.TryLoad(Items.Bait);
        ItemWrapperBase.TryLoad(Items.FishEncyclopedia);
        ItemWrapperBase.TryLoad(Items.Bucket);
        ItemWrapperBase.TryLoad(Items.WalkieTalkie);
        ItemWrapperBase.TryLoad(Items.Compass);
        ItemWrapperBase.TryLoad(Items.RunningShoes, GetShoesState);
        ItemWrapperBase.TryLoad(Items.CampingPermit, GetPermitState);
    }
    internal static int? FishingRodOnKeyboardState { get; private set; } = null;
    private static int? GetCoinState()
    {
        var coinSavedup = Items.CoinsNum >= 400 || Items.CoinsSavedUp;
        if (coinSavedup) return Cont.IsEndingCont ? 3 : 2;
        return Items.CoinsNum >= 300 ? 1 : null;
    }
    private static int? GetShoesState()
    {
        return STags.GetBool(Avery.ShoesEquipped) ? 1 : null;
    }
    private static int? GetFeatherState()
    {
        if (Cont.IsLow) return 2;
        return Items.Num(Items.GoldenFeather) < 4 ? 1 : null;
    }
    private static int? GetPermitState()
    {
        return Cont.IsEndingCont && Items.CoinsSavedUp ? 1 : null;
    }
    private static int? GetWatchState()
    {
        if (Cont.IsLow) return 2;
        if (Flags.NodeDone(Deborah.Start1) || Flags.NodeDone(RumorGuy.BeforeJA3)) return 1;
        return null;
    }
    private static int? GetStickState()
    {
        if (Flags.NodeDone(BeachstickGameEnd.End2) || Flags.NodeDone(BeachstickGameEnd.End3)) return 2;
        if (!Flags.NodeYet(BeachstickKid.Start)) return 1;
        return null;
    }
    private static int? GetSouvenirMedalState()
    {
        return Flags.NodeYet(Jon.SouvenirMedal) ? null : 1;
    }
    private static int? GetOldPictureState()
    {
        return Flags.NodeYet(Jon.OldPicture) ? null : 1;
    }
    private static int? GetTradingCardState()
    {
        return Flags.NodeYet(Wil.TradingCard1) ? null : 1;
    }
}

