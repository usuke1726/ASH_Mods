
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;
using Shadow = UnityEngine.Rendering.ShadowCastingMode;

namespace Sidequel.NodeData;

internal class Wil : NodeEntry
{
    internal const string BeforeJA1 = "Wil.BeforeJA1";
    internal const string BeforeJA2 = "Wil.BeforeJA2";
    internal const string BeforeJA3 = "Wil.BeforeJA3";
    internal const string Nothing = "Wil.Nothing";
    internal const string TradingCard1 = "Wil.TradingCard1";
    internal const string TradingCardAccept = "Wil.TradingCard.accept";
    internal const string TradingCardRefuse = "Wil.TradingCard.refuse";
    internal const string TradingCard2 = "Wil.TradingCard2";
    internal const string TradingCard3 = "Wil.TradingCard3";
    internal const string AfterJA1 = "Wil.AfterJA1";
    internal const string BoatInspectionAccept = "Wil.BoatInspection.accept";
    internal const string BoatInspectionRefuse = "Wil.BoatInspection.refuse";
    internal const string AfterJA2 = "Wil.AfterJA2";
    internal const string AfterBoatInspection1 = "Wil.AfterBoatInspection1";
    internal const string AfterBoatInspection2 = "Wil.AfterBoatInspection2";
    internal const string GoldMedal1 = "Wil.GoldMedal1";
    internal const string GoldMedal2 = "Wil.GoldMedal2";
    internal const string AfterGoldMedal = "Wil.AfterGoldMedal";
    private bool soldCardInThisGame = false;
    protected override Characters? Character => Characters.DadBoatDeer1;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 11, digit2, [1, 5, 9, 10], [
                new(4, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
                new(11, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 10, digit2, [2, 3, 6, 9], [
                new(8, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 3, digit2, [3], [new(2, emote(Emotes.Happy, Original))]),
        ], condition: () => _bJA && NodeDone(BeforeJA2)),

        new(Nothing, [
            lines(1, 3, digit2, [3], [new(2, emote(Emotes.Happy, Original))]),
        ], condition: () => true, priority: int.MinValue),

        new(AfterJA1, [
            @if(() => NodeDone(BeforeJA1),
                lines(1, 2, digit2("MetFirst"), [], [new(1, emote(Emotes.Happy, Original))]),
                line(1, Original)
            ),
            emote(Emotes.Normal, Original),
            lines(2, 4, digit2, []),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? BoatInspectionAccept : BoatInspectionRefuse),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(BoatInspectionAccept, [
            done(AfterJA1),
            lines(1, 9, digit2, [9], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
                new(6, transition(MoveToInspectBoat)),
                new(6, wait(1.5f)),
                new(7, emote(Emotes.Happy, Original)),
                new(9, item(Items.Coin, 30)),
            ]),
            cont(-10),
        ], condition: () => false),

        new(BoatInspectionRefuse, [
            state(AfterJA1, NodeStates.Refused),
            lines(1, 4, digit2, [3], [new(4, emote(Emotes.Happy, Original))]),
        ], condition: () => false),

        new(AfterJA2, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? BoatInspectionAccept : BoatInspectionRefuse),
        ], condition: () => NodeRefused(AfterJA1)),

        new(AfterBoatInspection1, [
            lines(1, 7, digit2, [2, 3, 4], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(AfterJA1) && NodeYet(AfterBoatInspection1) && ResetPositionWatcher.IsActive, priority: int.MaxValue),

        new(AfterBoatInspection2, [
            lines(1, 2, digit2, []),
        ], condition: () => NodeDone(AfterBoatInspection1) && ResetPositionWatcher.IsActive, priority: int.MaxValue),

        new(TradingCard1, [
            lines(1, 7, digit2, [1, 2, 5]),
            @if(() => _HM,
                lines(1, 2, digit2("HM", ""), [1, 2]),
                lines(1, 3, digit2("L", ""), [1, 2], [
                    new(3, emote(Emotes.Happy, Original)),
                ])
            ),
            emote(Emotes.Happy, Original),
            lines(10, 11, digit2, []),
            emote(Emotes.Normal, Original),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? TradingCardAccept : TradingCardRefuse),
        ], condition: () => Items.Has(Items.TradingCard) && NodeYet(TradingCard1)),

        new(TradingCardAccept, [
            lines(1, 3, digit2, [3], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(2, item([Items.TradingCard, Items.Coin], [-1, 60])),
                new(2, emote(Emotes.Happy, Original)),
            ]),
            command(() => soldCardInThisGame = true),
            cont(-5),
            done(TradingCard1),
        ], condition: () => false),

        new(TradingCardRefuse, [
            lines(1, 2, digit2, [], [new(2, emote(Emotes.Happy, Original))]),
            state(TradingCard1, NodeStates.Refused),
        ], condition: () => false),

        new(TradingCard2, [
            lines(1, 2, digit2, []),
            option(["O1", "O2"]),
            next(() => LastSelected == 0 ? TradingCardAccept : TradingCardRefuse),
        ], condition: () => NodeRefused(TradingCard1) && !NodeActive(Const.Events.GoldMedal)),

