
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Alex : NodeEntry
{
    protected override Characters? Character => Characters.ClimbingRhino3;
    internal const string TalkedAlexOnce = "TalkedAlexOnce";
    internal const string JA1 = "Alex.JA1";
    internal const string JA2 = "Alex.JA2";
    internal const string AfterJA1 = "Alex.AfterJA1";
    internal const string HighMidAfterJA2 = "Alex.HighMidAfterJA2";
    internal const string HighAfterJA2 = "Alex.HighAfterJA2";
    internal const string HighAfterJA3 = "Alex.HighAfterJA3";
    internal const string MidAfterJA2 = "Alex.MidAfterJA2";
    internal const string MidAfterJA3 = "Alex.MidAfterJA3";
    internal const string MidAfterJA4 = "Alex.MidAfterJA4";
    internal const string MidCartersConfessionToAlex = "Alex.MidCartersConfessionToAlex";
    internal const string MidAfterConfession = "Alex.MidAfterConfession";
    internal const string LowAfterJA2 = "Alex.LowAfterJA2";
    internal const string LowCartersConfessionToAlex = "Alex.LowCartersConfessionToAlex";
    internal const string LowAfterConfession = "Alex.LowAfterConfession";
    internal const string AfterCoinCompletion1 = "Alex.AfterCoinCompletion1";
    internal const string AfterCoinCompletion2 = "Alex.AfterCoinCompletion2";
    internal static bool HasAlexMoved { get; private set; } = false;
    protected override Node[] Nodes => [
        new(JA1, [
            tag(TalkedAlexOnce, true),
            tag(Const.STags.CoinsAtJA, () => Items.CoinsNum),
            lines(1, 18, digit2, [1, 2, 5, 9, 10, 15, 16, 17]),
            line(19, Player, replacer: s => Items.ReplaceApproxDiff(GetInt(Const.STags.CoinsAtJA), s)),
            lines(20, 22, digit2, []),
            @if(() => Items.CoinsNum >= 150,
                lines(1, 6, digit2("ge150"), [1, 2, 6]),
                lines(1, 11, digit2("lt150"), [1, 2, 6, 7, 8])
            ),
            lines(23, 25, digit2, [25], [new(24, emote(Emotes.Surprise, Player))]),
            transition(() => {
                MoveForJA();
                HasAlexMoved = true;
            }),
            wait(0.5f),
            lines(26, 28, digit2, Player),
            tag(Const.STags.JADone, true),
            tag(Const.STags.JATriggeredByJon, false),
            cont(-5),
            command(() => Tim.timeTriggeredJAByAlex = Time.time),
        ], condition: () => _bJA),

        new(JA2, [
            command(Face),
            lines(1, 3, digit2, [2]),
            @switch(() => {
                if(Items.CoinsNum >= 350) return "ge350";
                return Items.CoinsNum - STags.GetInt(Const.STags.CoinsAtJA) >= 150 ? "largeinc" : "smallinc";
            }),
            lines(1, 6, digit2("ge350"), [1, 3, 6], anchor: "ge350"),
            end(),
            lines(1, 5, digit2("lt350largeinc"), [1, 3, 6], anchor: "largeinc"),
            end(),
            lines(1, 4, digit2("lt350smallinc"), [1, 2, 3], anchor: "smallinc"),
            @switch(() => {
                if(!GetBool(AlexTalkedAboutMay)) return "may";
                if(!GetBool(AlexTalkedAboutWil)) return "wil";
                return "other";
            }),
            lines(1, 11, digit2("lt350smallinc.may"), [6, 7, 11], [new(10, emote(Emotes.Happy, Original))], anchor: "may"),
            tag(AlexTalkedAboutMay, true),
            end(),
            lines(1, 5, digit2("lt350smallinc.wil"), [5], anchor: "wil"),
            tag(AlexTalkedAboutWil, true),
            @if(() => GetBool(Const.STags.TalkedAboutWilOnce), "wilEnd", null),
            lines(6, 8, digit2("lt350smallinc.wil"), [6, 8]),
            lines(9, 14, digit2("lt350smallinc.wil"), [9, 10, 14], anchor: "wilEnd"),
            tag(Const.STags.TalkedAboutWilOnce, true),
            end(),
            lines(1, 2, digit2("lt350smallinc.other"), [], anchor: "other"),
            lineif(() => _HM, "lt350smallinc.other.HM03", "lt350smallinc.other.L03", Player),
            end(),
        ], condition: () => _aJA && !_JAJon),

        new(AfterJA1, [
            tag(TalkedAlexOnce, true),
            lines(1, 9, digit2, [2, 5, 9]),
            done(),
        ], condition: () => TriggeredByJon && NodeYet(AfterJA1)),

        new(HighAfterJA2, [
            lines(1, 22, digit2($"{HighMidAfterJA2}"), [5, 6, 7, 8, 9, 22], [
                new(8, emote(Emotes.EyesClosed, Player)),
                new(12, emote(Emotes.Normal, Player)),
            ], useId: false),
            lines(23, 31, digit2, [23, 24, 29, 30, 31]),
            done(),
        ], condition: () => TriggeredByJon && _H && NodeDone(AfterJA1) && NodeYet(HighAfterJA2)),

        new(MidAfterJA2, [
            lines(1, 22, digit2($"{HighMidAfterJA2}"), [5, 6, 7, 8, 9, 22], [
                new(8, emote(Emotes.EyesClosed, Player)),
                new(12, emote(Emotes.Normal, Player)),
            ], useId: false),
            lines(23, 26, digit2, [23, 25, 26]),
            transition(PrepareConfession),
        ], condition: () => TriggeredByJon && _M && NodeDone(AfterJA1) && NodeYet(MidCartersConfessionToAlex)),

        new(HighAfterJA3, [
            lines(1, 8, digit2, [1, 4, 5, 8]),
        ], condition: () => TriggeredByJon && _H && NodeDone(HighAfterJA2)),

        new(MidAfterJA3, [
            lines(1, 11, digit2, [1, 4, 5, 7, 8, 10, 11], [
                new(10, emote(Emotes.EyesClosed, Player)),
                new(11, emote(Emotes.Normal, Player)),
            ]),
            transition(PrepareConfession),
        ], condition: () => TriggeredByJon && _M && !ConfessionDone && NodeDone(HighAfterJA2)),

        new(MidCartersConfessionToAlex, [
            wait(1f),
            line("01", Player),
            lines(digit2("MidCartersConfession"), Player, useId: false),
            lines(2, 28, digit2, [7, 8, 15, 16, 18, 19, 23, 24, 27], [
                new(14, emote(Emotes.Happy, Original)),
                new(17, emote(Emotes.Normal, Original)),
                new(26, emote(Emotes.Happy, Original)),
            ]),
            cont(-20),
            done(),
        ], condition: () => false),

        new(MidAfterConfession, [
            command(TryToFace),
            lines(1, 5, digit2, [3, 5], [new(4, emote(Emotes.Happy, Original))]),
        ], condition: () => TriggeredByJon && _M && ConfessionDone && !Items.CoinsSavedUp),

        new(LowAfterJA2, [
            lines(1, 20, digit2, [5, 6, 7, 9, 10, 11, 12, 13, 18, 19, 20])
        ], condition: () => TriggeredByJon && _L && !ConfessionDone && NodeDone(AfterJA1)),

        new(LowCartersConfessionToAlex, [
            lines(1, 22, digit2("LowCartersConfession"), Player, useId: false),
            wait(1f),
            lines(23, 32, digit2("LowCartersConfession"), Player, useId: false),
            wait(1.5f),
            lines(digit2, []),
            cont(-20),
            done(),
        ], condition: () => false),

        new(LowAfterConfession, [
            command(TryToFace),
            lines(1, 7, digit2, [3, 4, 7], [new(5, emote(Emotes.Happy, Original))]),
        ], condition: () => ConfessionDone && !Items.CoinsSavedUp),

        new(AfterCoinCompletion1, [
            command(TryToFace),
            lines(1, 11, digit2, [2, 4, 7, 8, 11], [
                new(5, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Happy, Player)),
                new(11, emote(Emotes.Normal, Player)),
            ]),
            @if(() => STags.GetBool(TalkedAlexOnce), lines(digit2("TalkedOnce"), [1])),
            lines(12, 20, digit2, [13, 14, 15, 16, 17, 20], [
                new(12, emote(Emotes.Normal, Original)),
                new(18, emote(Emotes.Happy, Original)),
                new(20, emote(Emotes.Happy, Player)),
            ]),
            done(),
        ], condition: () => Items.CoinsSavedUp && NodeYet(AfterCoinCompletion1), priority: 10),

        new(AfterCoinCompletion2, [
            command(TryToFace),
            lines(1, 3, digit2, [1]),
            @if(() => _HM,
                lines(4, 7, digit2("HM", ""), [4, 7], [new(5, emote(Emotes.Happy, Original))]),
                lines(4, 7, digit2("L", ""), [4, 7], [new(5, emote(Emotes.Happy, Original))])
            ),
        ], condition: () => Items.CoinsSavedUp && NodeDone(AfterCoinCompletion1), priority: -1),
    ];

    private const string AlexTalkedAboutWil = "Alex.HasTalkedAboutWil";
    private const string AlexTalkedAboutMay = "Alex.HasTalkedAboutMay";
    private bool ConfessionDone => NodeDone(MidCartersConfessionToAlex) || NodeDone(LowCartersConfessionToAlex);
    private bool TriggeredByJon => _aJA && _JAJon;
    private ICanFace facer = null!;
    private void Face() => facer.TurnToFace(Context.player.transform);
    private void TryToFace()
    {
        if (IsStanding()) Face();
    }
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            HasAlexMoved = false;
            if (_aJA && !_JAJon) MoveForJA();
            var ch = Ch(Characters.ClimbingRhino3);
            facer = ch.gameObject.AddComponent<NPCFacer>();
        });
    }
    private void MoveForJA()
    {
        var ch = Ch(Characters.ClimbingRhino3);
        ch.transform.position = pos1;
        Sidequel.Character.Pose.Set(ch.transform, Poses.Standing);
    }
    private bool IsStanding()
    {
        var ch = Ch(Characters.ClimbingRhino3);
        List<Vector3> positions = [pos1, pos2];
        return positions.Any(p => (ch.transform.position - p).sqrMagnitude < 100);
    }
    private static readonly Vector3 pos1 = new(338.5625f, 41.038f, 168.6002f);
    private static readonly Vector3 pos2 = new(265.2325f, 268.2634f, 557.7928f);
    private void PrepareConfession()
    {
        Context.player.transform.position = new(258.5855f, 267.1069f, 560.9364f);
        Context.player.transform.localRotation = Quaternion.Euler(0, 109.1662f, 0);
        var ch = Ch(Characters.ClimbingRhino3);
        ch.transform.position = pos2;
        Sidequel.Character.Pose.Set(ch.transform, Poses.Standing);
        SetNext(MidCartersConfessionToAlex);
        HasAlexMoved = true;
    }
    internal static void ResetPosition()
    {
        var ch = Ch(Characters.ClimbingRhino3);
        ch.transform.position = new(323.41f, 398.97f, 626.03f);
        ch.transform.localRotation = Quaternion.Euler(0f, 302.5799f, 0);
        Sidequel.Character.Pose.Set(ch.transform, Poses.Sitting);
        HasAlexMoved = false;
    }
}

