
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.NodeData;

internal class BeachstickKid : NodeEntry
{
    protected override Characters? Character => Characters.BeachstickballKid;
    private const string StickEvent = Const.Events.StickEvent;
    private static bool isTalking = false;

    internal const string Start = "BeachstickKid.Start";
    internal const string StickInProgress = "BeachstickKid.InProgress";
    internal const string StickComplete = "BeachstickKid.Complete";
    internal const string HighAfterCompleted = "BeachstickKid.HighAfterCompleted";
    internal const string MidLowAfterCompleted = "BeachstickKid.MidLowAfterCompleted";
    internal const string WaitGameStart = "BeachstickKid.WaitGameStart";
    internal const string AfterGame = "BeachstickKid.AfterGame";
    protected override Node[] Nodes => [
        new(Start, [
            command(OnStartFirstTalk),
            lines(1, 12, digit2, [2, 4, 5, 7, 8, 11]),
            @if(() => _L, "Accept"),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "Accept"),
            lines(1, 7, digit2("O2"), [3, 6]),
            option(["O3", "O4"]),
            @if(() => LastSelected == 0, "Accept"),
            lines(1, 6, digit2("O4"), [5, 6]),
            anchor("Accept"),
            lines(1, 21, digit2("Accept"), [3, 4, 6, 7, 8, 9, 11, 14, 15, 16, 18, 20], [
                new(2, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
                new(19, emote(Emotes.Happy, Original)),
            ]),
            state(StickEvent, NodeStates.InProgress),
            done(),
        ], condition: () => NodeYet(Start),
            priority: 10,
            onConversationFinish: OnEndTalking),

        new(StickInProgress, [
            lines(1, 4, digit2, [2, 4]),
        ], condition: () => NodeIP(StickEvent) && Items.Num(Items.Stick) < 4),

        new(StickComplete, [
            lines(1, 8, digit2, [2, 3, 5, 6], [
                new(4, command(() => {
                    if(BeachstickGameStartPoint.HoldsStick && Items.Num(Items.Stick) == 3){
                        var heldItem = Context.player.heldItem;
                        Context.player.DropItem(false);
                        Context.globalData.gameData.AddCollected(heldItem.associatedItem, 1, equipAction: true);
                        GameObject.Destroy(heldItem.gameObject);
                    }
                })),
                new(4, item(Items.Stick, -4)),
                new(4, command(OnStickEventDone)),
                new(4, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Normal, Original)),
            ]),
            @if(() => _HM, lines(9, 12, digit2("HM", ""), [9, 10, 12]), lines(9, 12, digit2("L", ""), [9, 10, 12])),
            item(Items.Coin, 20),
            lines(13, 18, digit2, [13, 14, 17, 18]),
            state(StickEvent, NodeStates.Done),
            cont(-5),
        ], condition: () => NodeIP(StickEvent) && (
            Items.Num(Items.Stick) >= 4 || (BeachstickGameStartPoint.HoldsStick && Items.Num(Items.Stick) >= 3)
        ), priority: 5),

        new(HighAfterCompleted, [
            lines(1, 3, digit2, [3]),
            @if(() => NodeYet(HighAfterCompleted), lines(1, 4, digit2("First"), [1, 3, 4])),
            lines(4, 5, digit2, []),
            done(),
        ], condition: () => NodeDone(StickEvent) && _H),

