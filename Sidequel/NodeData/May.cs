
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;
using Sidequel.System.Patrol;

namespace Sidequel.NodeData;

internal class May : NodeEntry
{
    internal const string BeforeJA1 = "May.BeforeJA1";
    internal const string BeforeJA2 = "May.BeforeJA2";
    internal const string Nothing = "May.Nothing";
    internal const string AfterJA1 = "May.AfterJA1";
    internal const string PatrolAccept = "May.AfterJA1.O1";
    internal const string PatrolRefuse = "May.AfterJA1.O2";
    internal const string AfterJA2 = "May.AfterJA2";
    internal const string Patrol1 = "May.Patrol1";
    internal const string Patrol2 = "May.Patrol2";
    internal const string PatrolPlace = "May.PatrolPlace";
    internal const string Patrol3 = "May.Patrol3";
    internal const string AfterPatrol = "May.AfterPatrol";
    internal const string Sunscreen1 = "May.Sunscreen1";
    internal const string SunscreenHold = "May.Sunscreen1.O1";
    internal const string SunscreenAccept = "May.Sunscreen1.O2";
    internal const string SunscreenRefuse = "May.Sunscreen1.O3";
    internal const string Sunscreen2 = "May.Sunscreen2";
    private const string Claire = "Claire";
    protected override Characters? Character => Characters.AuntMay;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 13, digit2, [1, 4, 5, 6, 7, 13]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 7, digit2, i => i switch {
                5 => Player,
                1 or 2 or 3 => Original,
                _ => Claire
            }, [
                new(4, emote(Emotes.Happy, Claire)),
                new(7, emote(Emotes.Normal, Claire)),
            ]),
            @if(() => GetBool(Const.STags.HasClimbedPeakOnce),
                lines(1, 7, digit2("HasClimbed"), i => i switch {
                    1 or 3 or 4 or 7 => Player,
                    2 or 5 => Claire,
                    _ => Original
                }),
                lines(1, 3, digit2("HasNotClimbed"), i => i switch {
                    2 => Claire,
                    _ => Player
                })
            ),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(Nothing, [
            lines(1, 6, digit2, [6], [
                new(4, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
            ]),
        ], condition: () => NodeDone(BeforeJA2) || NodeDone(AfterPatrol), priority: -1),

        new(AfterJA1, [
            lines(1, 8, digit2, [1, 6], replacer: Const.formatJATrigger),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? PatrolAccept : PatrolRefuse),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(PatrolAccept, [
            lines(1, 9, digit2, [8], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(9, emote(Emotes.Happy, Original)),
            ]),
            done(AfterJA1),
            command(Data.CreateCheckpoints),
            state(Const.Events.Patrol, NodeStates.InProgress),
        ], condition: () => false),

        new(PatrolRefuse, [
            lines(1, 2, digit2, []),
            state(AfterJA1, NodeStates.Refused),
        ], condition: () => false),

        new(AfterJA2, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? PatrolAccept : PatrolRefuse),
        ], condition: () => NodeRefused(AfterJA1)),

        new(Patrol1, [
            line(1, Original),
            lineif(() => PassedCount < 3, "lt3.02", "ge3.02", Player),
            lineif(() => PassedCount < 3, "lt3.03", "ge3.03", Player,
                replacer: s => ReplaceToPlaceI18nString(s, true, false)),
            line(4, Player, replacer: s => ReplaceToPlaceI18nString(s, true, true)),
            line(5, Original),
        ], condition: () => IsPatrolActive && !IsPatrolFinishing && PassedCount < PatrolBorderNum),

        new(Patrol2, [
            lines(1, 5, digit2, [2, 3, 4]),
            line("place1", Original, replacer: s => ReplaceToPlaceI18nString(s, false, false)),
            @if(() => TalkingPatsPoint && !HasTalkedAboutPatsPoint,
                lines(1, 4, digit2("place.PatsPoint"), [1, 3, 4], [new(1, command(() => HasTalkedAboutPatsPoint = true))]),
                line("place1.default", Player)
            ),
            @if(() => UnpassedCount == 1, "end"),
            line("place2", Original, replacer: s => ReplaceToPlaceI18nString(s, false, true)),
            @if(() => TalkingPatsPoint && !HasTalkedAboutPatsPoint,
                lines(1, 4, digit2("place.PatsPoint"), [1, 3, 4], [new(1, command(() => HasTalkedAboutPatsPoint = true))]),
                line("place2.default", Player)
            ),
            anchor("end"),
            emote(Emotes.Happy, Original),
            @if(() => UnpassedCount <= 2,
                lineif(() => UnpassedCount == 1, "onlyone", "onlytwo", Original),
                line(6, Original)
            ),
            line(7, Player),
        ], condition: () => IsPatrolActive && !IsPatrolFinishing && PassedCount >= PatrolBorderNum),

        new(Patrol3, [
            lines(1, 5, digit2, [2, 5], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
                new(5, item(Items.Coin, 40)),
            ]),
            done(Const.Events.Patrol),
            cont(-15),
            command(PassingStateRegistory.OnEventDone),
        ], condition: () => IsPatrolActive && IsPatrolFinishing),

        new(AfterPatrol, [
            lines(1, 5, digit2, [5], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(4, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Const.Events.Patrol) && NodeYet(AfterPatrol)),

        new(Sunscreen1, [
            lines(1, 14, digit2, [1, 4, 5], [
                new(10, transition(() => {})),
                new(10, wait(1f)),
                new(10, emote(Emotes.Happy, Original)),
                new(11, emote(Emotes.Normal, Original)),
                new(12, emote(Emotes.Happy, Original)),
                new(13, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2", "O3"]),
            next(() => LastSelected switch {
                0 => SunscreenHold,
                1 => SunscreenAccept,
                _ => SunscreenRefuse,
            }),
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeYet(Sunscreen1), priority: 5),
        new(Sunscreen2, [
            lines(1, 3, digit2, [1], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2", "O3"]),
            next(() => LastSelected switch {
                0 => SunscreenHold,
                1 => SunscreenAccept,
                _ => SunscreenRefuse,
            }),
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeIP(Sunscreen1), priority: 5),

        new(SunscreenHold, [
            emote(Emotes.Happy, Original),
            @if(() => NodeIP(Sunscreen1),
                line($"{Sunscreen2}.O1.01", Original, useId: false),
                lines(1, 3, digit2, [], [new(2, emote(Emotes.Normal, Original))])
            ),
            state(Sunscreen1, NodeStates.InProgress),
        ], condition: () => false),
        new(SunscreenAccept, [
            line(1, Original),
            @if(() => Items.CoinsNum < 10, "shortOnCash"),
            item(Items.Coin, -10),
            item(Items.WeakSunscreen),
            emote(Emotes.Happy, Original),
            line(2, Original),
            done(Sunscreen1),
            end(),
            anchor("shortOnCash"),
            lines(1, 5, digit2("shortOnCash"), [1, 2], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
            state(Sunscreen1, NodeStates.InProgress),
        ], condition: () => false),
        new(SunscreenRefuse, [
            lines(1, 7, digit2, [1, 2, 3, 7]),
            state(Sunscreen1, NodeStates.Refused),
        ], condition: () => false),
    ];

    private const int PatrolBorderNum = 20;
    private const string HasTalkedAboutPatsPointTag = "HasTalkedAboutPatsPointInPatrolEvent";
    private static bool HasTalkedAboutPatsPoint { get => STags.GetBool(HasTalkedAboutPatsPointTag); set => STags.SetBool(HasTalkedAboutPatsPointTag, value); }
    private static bool TalkingPatsPoint => checkpoint == Const.PatrolCheckpoints.PatsPointBackSide || checkpoint == Const.PatrolCheckpoints.PatsPointFrontSide;
    private static bool IsPatrolActive => NodeIP(Const.Events.Patrol);
    private static bool IsPatrolFinishing => UnpassedCount == 0;
    private static int PassedCount => PassingStateRegistory.PassedCount;
    private static int UnpassedCount => PassingStateRegistory.UnpassedCount;
    private static bool PassedAllCheckpoints => PassingStateRegistory.PassedAllCheckpoints;
    private static Const.PatrolCheckpoints checkpoint = default;
    private static string ReplaceToPlaceI18nString(string mes, bool isPatrol1, bool isSecondTime)
    {
        if (isPatrol1)
        {
            HashSet<Const.PatrolCheckpoints> invalidCheckpoints = [
                Const.PatrolCheckpoints.PatsPointFrontSide,
                Const.PatrolCheckpoints.PatsPointBackSide,
            ];
            while (invalidCheckpoints.Contains(
                checkpoint = PassingStateRegistory.GetRandomUnpassedCheckpointID()
            )) { }
        }
        else if (isSecondTime)
        {
            var prev = checkpoint;
            while ((checkpoint = PassingStateRegistory.GetRandomUnpassedCheckpointID()) == prev) { }
        }
        else
        {
            checkpoint = PassingStateRegistory.GetRandomUnpassedCheckpointID();
        }
        return mes.Replace("{{Place}}", I18nLocalize($"node.May.PatrolPlace.{checkpoint}"));
    }
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            Ch(Characters.AuntMay).transform.GetComponentInChildren<NPCIKAnimator>().lookAtPlayerRadius = 8f;
        });
    }
}

