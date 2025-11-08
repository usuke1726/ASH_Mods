
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Noah : NodeEntry
{
    internal const string Start1 = "Noah.Start1";
    internal const string Start2 = "Noah.Start2";
    internal const string Start3 = "Noah.Start3";
    internal const string Start4 = "Noah.Start4";
    internal const string Sunscreen1 = "Noah.Sunscreen1";
    internal const string Sunscreen2 = "Noah.Sunscreen2";
    internal const string SunscreenAccept = "Noah.Sunscreen1.O1";
    internal const string SunscreenRefuse = "Noah.Sunscreen1.O2";
    protected override Characters? Character => Characters.BreakfastKid;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 6, digit2, [1, 4, 5], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(6, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeYet(Start1)),

        new(Start2, [
            lines(1, 8, digit2, [1, 4, 8], [
                new(7, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2)),

        new(Start3, [
            lines(1, 13, digit2, [3, 4]),
            @if(() => _H,
                lines(1, 6, digit2("H", ""), [1, 2, 3, 6], [new(4, emote(Emotes.Happy, Original))]),
                lines(1, 28, digit2("ML", ""), [1, 2, 3, 4, 9, 10, 19, 25, 27], [
                    new(8, emote(Emotes.Happy, Original)),
                    new(11, emote(Emotes.Normal, Original)),
                ])
            ),
            done(),
        ], condition: () => NodeDone(Start2) && NodeYet(Start3)),

        new(Start4, [
            lines(1, 7, digit2, [3, 5, 7], [new(6, emote(Emotes.Happy, Original))]),
        ], condition: () => NodeDone(Start3)),

        new(Sunscreen1, [
            line(1, Player),
            lineif(() => NodeDone(Start1), "HasMet.02", "HasNotMet.02", Original),
            done(Start1),
            lines(3, 10, digit2, [3, 4, 5], [
                new(7, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? SunscreenAccept : SunscreenRefuse),
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeYet(Sunscreen1), priority: 10),

        new(Sunscreen2, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? SunscreenAccept : SunscreenRefuse),
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeRefused(Sunscreen1), priority: 10),

        new(SunscreenAccept, [
            lines(1, 15, digit2, [7, 11, 12, 15], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(6, emote(Emotes.Happy, Original)),
                new(7, item(Items.HalfUsedSunscreen)),
                new(8, emote(Emotes.Normal, Original)),
                new(13, emote(Emotes.Happy, Original)),
            ]),
            state(Const.Events.Sunscreen, NodeStates.Stage1),
            done(Sunscreen1),
        ], condition: () => false),

        new(SunscreenRefuse, [
            lines(1, 2, digit2, []),
            state(Sunscreen1, NodeStates.Refused),
        ], condition: () => false),
    ];
}

