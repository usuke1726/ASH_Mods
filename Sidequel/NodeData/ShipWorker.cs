
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;
using FishRegistory = Sidequel.System.Fishing.Registory;

namespace Sidequel.NodeData;

internal class ShipWorker : NodeEntry
{
    internal const string Start1 = "ShipWorker.Start1";
    internal const string Start2 = "ShipWorker.Start2";
    internal const string BeforeJA = "ShipWorker.BeforeJA";
    internal const string AfterJA1 = "ShipWorker.AfterJA1";
    internal const string FishNotYet = "ShipWorker.FishNotYet";
    internal const string FishShowing = "ShipWorker.FishShowing";
    internal const string Fish = "ShipWorker.Fish";
    internal static bool isShowing = false;
    private static FishBuyer fishBuyer = null!;
    protected override Characters? Character => Characters.ShipWorker1;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 3, digit2, [], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(3, emote(Emotes.Happy, Original)),
            ]),
            @if(() => _HM, lines(4, 5, digit2("HM", ""), [4, 5]), line("L04", Player)),
            lines(6, 7, digit2, []),
            done(),
        ], condition: () => NodeYet(Start1), priority: 10),

        new(Start2, [
            lines(1, 5, digit2, [5], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2)),

        new(BeforeJA, [
            lines(1, 3, digit2, [3], [new(2, emote(Emotes.Happy, Original))]),
        ], condition: () => _bJA && NodeDone(Start2)),

        new(AfterJA1, [
            lines(1, 19, digit2, [4, 8, 9, 16, 17, 18], [
                new(15, emote(Emotes.Happy, Original)),
                new(16, emote(Emotes.Normal, Original)),
                new(19, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _aJA && NodeDone(Start2) && NodeYet(AfterJA1)),

        new(FishNotYet, [
            lines(1, 4, digit2, [3]),
        ], condition: () => NodeDone(AfterJA1) && !Items.Has(Items.Fish)),

        new(FishShowing, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "showing"),
            emote(Emotes.Happy, Original),
            line("O2.01", Original),
            end(),
            anchor("showing"),
            command(() => isShowing = true),
        ], condition: () => NodeDone(AfterJA1) && Items.Has(Items.Fish),
            onConversationFinish: CheckForSell
        ),
    ];
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            fishBuyer = Ch(Characters.ShipWorker1).gameObject.GetComponent<FishBuyer>();
            Assert(fishBuyer != null, "fishBuyer is null");
        });
        FishRegistory.Load();
    }
    internal static void CheckForSell()
    {
        if (!isShowing) return;
        isShowing = false;
        OpenMenu();
    }
    private static void OpenMenu()
    {
        FishItemActions.CreateFishMenu(fishBuyer.collectionUIPrefab, ShowSubmenu)
            .GetComponent<KillOnBackButton>().onKill += SellFishCancelNode.StartConversation;
    }
    private static void ShowSubmenu(Fish fish, CollectionListUIElement element, GameObject fishMenu)
    {
        LinearMenu menu = null!;
        List<string> list = [];
        List<Action> list2 = [];
        int baitWorth = 1 + (fish.sizeCategory != global::Fish.SizeCategory.Normal ? 1 : 0) + (fish.rare ? 2 : 0);
        if (GetBool(Const.STags.SoldFishBait) && FishRegistory.HasSold(fish))
        {
            string format = baitWorth == 1 ? I18n.STRINGS.sellForBait : I18n.STRINGS.sellForBaitPlural;
            list.Add(string.Format(format, baitWorth));
            list2.Add(() =>
            {
                FishRegistory.RemoveFish(fish);
                Item.DataHandler.AddCollected(Items.Bait, baitWorth);
                menu.Kill();
                fishMenu.GetComponent<CollectionListUI>().RemoveElement(element.gameObject);
                fishBuyer.saleSoundEffect.Play();
            });
        }
        else
        {
            list.Add(I18n.STRINGS.sell);
            list2.Add(() =>
            {
                FishRegistory.RemoveFish(fish);
                menu.Kill();
                GameObject.Destroy(fishMenu);
                SellFishNode.StartConversation(fish, baitWorth);
            });
        }
        menu = Context.gameServiceLocator.ui.CreateSimpleMenu([.. list], [.. list2]);
        element.PositionSimpleMenuAbove(menu.gameObject);
    }
}

