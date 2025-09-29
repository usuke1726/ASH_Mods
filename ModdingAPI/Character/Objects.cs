
using ModdingAPI.Events;
using UnityEngine;

namespace ModdingAPI;

public class Character(Characters ch, GameObject obj)
{
    public Characters character { get; private set; } = ch;
    public GameObject gameObject { get; private set; } = obj;
    public Character(Character info) : this(info.character, info.gameObject) { }


    private static bool setupDone = false;
    private static IModHelper helper = null!;
    private static readonly Dictionary<Characters, Character> characters = [];
    internal static void Setup(IModHelper helper)
    {
        Character.helper = helper;
        helper.Events.Gameloop.PlayerUpdated += SetupCharacters;
        helper.Events.Gameloop.GameQuitting += (_, _) =>
        {
            characters.Clear();
            setupDone = false;
            helper.Events.Gameloop.PlayerUpdated += SetupCharacters;
        };
    }
    public static Character Get(Characters ch)
    {
        if (!Context.GameStarted) throw new Exception("Tried to get a character object before game started");
        return characters[ch];
    }
    public static IEnumerable<Character> GetCharacters() => characters.Values;
    public static void OnSetupDone(Action callback)
    {
        if (setupDone) callback.Invoke();
        else onSetupDone += callback;
    }
    public static void SetForEach(Action<Character> callback)
    {
        var action = () =>
        {
            foreach (var k in characters.Keys) callback(characters[k]);
        };
        if (setupDone) action.Invoke();
        else onSetupDone += action;
    }
    private static event Action onSetupDone = null!;
    private static void SetupCharacters(object? sender, PlayerUpdatedEventArgs e)
    {
        if (setupDone) return;
        helper.Events.Gameloop.PlayerUpdated -= SetupCharacters;
        SetupCharacter_normal(Characters.Claire, "Player", null);
        SetupCharacter_normal(Characters.AuntMay, "AuntMayNPC", "NPCs");
        SetupCharacter_normal(Characters.RangerJon, "CampRangerNPC", "NPCs");
        SetupCharacter_normal(Characters.DadBoatDeer, "DadDeerDock", "NPCs");
        SetupCharacter_normal(Characters.KidBoatDeer, "DeerKidBoat", null);
        SetupCharacter_normal(Characters.ShipWorker, "FishBuyer", "NPCs");
        SetupCharacter_normal(Characters.RumorGuy, "Croc_WalkingNPC", "NPCs");
        SetupCharacter_normal(Characters.Charlie, "PolarBearNPC", "NPCs");
        SetupCharacter_normal(Characters.Tim, "ClimbSquirrel", "RockClimberHangout");
        SetupCharacter_normal(Characters.ClimbingRhino, "GroundRhinoNPC", "RockClimberHangout");
        SetupCharacter_normal(Characters.GlideKid, "LittleKidNPCVariant", "NPCs");
        SetupCharacter_normal(Characters.Jen, "LittleKidNPCVariant (1)", "NPCs");
        SetupCharacter_normal(Characters.Bill, "SittingNPC (1)", "FishingTutorial");
        SetupCharacter_normal(Characters.OutlookPointGuy, "Dog_WalkingNPC_BlueEyed", "NPCs");
        SetupCharacter_normal(Characters.ToughBird, "ToughBirdNPC (1)", "NPCs");
        SetupCharacter_fromNode(Characters.SunhatDeer, "SittingNPC", "NPCs", "DeerStart");
        SetupCharacter_fromNode(Characters.DiveKid, "SittingNPC", "NPCs", "TucanStart");
        SetupCharacter_fromNode(Characters.RunningLizard, "RunningNPC", "NPCs", "Runner2Start");
        SetupCharacter_fromNode(Characters.RunningNephew, "RunningNPC", "NPCs", "Runner4Start");
        SetupCharacter_fromNode(Characters.RunningRabbit, "RunningNPC", "NPCs", "RunRabbitStart");
        SetupCharacter_fromNode(Characters.RunningGoat, "RunningNPC", "NPCs", "Runner3Start");
        SetupCharacter_normal(Characters.BreakfastKid, "Frog_StandingNPC (1)", "NPCs");
        SetupCharacter_normal(Characters.Camper, "CamperNPC", "NPCs");
        SetupCharacter_normal(Characters.ShovelKid, "Frog_StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.CompassFox, "Fox_WalkingNPC", "NPCs");
        SetupCharacter_normal(Characters.PictureFox, "StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Taylor, "Turtle_WalkingNPC", "NPCs");
        SetupCharacter_normal(Characters.Sue, "Bunny_WalkingNPC (1)", "NPCs");
        SetupCharacter_normal(Characters.WatchGoat, "Goat_StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Julie, "RefereeKid", null);
        SetupCharacter_normal(Characters.BeachstickballKid, "VolleyballOpponent", null);
        SetupCharacter_normal(Characters.Avery, "RaceOpponent", null);
        SetupCharacter_normal(Characters.Artist, "Artist1", "ArtistQuest");
        SetupCharacter_normal(Characters.HydrationDog, "Dog_WalkingNPC", "NPCs");
        objCacheFromStartNode.Clear();
        objectCaches.Clear();
        setupDone = true;
        onSetupDone?.Invoke();
    }
    private static void SetupCharacter_normal(Characters ch, string objName, string? rootObj)
    {
        TryAddCharacter(ch, GetChObject_normal(objName, rootObj));
    }
    private static void SetupCharacter_fromNode(Characters ch, string objName, string? rootObj, string startNode)
    {
        TryAddCharacter(ch, GetChObject_fromStartNode(objName, rootObj, startNode));
    }
    private static readonly Dictionary<string, GameObject> objCacheFromStartNode = [];
    private static readonly Dictionary<string, GameObject?> objectCaches = [];
    private static GameObject? GetChObject_normal(string objName, string? rootObj)
    {
        GameObject? obj;
        if (rootObj == null) obj = GameObject.Find(objName);
        else
        {
            objectCaches.TryAdd(rootObj, null);
            obj = (objectCaches[rootObj] ??= GameObject.Find(rootObj))?.transform?.Find(objName)?.gameObject;
        }
        return obj;
    }
    private static GameObject? GetChObject_fromStartNode(string objName, string? rootObj, string startNode)
    {
        if (objCacheFromStartNode.TryGetValue(startNode, out var obj)) return obj;
        if (rootObj == null) return null;
        objectCaches.TryAdd(rootObj, null);
        var root = (objectCaches[rootObj] ??= GameObject.Find(rootObj))?.transform;
        if (root == null) return null;
        foreach (var child in root.GetChildren())
        {
            var di = child.GetComponent<DialogueInteractable>();
            if (di == null) continue;
            if (child.name == objName && di.startNode == startNode) return child.gameObject;
            objCacheFromStartNode[di.startNode] = child.gameObject;
        }
        return null;
    }
    private static void TryAddCharacter(Characters ch, GameObject? obj)
    {
        if (obj == null)
        {
            Monitor.SLog($"NPC {ch} not found", LogLevel.Warning);
            return;
        }
        characters[ch] = new(ch, obj);
    }
}

