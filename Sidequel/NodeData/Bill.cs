
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.Dialogue.Actions;
using Sidequel.System.Fishing;

namespace Sidequel.NodeData;

internal class Bill : NodeEntry
{
    internal const string BeforeJA = "Bill.BeforeJA";
    internal const string AfterJA = "Bill.AfterJA";
    internal const string Repeatable1 = "Bill.Repeatable1";
    internal const string Hook = "Bill.Hook";
    internal const string MidLowAfterGettingRod = "Bill.MidLowAfterGettingRod";
    internal const string StartFishing = "Bill.StartFishing";
    internal const string AfterFishing = "Bill.AfterFishing";
    protected override Characters? Character => Characters.Bill;
    private static bool IsHoldingRod()
    {
        var player = Context.player;
        var heldItem = player.heldItem;
        return heldItem != null && heldItem.name.StartsWith("FishingRod");
    }
    private static bool HasFishingRod => Items.Has(Items.FishingRod) || IsHoldingRod();
    private static bool startFishing = false;
    protected override Node[] Nodes => [
        new(BeforeJA, [
            lines(1, 5, digit2, [1, 3, 5]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA)),

        new(AfterJA, [
            line(1, Original, replacer: Const.formatJATrigger),
            lines(2, 12, digit2, [2, 6, 10], [
                new(9, emote(Emotes.Happy, Original)),
                new(10, emote(Emotes.Normal, Original)),
                new(10, item(Items.Bait, 5)),
            ]),
            cont(-3),
            lineif(() => _HM, "HM13", "L13", Player),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA)),

        new(Repeatable1, [
            lines(1, 3, digit2, [1, 3]),
        ], condition: () => true, priority: int.MinValue),

        new(MidLowAfterGettingRod, [
            lines(1, 5, digit2, [1, 2], [new(5, emote(Emotes.Happy, Original))]),
            command(BillScene.Activate),
            done(),
        ], condition: () => _ML && HasFishingRod && NodeYet(Const.Events.Fishing) && !BillScene.IsActive && NodeYet(MidLowAfterGettingRod)),

        new(StartFishing, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "end"),
            emote(Emotes.Happy, Original),
            lines(1, 3, digit2("O2"), [3]),
            emote(Emotes.Normal, Original),
            command(() => startFishing = true),
            end(),
            anchor("end"),
            line("O1.01", Original),
        ], condition: () => _ML && HasFishingRod && NodeYet(Const.Events.Fishing) && NodeDone(MidLowAfterGettingRod),
            onConversationFinish: () => {
            BillScene.Activate();
            if(startFishing){
                BillScene.TryToStart();
            }
        }),

        new(AfterFishing, [
            command(() => afterFishing = false),
            lines(1, 4, digit2, [1, 3], [
                new(2, emote(Emotes.Happy, Original)),
            ]),
            cont(-3),
            done(),
        ], condition: () => afterFishing && NodeYet(AfterFishing)),
    ];
    internal static bool afterFishing = false;
    internal override void OnGameStarted()
    {
        afterFishing = false;
    }
}

internal class Fishing : NodeEntry
{
    internal const string Entry = "MidLowFishing.Entry";
    internal const string MidLowFishing = "MidLowFishing";
    internal const string MidLowFishing1 = "MidLowFishing1";
    internal const string MidLowFishing2 = "MidLowFishing2";
    internal const string MidLowFishing3 = "MidLowFishing3";
    protected override Characters? Character => null;
    internal const string Bill = "Bill";
    internal static bool End { get; private set; }
    private static Func<int, string> sp(HashSet<int> indexes) => i => indexes.Contains(i) ? Player : Bill;
    private static Node node(string id, List<BaseAction> actions) => new(id, actions, condition: () => false, onConversationFinish: () => End = true);
    private static int progress = 0;
    protected override Node[] Nodes => [
        new(Entry, [
            command(() => isActive = false),
            next(() => progress switch {
                0 => MidLowFishing1,
                1 => MidLowFishing2,
                _ => MidLowFishing3,
            }),
            command(() => progress++),
        ], condition: () => isActive, priority: int.MaxValue),

        node(MidLowFishing1, [
            lines(1, 38, digit2(MidLowFishing),
                sp([1, 3, 5, 6, 7, 8, 10, 11, 12, 13, 16, 17, 18, 23, 24, 27, 28, 29, 30, 31, 37, 38]), [
                new(4, emote(Emotes.Happy, Bill)),
                new(5, emote(Emotes.Normal, Bill)),
                new(15, emote(Emotes.Happy, Bill)),
                new(16, emote(Emotes.Normal, Bill)),
                new(25, emote(Emotes.Happy, Bill)),
                new(27, emote(Emotes.Happy, Player)),
                new(28, emote(Emotes.Normal, Bill)),
                new(28, emote(Emotes.Normal, Player)),
                new(36, emote(Emotes.Happy, Bill)),
            ], useId: false),
            emote(Emotes.Normal, Bill),
        ]),

        node(MidLowFishing2, [
            lines(39, 41, digit2(MidLowFishing), Player, useId: false),
        ]),

        node(MidLowFishing3, [
            done(Const.Events.Fishing),
            cont(-10),
            wait(1f),
            @switch(() => {
                if(BillScene.IsSwimming) return "Swimming";
                else if(BillScene.HasGotFish) return "GotFish";
                return "LoseFish";
            }),
            anchor("Swimming"),
            lines(1, 6, digit2($"{MidLowFishing}.Swimming"), sp([1, 2, 4, 6]), [
                new(1, emote(Emotes.Happy, Player)),
                new(3, emote(Emotes.Happy, Bill)),
                new(4, emote(Emotes.Normal, Player)),
                new(4, emote(Emotes.Normal, Bill)),
            ], useId: false),
            emote(Emotes.Normal, Bill),
            end(),
            anchor("GotFish"),
            lines(1, 6, digit2($"{MidLowFishing}.GotFish"), sp([1, 3, 4, 5]), [
                new(3, emote(Emotes.Happy, Player)),
                new(4, emote(Emotes.Normal, Player)),
                new(6, emote(Emotes.Happy, Bill)),
            ], useId: false),
            emote(Emotes.Normal, Bill),
            end(),
            anchor("LoseFish"),
            lines(1, 6, digit2($"{MidLowFishing}.LoseFish"), sp([1, 3, 4, 5]), [
                new(6, emote(Emotes.Happy, Bill)),
            ], useId: false),
            emote(Emotes.Normal, Bill),
        ]),
    ];
    private static bool isActive = false;
    internal static void StartConversation()
    {
        End = false;
        isActive = true;
        Dialogue.DialogueController.instance.StartConversation(null);
    }
    internal static bool CanStart => NodeYet(MidLowFishing);
}

