
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.World;

namespace Sidequel.NodeData;

internal class Chest : NodeEntry
{
    protected override Characters? Character => null;
    private static bool hasChestInteractedJustNow = false;
    private static string chestId = null!;
    private ChestController.ChestReward itemInChest = null!;
    private bool isItemIn;
    private bool didLastChestHaveItem;
    private bool hasAlreadyChecked;
    private readonly HashSet<string> checkedIds = [];
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
    protected override Node[] Nodes => [
        new("chest", [
            tag(Const.STags.HasCheckedChestOnce, true),
            command(() => {
                hasChestInteractedJustNow = false;
                didLastChestHaveItem = isItemIn;
                isItemIn = ChestController.TryGet(chestId, out itemInChest);
                ChestController.OnChestInteracted(chestId);
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
            tag(Const.STags.HasGotItemFromChestOnce, true),
            @if(
                () => itemInChest.Amount == 1,
                item(() => itemInChest.Item.id),
                item(() => itemInChest.Item.id, () => itemInChest.Amount)
            ),
            next(() => $"chest.item.{itemInChest.Item.id}"),
        ], condition: () => hasChestInteractedJustNow, priority: int.MaxValue),
        new($"chest.item.{Items.GoldMedal}", [
            @if(() => _HM, lines(1, 3, digit2($"HM", ""), Player), lines(1, 3, digit2($"L", ""), Player)),
        ], () => false),
        new($"chest.item.{Items.Coin}", [
            @if(() => _HM, lines(1, 2, digit2($"HM", ""), Player), lines(1, 3, digit2($"L", ""), Player)),
        ], () => false),
        new($"chest.item.{Items.SouvenirMedal}", [
            lines(1, 4, digit2, Player),
        ], () => false),
        new($"chest.item.{Items.FishHook}", [
            lines(1, 3, digit2, Player),
        ], () => false),
        new($"chest.item.{Items.AntiqueFigure}", [
            @if(() => _HM, lines(1, 4, digit2($"HM", ""), Player), lines(1, 4, digit2($"L", ""), Player)),
        ], () => false),
        new($"chest.item.{Items.OldPicture}", [
            lines(1, 8, digit2, Player, [
                new(3, emote(Emotes.Surprise, Player)),
                new(5, emote(Emotes.Normal, Player)),
                new(7, emote(Emotes.EyesClosed, Player)),
            ]),
        ], () => false),
        new($"chest.item.{Items.CuteEmptyCan}", [
            @if(() => _HM, lines(1, 4, digit2($"HM", ""), Player), lines(1, 4, digit2($"L", ""), Player)),
        ], () => false),
        new($"chest.item.{Items.TradingCard}", [
            lines(1, 7, digit2, Player),
        ], () => false),
    ];
}

