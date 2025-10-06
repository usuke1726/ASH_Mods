
namespace SideStory.Item;

internal static class Data
{
    internal static void Setup()
    {
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
    }
    internal static int? FishingRodOnKeyboardState { get; private set; } = null;
}