        new(MidLowAfterCompleted, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "accept"),
            lines(1, 6, digit2("O2"), [3, 5]),
            end(),
            anchor("accept"),
            lines(3, 6, digit2, []),
            lines(7, 8, digit2, "Julie"),
            lines(9, 32, digit2, [14, 17, 18, 22, 27, 28]),
            @if(() => Items.Has(Items.Stick) || BeachstickGameStartPoint.HoldsStick, "start"),
            lines(1, 3, digit2("NotHasStick"), [1], [new(3, item(Items.Stick, 1))]),
            anchor("start"),
            line(33, Player),
            command(BeachstickGameStartPoint.OnGameReady),
            done(),
        ], condition: () => NodeDone(StickEvent) && NodeYet(MidLowAfterCompleted) && _ML),

        new(WaitGameStart, [
            @if(() => BeachstickGameStartPoint.HoldsStick,
                lines(1, 1, digit2, []),
                lines(1, 1, digit2("NotHoldsStick"), [])
            ),
        ], condition: () => NodeDone(MidLowAfterCompleted) && !GetBool(Const.STags.HasPlayedBeachstickball)),

        new(AfterGame, [
            line(1, Original, replacer: s => s.Replace("{{BallHitsBest}}", $"{BeachstickGameEnd.Highscore}")),
            @if(() => BeachstickGameEnd.Highscore < 50,
                lines(2, 3, digit2("lt50"), [2], [new(3, emote(Emotes.Happy, Original))]),
                lines(2, 3, digit2("ge50"), [2], [new(3, emote(Emotes.Happy, Original))])
            ),
        ], condition: () => NodeDone(MidLowAfterCompleted) && GetBool(Const.STags.HasPlayedBeachstickball)),
    ];
    internal override void OnGameStarted()
    {
        new GameObject("Sidequel_BeachstickKid_AnimatorWatcher").AddComponent<AnimatorWatcher>();
        GameObject.Find("/LevelObjects/VolleyballMinigame").transform.Find("Stick").gameObject.SetActive(false);
        Vector3[] positions = [
            new(37.8949f, 8.3911f, 852.9454f),
        ];
        foreach (var child in GameObject.Find("/LevelObjects/Tools").transform.GetChildren())
        {
            if (!child.name.StartsWith("Stick")) continue;
            if (positions.Any(p => (child.transform.position - p).sqrMagnitude < 10f)) child.gameObject.SetActive(false);
        }
        ModdingAPI.Character.OnSetupDone(() =>
        {
            Ch(Characters.BeachstickballKid).transform.Find("Character/Armature/root/Base/Chest/Head/BaseballCap").gameObject.SetActive(false);
        });
    }
    private static void OnStartFirstTalk()
    {
        isTalking = true;
        AnimatorWatcher.instance.OnStartFirstTalk();
    }
    private static void OnStickEventDone()
    {
        AnimatorWatcher.instance.OnStickEventDone();
    }
    private static void OnEndTalking()
    {
        isTalking = false;
    }
    private class AnimatorWatcher : MonoBehaviour
    {
        private const string StretchParam = "StretchDance";
        private const string SitParam = "Sit";
        private const string LookAroundParam = "LookAround";
        private Animator animator = null!;
        private List<Transform> sticks = null!;
        private bool eventDone;
        private bool eventStarted;
        internal static AnimatorWatcher instance = null!;
        private void Awake()
        {
            instance = this;
            var obj = GameObject.Find("VolleyballOpponent");
            Assert(obj != null, "obj is null!");
            if (obj == null) return;
            animator = obj.GetComponentInChildren<Animator>();
            sticks = [
                obj.transform.Find("Character/Cube"),
                obj.transform.Find("Character/Armature/root/Base/Chest/collar_r/arm_r/hand_r/HandToolAnchor/Stick"),
            ];
            eventDone = NodeDone(StickEvent);
            eventStarted = NodeIP(StickEvent);
            if (!eventDone)
            {
                foreach (var stick in sticks) stick.gameObject.SetActive(false);
            }
        }
        private void Update()
        {
            if (isTalking) return;
            if (!eventDone)
            {
                animator.SetBool(StretchParam, false);
            }
            if (eventStarted && !eventDone)
            {
                animator.SetBool(LookAroundParam, true);
            }
            if (!eventStarted && !eventDone)
            {
                animator.SetBool(SitParam, true);
            }
        }
        internal void OnStickEventDone()
        {
            eventDone = true;
            foreach (var stick in sticks) stick.gameObject.SetActive(true);
            animator.SetBool(SitParam, false);
            animator.SetBool(StretchParam, true);
            animator.SetBool(LookAroundParam, false);
        }
        internal void OnStartFirstTalk()
        {
            eventStarted = true;
            animator.SetBool(SitParam, false);
            animator.SetBool(LookAroundParam, true);
            var originalSpeed = animator.speed;
            animator.speed = 3f;
            Timer.Register(0.1f, () => animator.speed = originalSpeed);
        }
    }
}

internal class BeachstickGameStartPoint : StartNodeEntry
{
    private static VolleyballGameController controller = null!;
    protected override string StartNode => "VolleyballGameStart";

