
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Charlie : NodeEntry
{
    protected override Characters? Character => Characters.Charlie2;
    internal const string BeforeJA1 = "Charlie.BeforeJA1";
    internal const string BeforeJA2 = "Charlie.BeforeJA2";
    internal const string BeforeJA3 = "Charlie.BeforeJA3";
    internal const string AfterJA1 = "Charlie.AfterJA1";
    internal const string AfterJA2 = "Charlie.AfterJA2";
    internal const string GoldMedal1 = "Charlie.GoldMedal1";
    internal const string GoldMedal2 = "Charlie.GoldMedal2";
    internal const string AfterGoldMedal = "Charlie.AfterGoldMedal";
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 24, digit2, [2, 5, 10, 14, 15, 16, 21, 23], [
                new(3, emote(Emotes.EyesClosed, Original)),
                new(6, emote(Emotes.Normal, Original)),
                new(20, emote(Emotes.Happy, Original)),
                new(23, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 7, digit2, [2, 3, 4, 7], [new(6, emote(Emotes.Happy, Original))]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 6, digit2, [2, 4, 6], [new(5, emote(Emotes.Happy, Original))]),
        ], condition: () => _bJA && NodeDone(BeforeJA3)),

        new(AfterJA1, [
            line(1, Original),
            lineif(() => Flags.JATriggeredByJon, "Jon.02", "Alex.02", Original),
            lines(3, 7, digit2, [3, 7], [new(6, emote(Emotes.Happy, Original))]),
            done(),
            cont(-3),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA2, [
            line(1, Original),
            @if(() => _HM,
                lines(2, 3, digit2("HM", ""), [2], [new(3, emote(Emotes.Happy, Original))]),
                lines(2, 3, digit2("L", ""), [2], [new(3, emote(Emotes.Happy, Original))])
            ),
        ], condition: () => _aJA && NodeDone(AfterJA1), priority: -1),

        new(GoldMedal1, [
            lines(1, 11, digit2, [1, 4, 5,7, 10, 11]),
            line(12, Player, condition: () => GetBool(Const.STags.TalkedAboutCharlieOnce)),
            lines(13, 33, digit2, [16, 19, 21, 24], [
                new(20, emote(Emotes.EyesClosed, Original)),
                new(23, wait(3f)),
                new(23, emote(Emotes.Normal, Original)),
                new(23, emote(Emotes.Surprise, Player)),
                new(24, emote(Emotes.Normal, Player)),
            ]),
            @if(() => !GetBool(Const.STags.TalkedAboutWilOnce), lines(1, 3, digit2("TalkingAboutWilFirst"), [1, 3])),
            lines(34, 37, digit2, [37]),
            state(Const.Events.GoldMedal, NodeStates.Stage1),
            done(),
        ], condition: () => NodeIP(Const.Events.GoldMedal) && NodeYet(GoldMedal1), priority: 10),

        new(GoldMedal2, [
            lines(1, 3, digit2, [2]),
            @if(() => NodeYet(GoldMedal2), lines(4, 6, digit2("First"), [6])),
            done(),
        ], condition: () => NodeS1(Const.Events.GoldMedal), priority: 10),

        new(AfterGoldMedal, [
            lines(digit2, Original),
        ], condition: () => GoldMedalEnd.EventDoneInThisGame, priority: 10),
    ];
    private static bool eventSet = false;
    internal override void OnGameStarted()
    {
        if (!eventSet)
        {
            eventSet = true;
            GoldMedalEnd.OnPreparing += () =>
            {
                var ch = Ch(Characters.Charlie2);
                ch.transform.position = new(620.7261f, 132.8165f, 408.0128f);
                ch.transform.localRotation = Quaternion.Euler(0, 15.2164f, 0);
                Sidequel.Character.Pose.Set(ch.transform, Poses.Standing);
            };
        }
    }
}