        new(TradingCard3, [
            command(() => soldCardInThisGame = false),
            lines(1, 23, digit2, [1, 2, 9, 15, 16, 18, 20, 21], [
                new(4, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
                new(12, emote(Emotes.Happy, Original)),
                new(13, emote(Emotes.Normal, Original)),
                new(14, emote(Emotes.Happy, Original)),
                new(15, emote(Emotes.Surprise, Player)),
                new(16, emote(Emotes.Normal, Player)),
                new(17, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => soldCardInThisGame && NodeYet(TradingCard3) && !NodeActive(Const.Events.GoldMedal)),

        new(GoldMedal1, [
            lines(1, 13, digit2, [1, 2, 4, 5, 6, 12, 13]),
            done(),
        ], condition: () => NodeIP(Const.Events.GoldMedal) && NodeYet(GoldMedal1)),

        new(GoldMedal2, [
            command(() => soldCardInThisGame = false),
            lines(1, 6, digit2, [2, 3, 4, 6]),
            @if(() => NodeDone(GoldMedal1),
                lines(1, 2, digit2("GoldMedal1Done"), [1]),
                lines(1, 6, digit2("GoldMedal1Yet"), [1, 5, 6])
            ),
            lines(7, 51, digit2, [7, 8, 9, 10, 11, 13, 15, 18, 19, 22, 26, 27, 44, 51], [
                new(16, transition(ChangeBookColor)),
                new(16, wait(1.5f)),
                new(22, wait(1f)),
                new(24, wait(1.5f)),
                new(25, wait(1f)),
                new(26, emote(Emotes.Surprise, Player)),
                new(27, emote(Emotes.Normal, Player)),
                new(42, emote(Emotes.Surprise, Player)),
                new(43, emote(Emotes.Surprise, Original)),
                new(46, emote(Emotes.Normal, Original)),
                new(46, emote(Emotes.Normal, Player)),
            ]),
            transition(GoldMedalEnd.Prepare),
            next(() => GoldMedalEnd.Entry, ch: null),
        ], condition: () => NodeS1(Const.Events.GoldMedal), priority: 10),

        new(AfterGoldMedal, [
            lines(digit2, Original),
        ], condition: () => GoldMedalEnd.EventDoneInThisGame, priority: int.MaxValue),
    ];

    private static RuntimeAnimatorController defaultAnimatorController = null!;
    private Transform book = null!;
    private Transform deer = null!;
    private SkinnedMeshRenderer skinnedMeshRenderer = null!;
    private bool CastShadows { set => skinnedMeshRenderer.shadowCastingMode = value ? Shadow.On : Shadow.Off; }
    private static bool eventSet = false;
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            deer = Ch(Characters.DadBoatDeer1).transform;
            defaultAnimatorController = Sidequel.Character.Pose.GetController(Characters.DadBoatDeer1);
            book = deer.transform.Find("Fox/Armature/root/Base/Chest/collar_l/arm_l/hand_l/Cube");
            skinnedMeshRenderer = deer.Find("Fox/Body").GetComponent<SkinnedMeshRenderer>();
        });
        if (!eventSet)
        {
            eventSet = true;
            GoldMedalEnd.OnPreparing += () =>
            {
                deer.position = new(627.5654f, 130.601f, 413.8763f);
                CastShadows = true;
                Sidequel.Character.Pose.Set(deer, Poses.Standing);
                book.gameObject.SetActive(false);
            };
        }
    }
    private void MoveToInspectBoat()
    {
        var player = Context.player.transform;
        player.position = new(198.4326f, 12.2048f, 1270.392f);
        player.localRotation = Quaternion.Euler(0, 343.6974f, 0);
        Sidequel.Character.Pose.Set(deer, Poses.Standing);
        deer.position = new(192.2668f, 12.6957f, 1269.158f);
        deer.localRotation = Quaternion.Euler(0, 0, 0);
        CastShadows = true;
        var watcher = new GameObject("Sidequel_Wil_PositionWatcher").AddComponent<ResetPositionWatcher>();
        watcher.action = ResetDeerPosition;
        book.gameObject.SetActive(false);
    }
    private void ResetDeerPosition()
    {
        Sidequel.Character.Pose.Set(deer, defaultAnimatorController);
        deer.position = new(170.67f, 16.46f, 1261.083f);
        deer.localRotation = Quaternion.Euler(0, 321.4959f, 0);
        book.gameObject.SetActive(true);
        CastShadows = false;
    }
    private void ChangeBookColor()
    {
        book.GetComponent<MeshRenderer>().material.color = new(1, 0.839f, 0.646f, 1);
    }

    private class ResetPositionWatcher : MonoBehaviour
    {
        internal static bool IsActive { get; private set; } = false;
        private static readonly Vector3 basePosition = new(191.1197f, 0, 1269.398f);
        private const float Border = 66000f;
        internal Action action = null!;
        private void Awake() => IsActive = true;
        private void OnDestroy()
        {
            Debug($"Wil.ResetPositionWatcher deactivated", LL.Warning);
            IsActive = false;
        }
        private void Update()
        {
            var distance = (basePosition - Context.player.transform.position.SetY(0)).sqrMagnitude;
            if (distance >= Border)
            {
                action?.Invoke();
                GameObject.Destroy(gameObject);
            }
        }
    }
}

internal class GoldMedalEnd : NodeEntry
{
    internal const string Entry = "GoldMedal.End";
    internal const string Wil = "DadBoatDeer1";
    internal static bool EventDoneInThisGame { get; private set; } = false;
    internal static event Action OnPreparing = null!;
    protected override Characters? Character => null;
    protected override Node[] Nodes => [
        new(Entry, [
            command(() => {
                EventDoneInThisGame = true;
            }),
            done(Const.Events.GoldMedal),
            wait(1f),
            lines(digit2, Wil),
            item(Items.Coin, 150),
            cont(-20),
        ], condition: () => false),
    ];
    internal static void Prepare()
    {
        var player = Context.player;
        player.transform.position = new(624.4018f, 131.1851f, 419.817f);
        player.transform.localRotation = Quaternion.Euler(0, 224.7949f, 0);
        System.Music.FadeOutCurrentMusic(0.1f);
        OnPreparing?.Invoke();
    }
    internal override void OnGameStarted()
    {
        EventDoneInThisGame = false;
    }
}

