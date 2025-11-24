
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Camper : NodeEntry
{
    protected override Characters? Character => Characters.Camper;
    internal const string Start1 = "Camper.Start1";
    internal const string High2 = "Camper.High2";
    internal const string BorrowRod1 = "Camper.BorrowRod1";
    internal const string BorrowRod2 = "Camper.BorrowRod2";
    internal const string MidBorrowingRod = "Camper.MidBorrowingRod";
    internal const string ReturnRod = "Camper.ReturnRod";
    internal const string BorrowRodAgain = "Camper.BorrowRodAgain";

    private bool BorrowingRod => Items.Has(Items.FishingRod);
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 13, digit2, [2, 5, 8, 13], [
                new(3, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
                new(9, emote(Emotes.Happy, Original)),
                new(12, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeYet(Start1)),

        new(High2, [
            lines(1, 9, digit2, [4, 7, 9]),
            done(),
        ], condition: () => NodeDone(Start1) && _H),

        new(BorrowRod1, [
            lines(1, 19, digit2, [4, 5, 7, 11,12, 13], [
                new(6, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
                new(15, emote(Emotes.Happy, Original)),
                new(18, emote(Emotes.Normal, Original)),
                new(19, item(Items.FishingRod)),
                new(19, emote(Emotes.Happy, Original)),
            ]),
            tag(Const.STags.HasBorrowedFishingRodOnce, true),
            done(),
            cont(-10),
        ], condition: () => NodeYet(High2) && _ML && NodeYet(BorrowRod1)),

        new(BorrowRod2, [
            lines(1, 13, digit2, [1, 3, 4, 5, 8, 9, 12], [
                new(10, emote(Emotes.Happy, Original)),
                new(11, emote(Emotes.Normal, Original)),
                new(12, item(Items.FishingRod)),
                new(13, emote(Emotes.Happy, Original)),
            ]),
            tag(Const.STags.HasBorrowedFishingRodOnce, true),
            done(),
            cont(-10),
        ], condition: () => NodeDone(High2) && _ML && NodeYet(BorrowRod2)),

        new(MidBorrowingRod, [
            lines(1, 5, digit2, [1, 3], [new(4, emote(Emotes.Happy, Original))]),
        ], condition: () => BorrowingRod && _M),

        new(ReturnRod, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            @if(() => LastSelected == 1, "return"),
            lines(1, 2, digit2("O1"), [], [new(1, emote(Emotes.Happy, Original))]),
            end(),
            anchor("return"),
            lines(3, 8, digit2, [4, 5, 7], [
                new(6, emote(Emotes.Happy, Original)),
                new(7, item(Items.FishingRod, -1)),
            ]),
            done(),
        ], condition: () => BorrowingRod && _L),

        new(BorrowRodAgain, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "borrow"),
            lines(1, 2, digit2("O2"), [], [new(1, emote(Emotes.Happy, Original))]),
            end(),
            anchor("borrow"),
            lines(3, 8, digit2, [4, 7], [
                new(5, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
                new(7, item(Items.FishingRod)),
                new(8, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => NodeDone(ReturnRod) && !BorrowingRod),
    ];
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            Ch(Characters.Camper).animator.SetBool("Happy", true);
        });
    }
}