    internal const string StartGame = "BeachstickGame.StartGame";
    private const string BeachstickGameReady = "BeachstickGameReady";
    internal const string NotHoldsStick = "BeachstickGame.NotHoldsStick";
    internal static bool HoldsStick => Context.player.heldItem?.associatedItem?.name == "Stick";
    protected override Node[] Nodes => [
        new(StartGame, [
            @if(() => NodeYet(StartGame), line(1, Original), line(UnityEngine.Random.Range(1, 6), Original)),
            done(),
        ],
            condition: () => HoldsStick,
            onConversationFinish: StartBSB
        ),
        new(NotHoldsStick, [
            lines(1, 1, digit2, []),
        ], condition: () => !HoldsStick),
    ];
    private static void StartBSB()
    {
        STags.SetBool(Const.STags.HasPlayedBeachstickball, true);
        typeof(VolleyballGameController).GetMethod("StartGame", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller, []);
    }
    internal static void OnGameReady()
    {
        controller.gameObject.SetActive(true);
        STags.SetBool(BeachstickGameReady, true);
    }
    internal override void OnGameStarted()
    {
        var obj = GameObject.Find("/LevelObjects").transform.Find("VolleyballMinigame/VolleyballMinigameController");
        Assert(obj != null, "BSB controller is null!!");
        if (obj == null) return;
        controller = obj.GetComponent<VolleyballGameController>();
        controller.gameObject.SetActive(GetBool(BeachstickGameReady));
    }
}

internal class BeachstickGameEnd : StartNodeEntry
{
    internal static int Highscore { get; private set; }
    internal static int CurrentGameScore { get; set; }
    protected override string StartNode => "VolleyballGameEnd";
    internal const string End1 = "BeachstickGame.End1";
    internal const string End2 = "BeachstickGame.End2";
    internal const string End3 = "BeachstickGame.End3";
    internal const string End4 = "BeachstickGame.End4";
    internal const string BeachstickKid = "BeachstickballKid";
    internal const string Julie = "Julie";
    private const int TargetScore = 50;
    private static Func<int, string> GetSpeaker(HashSet<int> indexes) => i => indexes.Contains(i) ? Player : BeachstickKid;
    protected override Node[] Nodes => [
        new(End1, [
            end(() => CurrentGameScore == 0),
            wait(0.3f),
            command(Face),
            wait(0.7f),
            line(1, Julie, replacer: s => s.Replace("{{BallHits}}", $"{CurrentGameScore}")),
            lines(2, 8, digit2, GetSpeaker([2, 3, 6, 7, 8])),
            command(UpdateHighscore),
            command(Unface),
            done(),
        ], condition: () => NodeYet(End1) && CurrentGameScore < TargetScore),

        new(End2, [
            wait(0.3f),
            command(Face),
            wait(0.7f),
            line(1, Julie, replacer: s => s.Replace("{{BallHits}}", $"{CurrentGameScore}")),
            lines(2, 9, digit2, GetSpeaker([2, 4, 6, 7, 8]), [
                new(2, emote(Emotes.Happy, Player)),
                new(3, emote(Emotes.Surprise, BeachstickKid)),
                new(5, emote(Emotes.Happy, BeachstickKid)),
            ]),
            command(UpdateHighscore),
            command(Unface),
            done(),
            cont(-10),
        ], condition: () => NodeYet(End2) && CurrentGameScore >= TargetScore),

        new(End3, [
            end(() => CurrentGameScore == 0),
            wait(0.3f),
            command(Face),
            wait(0.7f),
            line(1, Julie, replacer: s => s.Replace("{{BallHits}}", $"{CurrentGameScore}")),
            lines(2, 12, digit2, GetSpeaker([2, 3, 4, 6, 7, 10, 12]), [
                new(8, emote(Emotes.Happy, BeachstickKid))
            ]),
            command(UpdateHighscore),
            command(Unface),
            done(),
            cont(-10),
        ], condition: () => NodeDone(End1) && NodeYet(End3) && CurrentGameScore < TargetScore),

        new(End4, [
            end(() => CurrentGameScore == 0),
            wait(0.3f),
            command(Face),
            wait(0.7f),
            line(1, Julie, replacer: s => s.Replace("{{BallHits}}", $"{CurrentGameScore}")),
            @if(() => CurrentGameScore > Highscore, "updated", "notUpdated"),
            lines(2, 5, digit2("updated"), GetSpeaker([4]), anchor: "updated"),
            command(UpdateHighscore),
            end(),
            anchor("notUpdated"),
            @if(() => CurrentGameScore >= 10, emote(Emotes.Happy, BeachstickKid)),
            @switch(() => CurrentGameScore switch{
                < 10 => "lt10",
                < 20 => "lt20",
                < 30 => "lt30",
                < 50 => "lt50",
                _ => "ge50"
            }),
            lines(2, 2, digit2("notUpdated.lt10"), BeachstickKid, anchor: "lt10"), end(),
            lines(2, 2, digit2("notUpdated.lt20"), BeachstickKid, anchor: "lt20"), end(),
            lines(2, 2, digit2("notUpdated.lt30"), BeachstickKid, anchor: "lt30"), end(),
            lines(2, 2, digit2("notUpdated.lt50"), BeachstickKid, anchor: "lt50"), end(),
            lines(2, 2, digit2("notUpdated.ge50"), BeachstickKid, anchor: "ge50"), end(),
        ], condition: () => NodeDone(End2) || NodeDone(End3), onConversationFinish: Unface),
    ];
    private static void Face()
    {
        var player = Context.player;
        var kid = Ch(Characters.BeachstickballKid).gameObject;
        var ptok = (kid.transform.position - player.transform.position).normalized;
        Context.player.WalkTo(player.transform.position + ptok * 3, null, 0.5f);
        player.GetComponentInChildren<PlayerIKAnimator>().lookAt = kid.transform;
        OpponentFacer.isActive = true;
    }
    private static void Unface()
    {
        Context.player.GetComponentInChildren<PlayerIKAnimator>().lookAt = null;
        OpponentFacer.isActive = false;
    }
    private static void UpdateHighscore()
    {
        if (CurrentGameScore > Highscore)
        {
            Highscore = CurrentGameScore;
            STags.SetInt(Const.STags.BeachstickballHighscore, CurrentGameScore);
        }
    }
    internal override void OnGameStarted()
    {
        Highscore = STags.GetInt(Const.STags.BeachstickballHighscore);
    }
}

