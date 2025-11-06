
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class GlideKid : NodeEntry
{
    internal const string BeforeJA1 = "GlideKid.BeforeJA1";
    internal const string BeforeJA2 = "GlideKid.BeforeJA2";
    internal const string BeforeJA3 = "GlideKid.BeforeJA3";
    internal const string HighAfterJA1 = "GlideKid.HighAfterJA1";
    internal const string HighAfterJA2 = "GlideKid.HighAfterJA2";
    internal const string HighAfterJA3 = "GlideKid.HighAfterJA3";
    internal const string MidAfterJA1 = "GlideKid.MidAfterJA1";
    internal const string MidAfterJA2 = "GlideKid.MidAfterJA2";
    internal const string LowAfterJA1 = "GlideKid.LowAfterJA1";
    internal const string LowAfterLowAfterJA1 = "GlideKid.LowAfterLowAfterJA1";
    internal const string LowAfterJA2 = "GlideKid.LowAfterJA2";
    private bool FirstAfterJADone => NodeDone(HighAfterJA1) || NodeDone(MidAfterJA1) || NodeDone(LowAfterJA1);
    protected override Characters? Character => Characters.GlideKid;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 7, digit2, [2, 4, 5, 7]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 4, digit2, [3]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 3, digit2, [2]),
        ], condition: () => _bJA && NodeDone(BeforeJA2)),

        new(HighAfterJA1, [
            lines(1, 26, digit2, [2, 4, 5, 6, 8, 10, 12, 13, 16, 17, 18, 20, 21, 22, 23, 26]),
            done(),
        ], condition: () => _H && _aJA && !FirstAfterJADone),

        new(HighAfterJA2, [
            lines(1, 11, digit2, [3, 4, 5, 6, 7, 8, 9, 10], [
                new(7, wait(1f)),
                new(7, emote(Emotes.EyesClosed, Player)),
            ]),
            done(),
        ], condition: () => _H && FirstAfterJADone && NodeYet(HighAfterJA2)),

        new(HighAfterJA3, [
            lines(1, 6, digit2, [2, 4, 5]),
        ], condition: () => _H && NodeDone(HighAfterJA2)),

        new(MidAfterJA1, [
            lines(1, 29, digit2, [2, 4, 5, 6, 8, 10, 12, 13, 14, 15, 16, 17, 18, 21, 22, 23, 24, 25, 26, 29], [
                new(4, emote(Emotes.Surprise, Player)),
                new(5, emote(Emotes.EyesClosed, Player)),
                new(8, emote(Emotes.Normal, Player)),
                new(26, emote(Emotes.EyesClosed, Player)),
                new(28, emote(Emotes.Normal, Player)),
            ]),
            done(),
        ], condition: () => _M & !FirstAfterJADone),

        new(MidAfterJA2, [
            lines(1, 4, digit2, [2, 4]),
        ], condition: () => _M & FirstAfterJADone),

        new(LowAfterJA1,[
            lines(1, 3, digit2, [2]),
            lineif(() => Items.CoinsNum >= 300, "ge300.04", "lt300.04", Player),
            lines(5, 16, digit2, [7, 8, 9, 10, 11, 12, 13, 16]),
            done(),
        ], condition: () => _L && !FirstAfterJADone),

        new(LowAfterLowAfterJA1, [
            lines(1, 5, digit2, [2, 3, 4]),
            done(),
        ], condition: () => _L && NodeDone(LowAfterJA1) && NodeYet(LowAfterLowAfterJA1)),

        new(LowAfterJA2, [
            lines(1, 7, digit2, [2, 4, 7]),
        ], condition: () => _L && (
            NodeDone(HighAfterJA1) || NodeDone(MidAfterJA1) || NodeDone(LowAfterLowAfterJA1)
        )),
    ];
}

