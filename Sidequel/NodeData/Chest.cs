
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;

namespace Sidequel.NodeData;

internal class Chest : NodeEntry
{
    protected override Characters? Character => null;
    private static bool hasChestInteractedJustNow = false;
    private static string chestId = null!;
    private Item.ItemWrapperBase itemInChest = null!;
    private bool isItemIn;
    private bool didLastChestHaveItem;
    private bool hasAlreadyChecked;
    private readonly HashSet<string> checkedIds = [];
    private string linePrefixOnGotItem = "";
    internal static void OnChestInteracted(string id)
    {
        hasChestInteractedJustNow = true;
        chestId = id;
    }
    internal override void OnGameStarted()
    {
        isItemIn = true;
        checkedIds.Clear();
    }
    protected override Node[] Nodes => [new("chest", [
        command(() => {
            hasChestInteractedJustNow = false;
            didLastChestHaveItem = isItemIn;
            isItemIn = World.ChestController.TryGet(chestId, out itemInChest);
            World.ChestController.OnChestInteracted(chestId);
            hasAlreadyChecked = checkedIds.Contains(chestId);
            checkedIds.Add(chestId);
        }),
        @if(() => hasAlreadyChecked, null, "firstCheck"),
        line("alreadyChecked", Player),
        command(() => isItemIn = didLastChestHaveItem),
        end(),
        @if(() => isItemIn, "isItemIn", null, anchor: "firstCheck"),
        lineif(() => didLastChestHaveItem, "empty", "empty2", Player),
        end(),
        @if(() => UnityEngine.Random.value > 0.9f, "v2", null, anchor: "isItemIn"),
        line("item1-1", Player),
        line("item1-2", Player),
        @goto("getItem"),
        anchor("v2"),
        lineif(() => didLastChestHaveItem, "empty", "empty2", Player),
        wait(0.8f),
        line("item2-1", Player),
        anchor("getItem"),
        @if(
            () => itemInChest.id == Items.Coin,
            item(() => Items.Coin, () => UnityEngine.Random.Range(1, 5)),
            item(() => itemInChest.id)
        ),
        command(() => linePrefixOnGotItem = GetLinePrefixOnGotItem()),
        lines(i => $"onGotItem.{linePrefixOnGotItem}{i}", Player),
    ],
        condition: () => hasChestInteractedJustNow,
        priority: int.MaxValue
    )];
    private static readonly string gotGoldenFeatherTag = "GotGoldenFeatherOnceFromChest";
    private string GetLinePrefixOnGotItem()
    {
        var id = itemInChest.id;
        if (id == Items.GoldenFeather)
        {
            if (STags.GetBool(gotGoldenFeatherTag)) return "end";
            STags.SetBool(gotGoldenFeatherTag);
            return Items.GoldenFeather;
        }
        if (id == Items.Coin) return Items.Coin;
        if (id == Items.Stick)
        {
            if (Item.DataHandler.GetCollected(Items.Stick) >= 10)
            {
                return "TooManySticks";
            }
            return Items.Stick;
        }
        return "default";
    }
}

