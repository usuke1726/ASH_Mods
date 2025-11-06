
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class CompassGuy : NodeEntry
{
    internal const string BeforeJA1 = "CompassGuy.BeforeJA1";
    internal const string AfterJA1 = "CompassGuy.AfterJA1";
    internal const string Repeatable = "CompassGuy.Repeatable";
    protected override Characters? Character => Characters.CompassFox;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 17, digit2, [3, 5, 10, 14, 17], [
                new(1, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(AfterJA1, [
            lines(1, 9, digit2, [2, 4], [
                new(6, emote(Emotes.Happy, Original)),
                new(7, item(Items.Compass)),
                new(7, emote(Emotes.Normal, Original))
            ]),
            @if(() => _HM,
                lines(10, 20, digit2("HM", ""), [10, 11, 12, 13, 19, 20]),
                lines(10, 12, digit2("L", ""), [10, 11], [new(12, emote(Emotes.Happy, Original))])
            ),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(Repeatable, [
            lines(1, 7, digit2, [1, 5, 6], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
                new(7, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => true, priority: -1),
    ];
}

