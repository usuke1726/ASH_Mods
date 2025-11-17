
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class RunningLizard : NodeEntry
{
    internal const string BeforeJA1 = "RunningLizard.BeforeJA1";
    internal const string BeforeJA2 = "RunningLizard.BeforeJA2";
    internal const string BeforeJA3 = "RunningLizard.BeforeJA3";
    internal const string AfterJA1 = "RunningLizard.AfterJA1";
    internal const string AfterJA2 = "RunningLizard.AfterJA2";
    internal const string GoldMedal1 = "RunningLizard.GoldMedal1";
    internal const string GoldMedal2 = "RunningLizard.GoldMedal2";
    internal const string GoldMedal3 = "RunningLizard.GoldMedal3";
    protected override Characters? Character => Characters.RunningLizard;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 7, digit2, [4, 5]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 6, digit2, [3, 4, 6]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 4, digit2, [4]),
        ], condition: () => _bJA && NodeDone(BeforeJA2)),

        new(AfterJA1, [
            lines(1, 9, digit2, [3, 6, 8]),
            cont(-3),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1), priority: -1),

        new(AfterJA2, [
            lines(1, 4, digit2, []),
        ], condition: () => NodeDone(AfterJA1), priority: -1),

        new(GoldMedal1, [
            lines(1, 11, digit2, [1, 2, 11]),
            done(),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeYet(GoldMedal1)),

        new(GoldMedal2, [
            lines(1, 3, digit2, [2]),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeDone(GoldMedal1)),

        new(GoldMedal3, [
            lines(1, 2, digit2, []),
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
                var ch = Ch(Characters.RunningLizard);
                Sidequel.Character.Pose.Set(ch.transform, Poses.Standing);
                var path = ch.transform.GetComponent<PathNPCMovement>();
                path.maxSpeed = 0.001f;
                path.enabled = false;
                ch.transform.GetComponent<CapsuleCollider>().enabled = true;
                ch.transform.GetComponent<Rigidbody>().isKinematic = true;
                var range = ch.transform.GetComponent<RangedInteractable>();
                range.range = 4f;
                Traverse.Create(range).Field("rangeSqr").SetValue(16f);
                ch.transform.position = new(672.7437f, 140.1992f, 617.9913f);
                ch.transform.localRotation = Quaternion.Euler(0, 172.5071f, 0);
            };
        }
    }
}

