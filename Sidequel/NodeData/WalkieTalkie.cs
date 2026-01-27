
using System.Collections;
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.Dialogue.Actions;
using UnityEngine;
namespace Sidequel.NodeData;

internal class WalkieTalkieEntry : StartNodeEntry
{
    internal static Transform Speaker { get; private set; } = null!;
    private static RaceCoordinator coordinator = null!;
    internal static RaceCoordinator Coordinator
    {
        get
        {
            if (coordinator != null && coordinator.gameObject != null) return coordinator;
            return coordinator = GameObject.Find("RaceCoordinator").GetComponent<RaceCoordinator>();
        }
    }
    private static PlayerReplay playerReplay = null!;
    internal static bool IsReplaying => playerReplay.isPlaying;
    private static Transform raceOpponent = null!;
    internal static Transform RaceOpponent => raceOpponent != null ? raceOpponent : (raceOpponent = GameObject.Find("RaceOpponent").transform);
    private static Renderer raceOpponentRenderer = null!;
    internal static bool OpponentVisible => raceOpponentRenderer.isVisible;
    internal static bool IsNearby => OpponentVisible && (Context.player.transform.position - raceOpponent.position).sqrMagnitude < 1000f;
    private const float MaxDistance = 100f;
    private static readonly Vector3 lightHousePos = new(574.3444f, 96.7446f, 339.8489f);
    private static readonly Vector3 royalRidgePos = new(52.1914f, 57.5393f, 338.3041f);
    internal static bool IsAtLightHouse => (RaceOpponent.position - lightHousePos).sqrMagnitude < MaxDistance;
    internal static bool IsAtStartPosition => (RaceOpponent.position - royalRidgePos).sqrMagnitude < MaxDistance;
    internal static bool IsAtValidPosition => IsAtLightHouse || IsAtStartPosition;
    private static void SetupSpeaker()
    {
        if (Speaker == null || Speaker.gameObject == null)
        {
            //Debug($"AVERY WALKIE TALKIE SETUP");
            var profile = TextBoxStyleProfile.Load("AveryRadioTextBoxProfile");
            Assert(profile != null, "profile is null!");
            var textBoxspeaker = new GameObject("ProfileHolder").AddComponent<TextBoxSpeaker>();
            textBoxspeaker.textBoxStyle = profile;
            Speaker = textBoxspeaker.transform;
        }
        if (raceOpponentRenderer == null || raceOpponentRenderer.gameObject == null)
        {
            raceOpponentRenderer = RaceOpponent.Find("Character/Body").GetComponent<SkinnedMeshRenderer>();
            playerReplay = RaceOpponent.GetComponent<PlayerReplay>();
        }
    }
    protected override string StartNode => "WalkieTalkieStart";
    internal const string Entry = "WalkieTalkie.Entry";
    protected override Node[] Nodes => [
        new(Entry, [
            command(SetupSpeaker),
            next(WalkieTalkie.GetNext, null),
        ]),
    ];
}

internal class WalkieTalkie : NodeEntry
{
    protected override Characters? Character => null;
    private class SpeakerAction(bool isPlayer) : BaseAction(ActionType.Command, null)
    {
        private readonly bool isPlayer = isPlayer;
        public override IEnumerator Invoke(IConversation conversation)
        {
            if (isPlayer) conversation.currentSpeaker = ModdingAPI.Character.Get(Characters.Claire).transform;
            else
            {
                var speaker = WalkieTalkieEntry.Speaker;
                speaker.position = Context.player.transform.position;
                conversation.currentSpeaker = speaker;
            }
            yield break;
        }
    }
    private static SpeakerAction sp(bool isPlayer = false) => new(isPlayer);
    private static LineAction line(string line) => new(line, "");
    private static LineAction line(int index) => new($"{index:00}", "");