internal class BeachstickGamePoppedBallEnd : StartNodeEntry
{
    protected override string StartNode => "PoppedBallEnd";
    internal const string End1 = "BeachstickGame.PoppedBallEnd1";
    internal const string End2 = "BeachstickGame.PoppedBallEnd2";
    private const string BeachstickKid = BeachstickGameEnd.BeachstickKid;
    private const string Julie = BeachstickGameEnd.Julie;
    protected override Node[] Nodes => [
        new(End1, [
            lines(1, 16, digit2, i => i switch {
                1 or 5 or 7 or 8 or 13 or 15 => Player,
                3 or 9 => Julie,
                _ => BeachstickKid,
            }, [
                new(2, emote(Emotes.Surprise, BeachstickKid)),
                new(3, emote(Emotes.Happy, Julie)),
                new(4, emote(Emotes.Normal, BeachstickKid)),
                new(15, emote(Emotes.Happy, Player)),
                new(16, emote(Emotes.Normal, Player)),
            ]),
            done(),
        ], condition: () => NodeYet(End1)),

        new(End2, [
            lines(1, 8, digit2, i => i switch {
                3 or 4 or 8 => Player,
                5 => Julie,
                _ => BeachstickKid
            }, [
                new(4, emote(Emotes.Happy, Player)),
                new(5, emote(Emotes.Happy, Julie)),
                new(6, emote(Emotes.Normal, Player)),
                new(6, emote(Emotes.Normal, Julie)),
            ]),
        ], condition: () => NodeDone(End1)),
    ];
}

[HarmonyPatch(typeof(AchievementManager))]
internal class AchievementManagerPatch
{
    private const string Name = "Beachstickball";
    [HarmonyPrefix()]
    [HarmonyPatch("SetLeaderboard")]
    internal static bool SetLeaderboard(string boardname, int value)
    {
        if (!State.IsActive || boardname != Name) return true;
        BeachstickGameEnd.CurrentGameScore = value;
        return false;
    }
}

[HarmonyPatch(typeof(VolleyballOpponent))]
internal class OpponentFacer
{
    internal static bool isActive = false;
    [HarmonyPrefix()]
    [HarmonyPatch("GetDesiredRotateDirection")]
    internal static bool Direction(VolleyballOpponent __instance, ref Vector3 __result)
    {
        if (!isActive) return true;
        __result = NPCMovement.TowardPlayerRotation(__instance.transform);
        return false;
    }
}

