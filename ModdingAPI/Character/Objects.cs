
using ModdingAPI.Events;
using UnityEngine;

namespace ModdingAPI;

public class Character
{
    public Characters character { get; private set; }
    public GameObject gameObject { get; private set; }
    private static readonly Dictionary<Transform, Character> transforms = [];
    public Character(Characters ch, GameObject obj)
    {
        character = ch;
        gameObject = obj;
        transforms[gameObject.transform] = this;
        SetupAnimator();
    }
    public Character(Character info) : this(info.character, info.gameObject) { }
    private IEmotionAnimator? emotionAnimator = null;
    private IPoseAnimator? poseAnimator = null;
    private void SetupAnimator()
    {
        var animator1 = gameObject.GetComponentInChildren<NPCIKAnimator>();
        if (animator1 != null)
        {
            emotionAnimator = animator1;
            poseAnimator = animator1;
            return;
        }
        var animator2 = gameObject.GetComponentInChildren<PlayerIKAnimator>();
        if (animator2 != null)
        {
            emotionAnimator = animator2;
            poseAnimator = animator2;
            return;
        }
        Monitor.SLog($"== failed to get animator of character {character}!!", LogLevel.Error);
    }
    private StackResourceSortingKey? emotionReleaser = null;
    private Action? poseReleaseer = null;
    public void ShowEmotion(Emotion emotion, int priority = 0)
    {
        ClearEmotion();
        emotionReleaser = emotionAnimator?.ShowEmotion(emotion, priority);
    }
    public void ClearEmotion()
    {
        emotionReleaser?.ReleaseResource();
        emotionReleaser = null;
    }
    public void Pose(Pose pose)
    {
        ClearPose();
        poseReleaseer = poseAnimator?.Pose(pose);
    }
    public void ClearPose()
    {
        poseReleaseer?.Invoke();
        poseAnimator = null;
    }


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
            transforms.Clear();
            setupDone = false;
            onSetupDone = null!;
            helper.Events.Gameloop.PlayerUpdated += SetupCharacters;
        };
    }
    public static bool TryGet(string name, out Character character)
    {
        if (CharacterUtil.TryParse(name, out Characters ch1))
        {
            character = characters[ch1];
            return true;
        }
        else if (CharacterUtil.TryParse(name, out UniqueCharacters ch2))
        {
            character = characters[CharacterUtil.ToCharacter(ch2)];
            return true;
        }
        character = null!;
        return false;
    }
    public static bool TryGet(Transform transform, out Character character)
    {
        character = null!;
        if (transform == null) return false;
        var tr = transform;
        while (true)
        {
            if (transforms.TryGetValue(tr, out character)) return true;
            if (tr.root == tr) break;
            tr = tr.parent;
        }
        return false;
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
        SetupCharacter_normal(Characters.DadBoatDeer1, "DadDeer", "NPCs");
        SetupCharacter_normal(Characters.DadBoatDeer2, "DadDeerDock", "NPCs");
        SetupCharacter_normal(Characters.KidBoatDeer1, "StandingNPC (1)", "NPCs");
        SetupCharacter_normal(Characters.KidBoatDeer2, "DeerKidBoat", null);
        SetupCharacter_normal(Characters.KidBoatDeer3, "StandingNPC (2)", "NPCs");
        SetupCharacter_normal(Characters.ShipWorker1, "FishBuyer", "World/Structures/Boat");
        SetupCharacter_normal(Characters.ShipWorker2, "FishBuyer", "NPCs");
        SetupCharacter_normal(Characters.RumorGuy, "Croc_WalkingNPC", "NPCs");
        SetupCharacter_normal(Characters.Charlie1, "PolarBearNPC", "NPCs");
        SetupCharacter_normal(Characters.Charlie2, "CampfireFriends/SitBear", "NPCs");
        SetupCharacter_normal(Characters.Tim1, "RockClimberHangout/ClimbSquirrel", "Cutscenes");
        SetupCharacter_normal(Characters.ClimbingRhino1, "RockClimberHangout/GroundRhinoNPC", "Cutscenes");
        SetupCharacter_normal(Characters.Tim2, "RockClimbersAbove/TimScared", "Cutscenes");
        SetupCharacter_normal(Characters.ClimbingRhino2, "RockClimbersAbove/RhinoClimb", "Cutscenes");
        SetupCharacter_normal(Characters.Tim3, "CampfireFriends/SitSquirrel", "NPCs");
        SetupCharacter_normal(Characters.ClimbingRhino3, "CampfireFriends/SitRhino", "NPCs");
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
        SetupCharacter_normal(Characters.PictureFox1, "StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.PictureFox2, "FoxClimberNPC", "NPCs");
        SetupCharacter_normal(Characters.Taylor, "Turtle_WalkingNPC", "NPCs");
        SetupCharacter_normal(Characters.Sue, "Bunny_WalkingNPC (1)", "NPCs");
        SetupCharacter_normal(Characters.WatchGoat, "Goat_StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Julie, "RefereeKid", null);
        SetupCharacter_normal(Characters.BeachstickballKid, "VolleyballOpponent", null);
        SetupCharacter_normal(Characters.Avery, "RaceOpponent", null);
        SetupCharacter_normal(Characters.Artist1, "ArtistQuest/Artist1/StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Artist2, "ArtistQuest/Artist2/StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Artist3, "ArtistQuest/Artist3/StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Artist4, "ArtistQuest/Artist4/StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Artist5, "ArtistQuest/Artist5/StandingNPC", "NPCs");
        SetupCharacter_normal(Characters.Artist6, "ArtistQuest/Artist6/StandingNPC", "NPCs");
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

