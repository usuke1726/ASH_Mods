
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class RunningGoat : NodeEntry
{
    internal const string BeforeJA1 = "RunningGoat.BeforeJA1";
    internal const string BeforeJA2 = "RunningGoat.BeforeJA2";
    internal const string AfterJA1 = "RunningGoat.AfterJA1";
    internal const string AfterJA2 = "RunningGoat.AfterJA2";
    internal const string HighMidGoldMedal1 = "RunningGoat.HighMidGoldMedal1";
    internal const string LowGoldMedal1 = "RunningGoat.LowGoldMedal1";
    internal const string GoldMedal2 = "RunningGoat.GoldMedal2";
    internal const string GoldMedal3 = "RunningGoat.GoldMedal3";
    internal const string GoldMedal4 = "RunningGoat.GoldMedal4";
    internal const string GoldMedalByRabbit = "RunningGoat.GoldMedalByRabbit";
    internal const string AfterGoldMedal = "RunningGoat.AfterGoldMedal";
    protected override Characters? Character => Characters.RunningGoat;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 14, digit2, [1, 3, 5, 7, 9, 12, 14]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 6, digit2, [3, 6]),
        ], condition: () => _bJA && NodeDone(BeforeJA1)),

        new(AfterJA1, [
            lines(1, 2, digit2, [], replacer: Const.formatJATrigger),
            lines(3, 12, digit2, [3, 6, 9, 11, 12]),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1), priority: -1),

        new(AfterJA2, [
            lines(1, 3, digit2, [3]),
        ], condition: () => _aJA && NodeDone(AfterJA1), priority: -1),

        new(HighMidGoldMedal1, [
            line(1, Player),
            @if(() => NodeDone(BeforeJA1),
                lines(1, 2, digit2("BeforeJA1Done"), [1, 2]),
                lines(1, 1, digit2("BeforeJA1Yet"), [1])
            ),
            lines(2, 17, digit2, [3, 4, 5, 6, 8, 14, 15, 16]),
            state(Const.Events.GoldMedal, NodeStates.InProgress),
            tag(Const.STags.GoldMedalTriggeredByGoat, true),
        ], condition: () => _aJA && Items.Has(Items.GoldMedal) && NodeYet(Const.Events.GoldMedal) && _HM),

        new(LowGoldMedal1, [
            lines(1, 13, digit2, [1, 2, 4, 11, 12]),
            state(Const.Events.GoldMedal, NodeStates.InProgress),
            tag(Const.STags.GoldMedalTriggeredByGoat, true),
        ], condition: () => _aJA && Items.Has(Items.GoldMedal) && NodeYet(Const.Events.GoldMedal) && _L),

        new(GoldMedal2, [
            lines(1, 5, digit2, [1, 2, 5]),
        ], condition: () => NodeIP(Const.Events.GoldMedal) && GetBool(Const.STags.GoldMedalTriggeredByGoat)),

        new(GoldMedal3, [
            lines(1, 10, digit2, [1, 3, 4, 6, 8, 9]),
            done(),
        ], condition: () => NodeS1(Const.Events.GoldMedal) && NodeYet(GoldMedal3) && GetBool(Const.STags.GoldMedalTriggeredByGoat)),

        new(GoldMedal4, [
            lines(1, 3, digit2, [2]),
        ], condition: () => (
            NodeS1(Const.Events.GoldMedal) && NodeDone(GoldMedal3) && GetBool(Const.STags.GoldMedalTriggeredByGoat)
        ) || (
            NodeDone(GoldMedalByRabbit) && NodeActive(Const.Events.GoldMedal)
        )),

        new(GoldMedalByRabbit, [
            lines(1, 6, digit2, [1, 2, 4, 6]),
            done(),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && !GetBool(Const.STags.GoldMedalTriggeredByGoat) && NodeYet(GoldMedalByRabbit), priority: 10),

        new(AfterGoldMedal, [
            @if(() => GetBool(Const.STags.GoldMedalTriggeredByGoat),
                lines(1, 2, digit2("TriggeredByGoat"), [], [
                    new(1, emote(Emotes.Happy, Original)),
                ]),
                lines(1, 1, digit2("TriggeredByRabbit"), [])
            ),
            emote(Emotes.Happy, Original),
            lines(3, 4, digit2, [4]),
            cont(-5, condition: () => NodeYet(AfterGoldMedal)),
            done(),
        ], condition: () => NodeDone(Const.Events.GoldMedal)),
    ];

    private static bool eventSet = false;
    internal override void OnGameStarted()
    {
        if (!eventSet)
        {
            eventSet = true;
            GoldMedalEnd.OnPreparing += () =>
            {
                var ch = Ch(Characters.RunningGoat);
                Sidequel.Character.Pose.Set(ch.transform, Poses.Standing);
                var path = ch.transform.GetComponent<PathNPCMovement>();
                path.maxSpeed = 0.001f;
                path.enabled = false;
                ch.transform.GetComponent<CapsuleCollider>().enabled = true;
                ch.transform.GetComponent<Rigidbody>().isKinematic = true;
                var range = ch.transform.GetComponent<RangedInteractable>();
                range.range = 4f;
                Traverse.Create(range).Field("rangeSqr").SetValue(16f);
                ch.transform.position = new(665.7356f, 140.2126f, 614.4457f);
                ch.transform.localRotation = Quaternion.Euler(0, 116.477f, 0);
            };
        }
    }
}

