
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class SunhatDeer : NodeEntry
{
    internal const string Start1 = "SunhatDeer.Start1";
    internal const string Start2 = "SunhatDeer.Start2";
    internal const string High3 = "SunhatDeer.High3";
    internal const string MidLow3 = "SunhatDeer.MidLow3";
    internal const string MidLow4 = "SunhatDeer.MidLow4";
    internal const string MidLow5 = "SunhatDeer.MidLow5";
    internal const string MidLow6 = "SunhatDeer.MidLow6";
    internal const string MidLow7 = "SunhatDeer.MidLow7";
    protected override Characters? Character => Characters.SunhatDeer;
    private bool HasBorrowedOnce => GetBool(Const.STags.HasBorrowedFishingRodOnce);
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 11, digit2, [1, 3, 7, 8, 9], [
                new(10, emote(Emotes.Happy, Original)),
                new(11, emote(Emotes.Normal, Original)),
            ]),
            lineif(() => _HM, "HM12", "L12", Player),
            done(),
        ], condition: () => NodeYet(Start1), priority: 10),

        new(Start2, [
            lines(1, 8, digit2, [1, 2, 8], [
                new(7, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2)),

        new(High3, [
            lines(1, 3, digit2, [1, 3]),
        ], condition: () => _H && NodeDone(Start2)),

        new(MidLow3, [
            lines(1, 13, digit2, [1, 2, 4, 5, 13]),
            done(),
        ], condition: () => _ML && NodeDone(Start2) && NodeYet(MidLow3)),

        new(MidLow4, [
            lines(1, 2, digit2, [2]),
            @if(() => HasBorrowedOnce,
                lines(1, 4, digit2("BorrowedRodOnce"), [1, 2, 3], [new(4, emote(Emotes.Happy, Original))]),
                lines(1, 16, digit2("NotBorrowed"), [1, 2, 3, 4, 7, 11, 12, 13, 14])
            ),
            done(),
        ], condition: () => NodeDone(MidLow3) && NodeYet(MidLow4)),

        new(MidLow5, [
            lines(1, 5, digit2, [3, 4, 5]),
        ], condition: () => NodeDone(MidLow4) && !HasBorrowedOnce),

        new(MidLow6, [
            lines(1, 6, digit2, [1, 2, 3, 5], [new(6, emote(Emotes.Happy, Original))]),
            done(),
        ], condition: () => NodeDone(MidLow4) && HasBorrowedOnce && NodeYet(MidLow6), priority: 1),

        new(MidLow7, [
            lines(1, 5, digit2, [1, 2, 5], [new(4, emote(Emotes.Happy, Original))]),
        ], condition: () => NodeDone(MidLow4) && HasBorrowedOnce),
    ];
}

