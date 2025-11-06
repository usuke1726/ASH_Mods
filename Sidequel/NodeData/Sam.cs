
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Sam : NodeEntry
{
    internal const string High1 = "Sam.High1";
    internal const string High2 = "Sam.High2";
    internal const string HighSunscreen = "Sam.HighSunscreen";
    internal const string MidLow1 = "Sam.MidLow1";
    internal const string MidLowSunscreen1 = "Sam.MidLowSunscreen1";
    internal const string MidLowSunscreen2 = "Sam.MidLowSunscreen2";
    internal const string StrongSunscreen = "Sam.StrongSunscreen";
    internal const string WeakSunscreen = "Sam.WeakSunscreen";
    internal const string AfterSunscreen1 = "Sam.AfterSunscreen1";
    internal const string AfterSunscreen2 = "Sam.AfterSunscreen2";

    internal const string WeakSunscreenShowedTag = "ShowedWeakSunscreenToSam";
    protected override Characters? Character => Characters.DiveKid;
    private bool HasStrongSunscreen => Items.Has(Items.StrongSunscreen) || Items.Has(Items.HalfUsedSunscreen);
    private bool HasWeakSunscreen => Items.Has(Items.WeakSunscreen) || Items.Has(Items.Sunscreen);
    private bool ShowingWeakOne => HasWeakSunscreen && !GetBool(WeakSunscreenShowedTag);
    protected override Node[] Nodes => [
        new(High1, [
            lines(1, 15, digit2, [1, 2, 6, 9, 10, 13, 14, 15], [
                new(7, emote(Emotes.Surprise, Original)),
                new(8, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => _H && NodeYet(High1)),

        new(High2, [
            lines(1, 9, digit2, [2, 6, 7, 9], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
            ]),
            state(Const.Events.Sunscreen, NodeStates.InProgress),
            done(),
        ], condition: () => _H && NodeDone(High1) && NodeYet(High2)),

        new(HighSunscreen, [
            lines(1, 3, digit2, [2]),
        ], condition: () => _H && NodeActive(Const.Events.Sunscreen) && !ShowingWeakOne),

        new(MidLow1, [
            lines(1, 2, digit2, [1]),
            @if(() => NodeDone(High1),
                lines(1, 2, digit2("High1Done"), []),
                lines(1, 2, digit2("High1Yet"), [1])
            ),
            lines(3, 5, digit2, [3]),
            @if(() => _M, lines(1, 9, digit2("M", ""), [1, 2, 3, 4, 5, 6, 7, 9]), lines(1, 2, digit2("L", ""), [1, 2])),
            lines(6, 11, digit2, [11], [
                new(6, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Normal, Original)),
            ]),
            state(Const.Events.Sunscreen, NodeStates.InProgress),
            done(),
        ], condition: () => _ML && NodeYet(MidLow1) && NodeYet(Const.Events.Sunscreen)),

        new(MidLowSunscreen1, [
            lines(1, 4, digit2, [2, 4]),
            done(),
        ], condition: () => _ML && NodeActive(Const.Events.Sunscreen) && NodeYet(MidLowSunscreen1) && NodeDone(High2) && !ShowingWeakOne),

        new(MidLowSunscreen2, [
            lines(1, 4, digit2, [2, 4]),
        ], condition: () => _ML && NodeActive(Const.Events.Sunscreen) && !ShowingWeakOne, priority: -1),

        new(StrongSunscreen, [
            lines(1, 8, digit2, [2, 7], [
                new(3, @if(() => Items.Has(Items.StrongSunscreen), item(Items.StrongSunscreen, -1), item(Items.HalfUsedSunscreen, -1))),
                new(3, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
                new(7, item(Items.Coin, 30)),
                new(8, emote(Emotes.Happy, Original)),
            ]),
            done(Const.Events.Sunscreen),
        ], condition: () => NodeActive(Const.Events.Sunscreen) && HasStrongSunscreen, priority: 10),

        new(WeakSunscreen, [
            lines(1, 8, digit2, [2, 4, 6, 7], [
                new(3, wait(1f)),
            ]),
            tag(WeakSunscreenShowedTag, true),
        ], condition: () => NodeActive(Const.Events.Sunscreen) && ShowingWeakOne, priority: 8),

        new(AfterSunscreen1, [
            lines(1, 8, digit2, [1, 3, 4, 8]),
            lineif(() => HasWeakSunscreen, "HasWeakOne.09", "NotHasWeakOne.09", Player),
            done(),
        ], condition: () => NodeDone(Const.Events.Sunscreen) && NodeYet(AfterSunscreen1), priority: 5),

        new(AfterSunscreen2, [
            lines(1, 5, digit2, [1, 2, 5], [new(3, emote(Emotes.Happy, Original))]),
        ], condition: () => NodeDone(Const.Events.Sunscreen)),
    ];
}

