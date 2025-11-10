
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class ShovelKid : NodeEntry
{
    internal const string Start1 = "ShovelKid.Start1";
    internal const string High2 = "ShovelKid.High2";
    internal const string High3 = "ShovelKid.High3";
    internal const string MidLow2 = "ShovelKid.MidLow2";
    internal const string Answer = "ShovelKid.Answer";
    internal const string Story1Start = "ShovelKid.Story1Start";
    internal const string Story1NotImplemented = "ShovelKid.Story1NotImplemented";
    internal const string AnyStoryActive = "ShovelKid.AnyStoryActive";
    internal const string ActiveStoryId = "ShovelKid.ActiveStoryId";
    protected override Characters? Character => Characters.ShovelKid;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 5, digit2, [2], [
                new(4, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
            ]),
            done(),
            @if(() => _ML, "answer"),
            lines(6, 7, digit2("H", ""), [6], [
                new(7, emote(Emotes.Happy, Original)),
            ]),
            end(),
            anchor("answer"),
            line("ML06", Player),
            done(MidLow2),
            next(() => Answer),
        ], condition: () => NodeYet(Start1)),

        new(High2, [
            lines(1, 4, digit2, [2, 4]),
            done(),
        ], condition: () => _H && NodeDone(Start1) && NodeYet(High2)),

        new(High3, [
            lines(1, 2, digit2, [2]),
        ], condition: () => _H && NodeDone(High2)),

        new(MidLow2, [
            lines(1, 2, digit2, [2]),
            done(),
            next(() => Answer),
        ], condition: () => _ML && NodeDone(Start1) && NodeYet(MidLow2)),

        new(Answer, [
            next(() => Story1Start),
        ], condition: () => false),

        new(Story1Start, [
            lines(1, 7, digit2, [1, 2, 3, 7], [
                new(6, emote(Emotes.Happy, Original)),
            ]),
            tag(AnyStoryActive, true),
            tag(ActiveStoryId, 1),
            done(),
        ], condition: () => false),

        new(Story1NotImplemented, [
            lines(1, 3, digit2, [3]),
        ], condition: () => GetBool(AnyStoryActive) && GetInt(ActiveStoryId) == 1),
    ];
}