    internal const string Nearby = "WalkieTalkie.Nearby";
    internal const string WithinRace = "WalkieTalkie.WithinRace";
    internal const string RaceWin = "WalkieTalkie.RaceWin";
    internal const string RaceWinWithinRace = "WalkieTalkie.RaceWinWithinRace";
    internal const string RaceWinWithinRaceNearby = "WalkieTalkie.RaceWinWithinRaceNearby";
    internal const string RaceLose = "WalkieTalkie.RaceLose";
    internal const string Normal = "WalkieTalkie.Normal";
    internal const string Place_LightHouse = "LightHouse";
    internal const string Place_RoyalRidge = "RoyalRidge";
    internal const string Place_Elsewhere = "Elsewhere";
    protected override Node[] Nodes => [
        new(Nearby, [
            line(1, "Avery"),
        ], condition: () => false),

        new(Normal, [
            sp(true),
            line(1),
            sp(),
            @switch(GetPlace),
            anchor(Place_LightHouse),
            line(Place_LightHouse),
            end(),
            anchor(Place_RoyalRidge),
            line(Place_RoyalRidge),
            end(),
            anchor(Place_Elsewhere),
            line($"{Place_Elsewhere}.01"),
            line($"{Place_Elsewhere}.02"),
            sp(true),
            line($"{Place_Elsewhere}.03"),
            sp(),
            line($"{Place_Elsewhere}.04"),
        ], condition: () => false),

        new(WithinRace, [
            sp(),
            line(1),
            sp(true),
            option(["O1", "O2", "O3"]),
            @switch(() => LastSelected switch {
                0 => "restart",
                1 => "abandon",
                _ => "nothing"
            }),
            anchor("restart"),
            sp(),
            line("O1.01"),
            command(Restart),
            end(),
            anchor("abandon"),
            sp(),
            line("O2.01"),
            line("O2.02"),
            command(Abandon),
            end(),
            anchor("nothing"),
            sp(),
            line("O3.01"),
            line("O3.02"),
        ], condition: () => false),

        new(RaceLose, [
            sp(),
            line(1),
            line(2),
            sp(true),
            option(["O1", "O2"]),
            sp(),
            @if(() => LastSelected == 0, "start"),
            line("O2.01"),
            end(),
            anchor("start"),
            line("O1.01"),
            command(() => isRaceStarting = true),
            transition(() => {
                WalkieTalkieEntry.Coordinator.PlaceRacer(null);
                Avery.PlaceRacers();
            }),
            wait(0.5f),
        ], condition: () => false, onConversationFinish: () => {
            if(isRaceStarting){
                isRaceStarting = false;
                requireStartingRace?.Invoke();
            }
        }),

        new(RaceWin, [
            sp(),
            line(1),
            line(2),
            line(3),
            sp(true),
            line(4),
        ], condition: () => false),

        new(RaceWinWithinRace, [
            sp(),
            line(1),
        ], condition: () => false),

        new(RaceWinWithinRaceNearby, [
            line(1, "Avery"),
        ], condition: () => false),
    ];

    private static void Abandon()
    {
        Avery.AveryPositionFixerAfterRace.isAbandoned = true;
        WalkieTalkieEntry.Coordinator.CallAbandonRace();
        CleanUp();
    }
    private static void Restart()
    {
        Avery.AveryPositionFixerAfterRace.isAbandoned = true;
        WalkieTalkieEntry.Coordinator.CallRestartRace();
    }
    private static void CleanUp()
    {
        Avery.DestroyRaceController();
    }
    internal static string GetNext()
    {
        if (Avery.RaceActive)
        {
            if (!Avery.RaceController!.recorder.recording)
            {
                if (WalkieTalkieEntry.IsNearby) return RaceWinWithinRaceNearby;
                return RaceWinWithinRace;
            }
            return WithinRace;
        }
        if (WalkieTalkieEntry.IsNearby) return Nearby;
        if (Avery.RaceLosed)
        {
            CleanUp();
            return RaceLose;
        }
        if (Avery.RaceWon) return RaceWin;
        return Normal;
    }
    private static string GetPlace()
    {
        if (WalkieTalkieEntry.IsAtLightHouse) return Place_LightHouse;
        if (WalkieTalkieEntry.IsAtStartPosition) return Place_RoyalRidge;
        return Place_Elsewhere;
    }
    private static Action? requireStartingRace = null;
    private static bool isRaceStarting = false;
    internal static void OnRequiredStartingRaceFromCall(Action action) => requireStartingRace = action;
}

