
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
                """
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
                """
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
                """
            ),
        ];
    }
    internal static void LoadOriginalItems()
    {
        ItemWrapperBase.TryLoad(Items.FishingRod);
        ItemWrapperBase.TryLoad(Items.Stick);
        ItemWrapperBase.TryLoad(Items.Pickaxe);
        ItemWrapperBase.TryLoad(Items.Coin);
        ItemWrapperBase.TryLoad(Items.GoldenFeather);
        ItemWrapperBase.TryLoad(Items.WristWatch);
        ItemWrapperBase.TryLoad(Items.Fish);
        ItemWrapperBase.TryLoad(Items.Bucket);
        ItemWrapperBase.TryLoad(Items.WalkieTalkie);
        ItemWrapperBase.TryLoad(Items.Compass);
        ItemWrapperBase.TryLoad(Items.RunningShoes);
    }
    internal static int? FishingRodOnKeyboardState { get; private set; } = null;
}

