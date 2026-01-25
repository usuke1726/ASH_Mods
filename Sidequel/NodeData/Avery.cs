
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Avery : NodeEntry
{
    protected override Characters? Character => Characters.Avery;
    internal const string BeforeJA1 = "Avery.BeforeJA1";
    internal const string BeforeJA2 = "Avery.BeforeJA2";
    internal const string BeforeJA3 = "Avery.BeforeJA3";
    internal const string AfterJA1 = "Avery.AfterJA1";
    internal const string AfterJA2 = "Avery.AfterJA2";
    internal const string AfterJABody = "Avery.AfterJABody";
    internal const string FirstRaceWin = "Avery.FirstRaceWin";
    internal const string FirstRaceLose = "Avery.FirstRaceLose";
    internal const string RaceStart = "Avery.RaceStart";
    internal const string RaceStartBody = "Avery.RaceStartBody";
    internal const string RaceWin = "Avery.RaceWin";
    internal const string RaceLose = "Avery.RaceLose";
    internal const string GoldMedal1 = "Avery.GoldMedal1";
    internal const string GoldMedal2 = "Avery.GoldMedal2";
    internal const string GoldMedal3 = "Avery.GoldMedal3";
    internal const string RacedOnce = "RacedWithAveryOnce";
    internal const string ShoesEquipped = "ShoesEquipped";
    private bool raceReady = false;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 12, digit2, [1, 4, 5, 6, 9, 12], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(10, emote(Emotes.Happy, Original)),
                new(11, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1) && NodeYet(RunningGoat.BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 15, digit2, [2, 5, 6,11, 15], [
                new(8, emote(Emotes.Happy, Original)),
                new(12, emote(Emotes.Normal, Original)),
                new(14, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 3, digit2, [2]),
        ], condition: () => _bJA && NodeDone(BeforeJA2)),

        new(AfterJA1, [
            done(),
            lines(1, 14, digit2, [2, 4, 7, 8, 13], [
                new(6, emote(Emotes.Happy, Original)),
                new(9, emote(Emotes.Normal, Original)),
                new(12, emote(Emotes.Happy, Original)),
                new(14, emote(Emotes.Normal, Original)),
            ], replacer: Const.formatJATrigger),
            @if(() => _ML, "ML"),
            lines(15, 21, digit2("H", ""), [15, 16, 17, 20], [new(21, emote(Emotes.Happy, Original))]),
            end(),
            anchor("ML"),
            next(() => AfterJABody),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA2, [
            line("01", Original),
            @if(() => _ML, "ML"),
            lines(2, 4, digit2("H", ""), [2]),
            end(),
            anchor("ML"),
            next(() => AfterJABody),
        ], condition: () => _aJA && NodeDone(AfterJA1) && !GetBool(RacedOnce) && !NodeIP(Const.Events.GoldMedal)),

        new(AfterJABody, [
            option(["O1", "O2"]),
            @if(() => LastSelected == 1, "accept"),
            lines(1, 2, digit2("O1"), []),
            end(),
            anchor("accept"),
            emote(Emotes.Happy, Original),
            lines(1, 6, digit2, [5], [new(2, emote(Emotes.Normal, Original))]),
            @if(() => GetBool(ShoesEquipped), "raceReady"),
            lines(1, 2, digit2("NotEquippedYet"), []),
            @if(() => Items.Has(Items.RunningShoes), "HasShoes"),
            lines(1, 3, digit2("NotHasShoes"), [1, 3], [new(2, emote(Emotes.Happy, Original))]),
            end(),
            anchor("HasShoes"),
            lines(1, 6, digit2("HasShoes"), [1, 2, 3, 5, 6], [
                new(4, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
            ]),
            tag(ShoesEquipped, true),
            command(EquipShoes),
            wait(1f),
            lines(7, 8, digit2("HasShoes"), [], [
                new(7, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
            ]),
            anchor("raceReady"),
            command(() => raceReady = true),
        ], condition: () => false, onConversationFinish: CheckForRaceReady),

        new(FirstRaceWin, [
            command(AveryPositionFixerAfterRace.Dispose),
            command(() => isFirstRace = false),
            command(() => RaceController = null),
            lines(1, 9, digit2, [5, 6, 7, 8], [
                new(8, emote(Emotes.Happy, Player)),
                new(9, emote(Emotes.Happy, Original)),
            ]),
            emote(Emotes.Normal, Player),
            @if(() => !GetBool(Const.STags.HasWonAveryOnce),
                lines(1, 6, digit2("Avery.RaceWin.Reward"), [2, 6], [
                    new(1, cont(-10)),
                    new(2, item(Items.Coin, 60)),
                    new(3, emote(Emotes.Normal, Original)),
                ], useId: false)
            ),
            tag(Const.STags.HasWonAveryOnce, true),
            lines(10, 16, digit2, [16], [
                new(12, emote(Emotes.Normal, Original)),
                new(15, item(Items.WalkieTalkie)),
                new(15, emote(Emotes.Happy, Original)),
            ]),
            cont(-10),
        ], condition: () => RaceWon && isFirstRace, priority: 10),

        new(FirstRaceLose, [
            command(AveryPositionFixerAfterRace.Dispose),
            command(() => isFirstRace = false),
            command(() => RaceController = null),
            lines(1, 16, digit2, [2, 3, 6, 8, 12, 17], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.EyesClosed, Player)),
                new(5, emote(Emotes.Normal, Original)),
                new(6, emote(Emotes.Normal, Player)),
                new(9, emote(Emotes.Happy, Original)),
                new(13, emote(Emotes.Normal, Original)),
                new(16, item(Items.WalkieTalkie)),
                new(16, emote(Emotes.Happy, Original)),
            ]),
            cont(-10),
        ], condition: () => RaceLosed && isFirstRace, priority: 10),

        new(RaceStart, [
            lines(1, 2, digit2, []),
            next(() => RaceStartBody),
        ], condition: () => GetBool(RacedOnce) && !NodeIP(Const.Events.GoldMedal)),

        new(RaceStartBody, [
            option(["O1", "O2"]),
            @if(() => LastSelected == 1, "accept"),
            lines(1, 2, digit2("O1"), []),
            end(),
            anchor("accept"),
            command(() => raceReady = true),
            emote(Emotes.Happy, Original),
            line("O2.01", Original),
            emote(Emotes.Normal, Original),
            @if(AveryAtStartPoint, "startImm"),
            line("O2.NotAtStartPoint", Original),
            transition(PlaceRacers),
            end(),
            anchor("startImm"),
            line("O2.02", Original),
        ], condition: () => false, onConversationFinish: CheckForRaceReady),

        new(RaceWin, [
            command(AveryPositionFixerAfterRace.Dispose),
            command(() => RaceController = null),
            lines(1, 5, digit2, [3, 7], [
                new(4, emote(Emotes.Happy, Original)),
            ]),
            @if(() => !GetBool(Const.STags.HasWonAveryOnce),
                lines(1, 6, digit2("Avery.RaceWin.Reward"), [2, 6], [
                    new(1, cont(-10)),
                    new(2, item(Items.Coin, 60)),
                    new(3, emote(Emotes.Normal, Original)),
                ], useId: false)
            ),
            tag(Const.STags.HasWonAveryOnce, true),
            lines(6, 8, digit2, [7], [
                new(6, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => RaceWon && !isFirstRace, priority: 10),

        new(RaceLose, [
            command(AveryPositionFixerAfterRace.Dispose),
            command(() => RaceController = null),
            lines(1, 8, digit2, [2, 4, 7], [
                new(1, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => RaceLosed && !isFirstRace, priority: 10),

        new(GoldMedal1, [
            lines(1, 35, digit2, [1, 3, 5, 9, 10, 14, 16, 17, 19, 20, 21, 25, 28, 29, 32, 33, 35], [
                new(6, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Happy, Original)),
                new(34, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeYet(GoldMedal1)),

        new(GoldMedal2, [
            lines(1, 5, digit2, [3]),
            next(() => RaceStartBody),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeDone(GoldMedal1), priority: -1),

        new(GoldMedal3, [
            lines(1, 14, digit2, [3, 5, 6, 8,  11, 14], [
                new(7, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
                new(12, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeS1(Const.Events.GoldMedal) && NodeDone(GoldMedal1) && NodeYet(GoldMedal3)),
    ];
    internal static RaceController? RaceController { get; private set; } = null;
    internal static void DestroyRaceController() => RaceController = null;
    private static Transform raceOpponent = null!;
    private bool isFirstRace = false;
    private void CheckForRaceReady()
    {
        if (!raceReady) return;
        raceReady = false;
        isFirstRace = !GetBool(RacedOnce);
        System.STags.SetBool(RacedOnce, true);
        RaceController = raceOpponent!.GetComponent<RaceController>();
        var raceData = RaceController.currentRaceData;
        PositionWatcher.Dispose();
        Assert(raceData != null, "raceData is null!!");
        RaceController.SetupRaceData(raceData);
        var begin = typeof(RaceController).GetMethod("BeginRace", BindingFlags.NonPublic | BindingFlags.Instance);
        begin.Invoke(RaceController, []);
    }

    internal static bool RaceActive => RaceController != null && _tags.GetBool(RaceController.startedRaceTag) && !_tags.GetBool(RaceController.finishedRaceTag);
    internal static bool RaceFinished => RaceController != null && _tags.GetBool(RaceController.finishedRaceTag);
    internal static bool RaceWon => RaceFinished && _tags.GetBool(RaceController!.raceWonTag);
    internal static bool RaceLosed => RaceFinished && !_tags.GetBool(RaceController!.raceWonTag);
    internal static bool RaceAbandoned => RaceFinished && _tags.GetBool(RaceController!.abandonRaceTag);
    private static Vector3 startPosition = new(51.9104f, 57.5393f, 337.0358f);
    private bool AveryAtStartPoint()
    {
        var distance = (raceOpponent.transform.position.SetY(0) - startPosition.SetY(0)).sqrMagnitude;
        return distance < 100;
    }
    internal static void PlaceRacers()
    {
        AveryPositionFixerAfterRace.Dispose();
        PositionWatcher.Dispose();
        Context.player.transform.position = new(46.2891f, 57.5667f, 342.6601f);
        Context.player.transform.localRotation = Quaternion.Euler(0, 177.2422f, 0);
        raceOpponent.position = new(46.0936f, 57.5393f, 337.4354f);
        raceOpponent.localRotation = Quaternion.Euler(0, 94.4436f, 0);
    }
    private void EquipShoes()
    {
        Traverse.Create(Context.player).Field("hasRunningShoes").SetValue(true);
    }
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            raceOpponent = Ch(Characters.Avery).transform;
            Assert(raceOpponent != null, "raceOpponent is null");
            var controller = raceOpponent!.GetComponent<RaceController>();
            _tags.WatchBool(controller.finishedRaceTag, f => { if (f) AveryPositionFixerAfterRace.Setup(); });
        });
        Traverse.Create(Context.player).Field("hasRunningShoes").SetValue(GetBool(ShoesEquipped));
        WalkieTalkie.OnRequiredStartingRaceFromCall(() =>
        {
            raceReady = true;
            CheckForRaceReady();
        });
    }
    internal class AveryPositionFixerAfterRace : MonoBehaviour
    {
        // エイブリーが遠くにいるときにゴールすると、謎のvelocityが発生し、エイブリーが崖から落ちてしまう。この不具合を解消するためにこのオブジェクトを用意する。
        private Rigidbody body = null!;
        private Vector3 position = Vector3.zero;
        private static AveryPositionFixerAfterRace? instance = null;
        internal static bool isAbandoned = false;
        internal static void Setup()
        {
            instance ??= new GameObject("Sidequel_AveryPositionFixer").AddComponent<AveryPositionFixerAfterRace>();
        }
        internal static void Dispose()
        {
            if (instance == null) return;
            GameObject.Destroy(instance.gameObject);
            instance = null;
            PositionWatcher.Setup();
            Debug($"disposed averyPositionFixer");
        }
        private void Awake()
        {
            body = Ch(Characters.Avery).transform.GetComponent<Rigidbody>();
            position = body.position;
        }
        private void FixedUpdate()
        {
            if (isAbandoned)
            {
                Dispose();
                isAbandoned = false;
            }
            body.velocity = body.velocity.SetX(0).SetZ(0);
            body.position = position.SetY(body.position.y);
        }
    }
    internal class PositionWatcher : MonoBehaviour
    {
        private float timer = 5f;
        private static PositionWatcher? instance = null;
        internal static void Setup()
        {
            instance = new GameObject("AveryPositionWatcher").AddComponent<PositionWatcher>();
        }
        internal static void Dispose()
        {
            if (instance == null) return;
            GameObject.Destroy(instance.gameObject);
            instance = null;
            Debug($"disposed averyPositionWatcher");
        }
        private void Update()
        {
            if (RaceActive) Dispose();
            timer -= Time.deltaTime;
            if (timer > 0) return;
            timer = 5f;
            var player = Context.player.transform;
            if (player.position.z < 1000 && player.position.y < 390) return;
            if (WalkieTalkieEntry.IsAtValidPosition) return;
            Debug($"reset avery position", LL.Warning);
            WalkieTalkieEntry.Coordinator.PlaceRacer(null!);
        }
    }
}

