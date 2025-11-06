
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Julie : NodeEntry
{
    internal const string Start1 = "Julie.Start1";
    internal const string Start2 = "Julie.Start2";
    internal const string Start3 = "Julie.Start3";
    internal const string AfterBSB = "Julie.AfterBSB";
    protected override Characters? Character => Characters.Julie;
    private bool IsAfterBSB => NodeDone(BeachstickGameStartPoint.StartGame);
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 10, digit2, [1, 3, 4, 7, 9], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeYet(Start1) && !IsAfterBSB),

        new(Start2, [
            lines(1, 6, digit2, [1, 3, 4, 6], [new(5, emote(Emotes.Happy, Original))]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2) && !IsAfterBSB),

        new(Start3, [
            lines(1, 3, digit2, [1]),
            lineif(() => _H, "H04", "ML04", Player),
        ], condition: () => NodeDone(Start2) && !IsAfterBSB),

        new(AfterBSB, [
            lines(1, 6, digit2, [2, 6], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
        ], condition: () => IsAfterBSB),
    ];
}

