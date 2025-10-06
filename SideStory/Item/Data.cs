
namespace SideStory.Item;

internal static class Data
{
    internal static void Setup()
    {
        List<ExtendedItem> _ = [
            new(
                id: Items.Coin,
                i18nKeys: new("item.Coin.name", "item.Coin.namePlural", "item.Coin.description"),
                iconData: """
                000011110000
                000100001000
                001011110100
                010110011010
                101100001101
                101101111101
                101101111101
                101100001101
                010110011010
                001011110100
                000100001000
                000011110000
                """
            ) {
                cannotDrop = true,
                cannotStash = true,
                showPrompt = CollectableItem.PickUpPrompt.OnlyOnce,
                priority = 0,
            },
            new(
                id: Items.GoldenFeather,
                i18nKeys: new("item.GoldenFeather.name", "item.GoldenFeather.namePlural", "item.GoldenFeather.description"),
                iconData: """
                000000111111
                000011100110
                000111011100
                001110111000
                001101110000
                011011100000
                010111000000
                101110000000
                101000000000
                010000000000
                010000000000
                001000000000
                """
            ) {
                cannotDrop = true,
                cannotStash = true,
                showPrompt = CollectableItem.PickUpPrompt.Always,
                priority = -10,
            },
            new(
                id: Items.WristWatch,
                i18nKeys: new("item.Wristwatch.name", "item.Wristwatch.namePlural", "item.Wristwatch.description"),
                iconData: """
                000011110000
                000101111000
                001001001000
                001001111100
                001011000110
                001010010010
                001010011010
                001011000110
                001001111100
                001001001000
                000101111000
                000011110000
                """
            ) {
                priority = -10
            },
        ];
    }
}

