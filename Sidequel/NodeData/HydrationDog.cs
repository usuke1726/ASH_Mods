
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class HydrationDog : NodeEntry
{
    internal const string BeforeJA1 = "HydrationDog.BeforeJA1";
    internal const string BeforeJA2 = "HydrationDog.BeforeJA2";
    internal const string BeforeJA3 = "HydrationDog.BeforeJA3";
    internal const string BeforeJA4 = "HydrationDog.BeforeJA4";
    internal const string AfterJA1 = "HydrationDog.AfterJA1";
    internal const string AfterJA2 = "HydrationDog.AfterJA2";
    protected override Characters? Character => Characters.HydrationDog;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 9, digit2, [1, 4, 5, 9], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(6, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Normal, Original)),
                new(8, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 17, digit2, [3, 12, 13, 16], [
                new(11, emote(Emotes.Happy, Original)),
                new(12, emote(Emotes.Happy, Player)),
                new(14, emote(Emotes.Normal, Player)),
            ]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 4, digit2, [1, 4]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA2) && NodeYet(BeforeJA3)),

        new(BeforeJA4, [
            lines(1, 5, digit2, [3, 5], [
                new(2, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
        ], condition: () => _bJA && NodeDone(BeforeJA3)),

        new(AfterJA1, [
            lines(1, 8, digit2, [3, 8], [
                new(7, emote(Emotes.Happy, Original)),
            ], replacer: Const.formatJATrigger),
            cont(-3),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA2, [
            lines(1, 3, digit2, [2], [new(3, emote(Emotes.Happy, Original))]),
        ], condition: () => _aJA && NodeDone(AfterJA1)),
    ];
}