internal class SellFishNode : NodeEntry
{
    internal const string ShipWorker1 = "ShipWorker1";
    internal const string Entry = "SellFishNode.Entry";
    internal const string Sold = "ShipWorker.Fish.CarterHasSold";
    internal const string ClaireFished = "ShipWorker.Fish.ClaireHasFished";
    internal const string Rare = "ShipWorker.Fish.Rare";
    internal const string AfterSold = "SellFishNode.AfterSold";
    private static bool isActive = false;
    private static Fish fish = null!;
    private static int coins = 0;
    private static int baitWorth = 0;
    protected override Characters? Character => null;
    private static readonly Dictionary<string, int> rareFishPrices = new()
    {
        [FishNames.Bluegill] = 60,
        [FishNames.BrookTrout] = 20,
        [FishNames.Burbot] = 56,
        [FishNames.Carp] = -1,
        [FishNames.Catfish] = 40,
        [FishNames.ChinookSalmon] = 80,
        [FishNames.Crayfish] = 32,
        [FishNames.NorthernPike] = 80,
        [FishNames.PumpkinSeed] = 60,
        [FishNames.RainbowTrout] = 80,
        [FishNames.SpottedBrookTrout] = 32,
        [FishNames.WhiteBass] = 60,
        [FishNames.WhitePerch] = 30,
        [FishNames.YellowPerch] = 30,
    };
    protected override Node[] Nodes => [
        new(Entry, [
            command(() => isActive = false),
            next(() => {
                if(FishRegistory.HasSold(fish)) return Sold;
                FishRegistory.OnSold(fish);
                if(!fish.rare) return ClaireFished;
                return Rare;
            }),
        ], condition: () => isActive, priority: int.MaxValue),

        new(Sold, [
            tag(Const.STags.SoldFishBait, true),
            lines(digit2, ShipWorker1),
            item(() => Items.Bait, () => baitWorth),
            next(() => AfterSold),
        ], condition: () => false),

        new(ClaireFished, [
            lines(1, 3, digit2, ShipWorker1, replacer: s => s.Replace("{{FishName}}", fish.GetTitle())),
            @if(() => quiteValuableSpecies.Contains(fish.species.name), "quiteValuable"),
            line(4, ShipWorker1),
            item(() => Items.Bait, () => baitWorth),
            @goto("end"),
            anchor("quiteValuable"),
            line("quiteValuable 04", ShipWorker1),
            item(Items.Coin, 5),
            next(() => AfterSold, anchor: "end"),
        ], condition: () => false),

        new(Rare, [
            command(() => coins = rareFishPrices[fish.species.name]),
            lines(i => $"ShipWorker.Fish.{FishNames.ToI18nKey(fish.species)}.{i:00}", ShipWorker1, replacer: s => s.Replace("{{Coin}}", $"{coins}"), useId: false),
            item(() => Items.Coin, () => coins),
            next(() => AfterSold),
        ], condition: () => false),

        new (AfterSold, [
            line("ShipWorker.FishShowing.O1.AfterSold", ShipWorker1, useId: false),
            @if(() => !Items.Has(Items.Fish), "empty"),
            option(["ShipWorker.FishShowing.O1.AfterSold.O1", "ShipWorker.FishShowing.O1.AfterSold.O2"], useId: false),
            @if(() => LastSelected == 0, "showing"),
            emote(Emotes.Happy, ShipWorker1),
            line("ShipWorker.FishShowing.O2.01", ShipWorker1, useId: false),
            emote(Emotes.Normal, ShipWorker1),
            end(),
            anchor("empty"),
            line("ShipWorker.FishShowing.O1.SoldOut", Player, useId: false),
            end(),
            anchor("showing"),
            command(() => ShipWorker.isShowing = true),
        ], condition: () => false, onConversationFinish: ShipWorker.CheckForSell),
    ];
    internal static void StartConversation(Fish _fish, int _baitWorth)
    {
        fish = _fish;
        baitWorth = _baitWorth;
        isActive = true;
        Dialogue.DialogueController.instance.StartConversation(null);
    }
    private static readonly HashSet<string> quiteValuableSpecies = [
        FishNames.RainbowTrout,
        FishNames.ChinookSalmon,
        FishNames.NorthernPike,
        FishNames.PumpkinSeed,
        FishNames.Bluegill,
        FishNames.WhiteBass,
    ];
}

internal class SellFishCancelNode : NodeEntry
{
    private static bool isActive = false;
    internal const string Cancel = "SellFishCalcelNode";
    protected override Characters? Character => null;
    protected override Node[] Nodes => [
        new(Cancel, [
            command(() => isActive = false),
            line("ShipWorker.FishShowing.O1.Canceled", SellFishNode.ShipWorker1, useId: false),
        ], condition: () => isActive, priority: int.MaxValue),
    ];
    internal static void StartConversation()
    {
        isActive = true;
        Dialogue.DialogueController.instance.StartConversation(null);
    }
}


