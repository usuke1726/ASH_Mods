
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Tim : NodeEntry
{
    internal const string HighMidStart1 = "Tim.HighMidStart1";
    internal const string LowStartWithoutAlex = "Tim.LowStartWithoutAlex";
    internal const string LowStart1 = "Tim.LowStart1";
    internal const string Start2 = "Tim.Start2";
    internal const string Start3 = "Tim.Start3";
    internal const string AfterJA1 = "Tim.AfterJA1";
    internal const string HighMidAfterJA2 = "Tim.HighMidAfterJA2";
    private const string AlexId = "ClimbingRhino3";
    private bool ConfessionDone => NodeDone(Alex.MidCartersConfessionToAlex) || NodeDone(Alex.LowCartersConfessionToAlex);
    private bool TriggeredByJon => _aJA && _JAJon;
    internal static float timeTriggeredJAByAlex = -1;
    protected override Characters? Character => Characters.Tim3;
    protected override Node[] Nodes => [
        new(HighMidStart1, [
            lines(1, 26, digit2, i => i switch {
                1 or 2 or 6 or 26 => Player,
                (>= 10 and <= 16) or 24 => AlexId,
                _ => Original
            }, [
                new(7, emote(Emotes.EyesClosed, Original)),
                new(13, emote(Emotes.Normal, Original)),
            ]),
            done(),
            done(LowStart1),
        ], condition: () => _HM && NodeYet(HighMidStart1) && !Alex.HasAlexMoved && (_bJA || TriggeredByJon)),

        new(LowStart1, [
            lines(1, 13, digit2, [1, 4, 5, 6, 9, 13], [
                new(8, emote(Emotes.Happy, Original)),
                new(12, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeYet(HighMidStart1) && _aJA && _L && NodeYet(LowStart1)),

        new(Start2, [
            @if(() => NodeDone(HighMidStart1), line("HighMidStart1Done", Original)),
            lines(1, 4, digit2, []),
            @if(() => _HM,
                lines(1, 9, digit2("HM", ""), [1, 7, 8]),
                lines(1, 7, digit2("L", ""), [1, 2, 6, 7])
            ),
            done(),
        ], condition: () => NodeDone(LowStart1) && NodeYet(Start2)),

        new(Start3, [
            lines(1, 4, digit2, [3]),
        ], condition: () => NodeDone(Start2), priority: -10),

        new(AfterJA1, [
            lines(1, 3, digit2, [3]),
            done(),
        ], condition: () => NodeYet(AfterJA1) && timeTriggeredJAByAlex > 0 && Time.time - timeTriggeredJAByAlex < 10, priority: int.MaxValue),

        new(HighMidAfterJA2, [
            line(1, Original),
            line(2, Original, replacer: s => Items.ReplaceApproxDiff(GetInt(Const.STags.CoinsAtJA), s)),
            lines(3, 4, digit2, []),
            @if(() => _H,
                lines(1, 8, digit2("H", ""), [1, 2, 4, 5, 8]),
                lines(1, 9, digit2("M", ""), [1, 2, 3, 5, 6, 9])
            ),
            done(),
        ], condition: () => _aJA && !TriggeredByJon && _HM && NodeYet(HighMidAfterJA2)),
    ];
}

