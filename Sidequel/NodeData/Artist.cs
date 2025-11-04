
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Artist : NodeEntry
{
    protected override Characters? Character => Characters.Artist1;
    internal const string HighMid1 = "Marley.HighMid1";
    internal const string High2 = "Marley.High2";
    internal const string High3 = "Marley.High3";
    internal const string Mid2 = "Marley.Mid2";
    internal const string MidLow2 = "Marley.MidLow2";
    internal const string MidLow3 = "Marley.MidLow3";
    protected override Node[] Nodes => [
        new(HighMid1, [
            lines(1, 7, digit2, [2, 4, 7]),
            done(),
        ], condition: () => _HM && NodeYet(HighMid1)),

        new(High2, [
            lines(1, 7, digit2, [2, 4, 7]),
            done(),
        ], condition: () => _H && NodeDone(HighMid1) && NodeYet(High2)),

        new(High3, [
            lines(1, 4, digit2, [4]),
        ], condition: () => _H && NodeDone(High2)),

        new(Mid2, [
            lines(1, 58, digit2, [2, 3, 4, 12, 13, 14, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 29, 30, 33, 34, 35, 36, 37, 38, 41, 48, 49, 51, 52, 53, 58]),
            done(),
        ], condition: () => _M && NodeDone(HighMid1) && NodeYet(Mid2)),

        new(MidLow2, [
            lines(1, 42, digit2, [4, 5, 6, 14, 15, 16, 23, 38], [
                new(33, emote(Emotes.Surprise, Original)),
                new(35, emote(Emotes.Happy, Original)),
                new(36, emote(Emotes.Surprise, Player)),
                new(38, emote(Emotes.Normal, Player)),
                new(41, emote(Emotes.Normal, Original)),
            ]),
            @if(() => _M,
                lines(43, 53, digit2("M", ""), [43, 44, 45, 48, 49, 53], [
                    new(51, emote(Emotes.Happy, Original)),
                ]),
                lines(43, 50, digit2("L", ""), [43, 44, 45, 49, 50], [
                    new(47, emote(Emotes.Happy, Original)),
                ])
            ),
            done(),
        ], condition: () => (_L && NodeDone(HighMid1)) || (_M && NodeDone(Mid2))),

        new(MidLow3, [
            lines(1, 3, digit2, []),
            @if(() => NodeDone(MidLow3), line("04", Player), lines(4, 8, digit2("MidFirst"), [4, 5, 6, 8])),
            done(),
        ], condition: () => _ML && NodeDone(MidLow2)),
    ];
}

