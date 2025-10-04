
using HarmonyLib;
using IMoreExpressions;
using ModdingAPI;
using UnityEngine;

namespace CustomConversation;

public abstract class CharacterObject
{
    internal static bool setupDone = false;
    private static readonly Dictionary<Characters, CharacterObject> characters = [];
    protected static RuntimeAnimatorController standingDummyAnimator = null!;
    protected static RuntimeAnimatorController raiseArmsDummyAnimator = null!;
    protected static RuntimeAnimatorController sittingDummyAnimator = null!;
    private RuntimeAnimatorController? defaultAnimator = null;
    private static IMoreExpressionsApi? emotionApi;
    public static void Setup(IModHelper helper)
    {
        Character.OnSetupDone(TryAddCharacter);
        helper.Events.Gameloop.PlayerUpdated += (s, e) =>
        {
            DebugUpdate();
        };
        helper.Events.Gameloop.GameQuitting += (s, e) =>
        {
            characters.Clear();
            setupDone = false;
        };
        helper.Events.Gameloop.GameLaunched += (_, _) =>
        {
            emotionApi = helper.ModRegistry.GetApi<IMoreExpressionsApi>("Quicker1726.MoreExpressions");
            if (emotionApi == null)
            {
                Monitor.Log($"emotionApi is null!", LL.Warning);
            }
        };
    }
    public static bool TryToGet(Characters id, out CharacterObject character) => characters.TryGetValue(id, out character);
    private static bool debugDone = false;
    private static void DebugUpdate()
    {
        return;
        if (debugDone || !setupDone) return;
        debugDone = true;
        foreach (var character in characters.Values)
        {
            var animator = character.gameObject.GetComponentInChildren<Animator>();
            if (animator == null) continue;
            animator.runtimeAnimatorController = standingDummyAnimator;
            // "NPCStand", "NPCWalk", "NPCTalking", "NPCStandWave"
            animator.Play("NPCStand", 0, 0f);
            //animator.Stop
            //animator.runtimeAnimatorController
        }
        return;
        foreach (var character in characters.Values)
        {
            var scale = character.gameObject.transform.localScale;
            scale = scale.SetY(1.0f + 0.5f * Mathf.Sin(2f * Time.time * Mathf.PI));
            character.gameObject.transform.localScale = scale;
        }
    }
    private static void TryAddCharacter()
    {
        foreach (var info in Character.GetCharacters())
        {
            var ch = info.character;
            var obj = info.gameObject;
            CharacterObject? newCharacter = null;
            var npc = obj.GetComponentInChildren<NPCIKAnimator>();
            if (npc != null)
            {
                newCharacter = new CharacterNPCObject(ch, obj, npc);
            }
            else
            {
                var player = obj.GetComponentInChildren<PlayerIKAnimator>();
                if (player != null) newCharacter = new CharacterPlayerObject(ch, obj, player);
            }
            if (newCharacter == null)
            {
                Monitor.Log($"IKAnimator of NPC {ch} not found", LL.Warning);
                return;
            }
            var animator = obj.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                newCharacter.defaultAnimator = animator.runtimeAnimatorController;
                if (ch == Characters.Camper) raiseArmsDummyAnimator = animator.runtimeAnimatorController;
                else if (ch == Characters.DadBoatDeer2) standingDummyAnimator = animator.runtimeAnimatorController;
                else if (ch == Characters.AuntMay) sittingDummyAnimator = animator.runtimeAnimatorController;
            }
            else
            {
                Monitor.Log($"Animator of NPC {ch} not found", LL.Warning);
            }
            characters[ch] = newCharacter;
        }
        setupDone = true;
    }

    public readonly GameObject gameObject;
    public readonly GameObject parent;
    protected readonly Animator animator;
    public readonly Characters character;
    private List<StackResourceSortingKey> emotions = [];
    private Transform defaultLookAt = null!;

    protected abstract ITalkingAnimator TAnimator { get; }
    protected abstract IEmotionAnimator EAnimator { get; }
    protected abstract ICanLook LAnimator { get; }
    protected abstract IPoseAnimator PAnimator { get; }
    protected CharacterObject(Characters ch, GameObject parent, GameObject obj)
    {
        this.parent = parent;
        gameObject = obj;
        character = ch;
        animator = parent.GetComponentInChildren<Animator>();
    }
    public override int GetHashCode() => (int)character;
    public override bool Equals(object obj) => (obj is CharacterObject c) && c.character == character;
    public virtual void Pose(Pose pose) => PAnimator.Pose(pose);
    protected abstract void EndPose(Pose pose);
    public void ClearPose()
    {
        foreach (Pose pose in Enum.GetValues(typeof(Pose))) EndPose(pose);
    }
    public void PoseStand()
    {
        if (character == Characters.AuntMay || character == Characters.DadBoatDeer2)
        {
            animator.runtimeAnimatorController = standingDummyAnimator;
            animator.Play("NPCStand");
            //animator.Play("BodyStanding");
        }
        //animator.Play("NPCTalking");
    }
    public virtual void OnPlaced()
    {
        if (!parent.activeSelf) parent.SetActive(true);
        ClearPose();
        PoseStand();
        switch (character)
        {
            // TODO: Add processes for other characters (some characters are implemented for making demo video)
            case Characters.Tim1:
                parent.GetComponent<ClimbingNPC>().enabled = false;
                break;
            case Characters.RumorGuy:
                parent.GetComponent<PathNPCMovement>().maxSpeed = 0;
                break;
        }
        defaultLookAt = LAnimator.lookAt;
        cleanUpDone = false;
    }
    private bool cleanUpDone = false;
    public virtual void CleanUp()
    {
        if (cleanUpDone) return;
        DisableLooking();
        if (character == Characters.Claire)
        {
            ClearPose();
            ClearEmotion();
        }
        cleanUpDone = true;
    }
    public void PoseSit()
    {
        animator.runtimeAnimatorController = sittingDummyAnimator;
    }
    public void PoseDefault()
    {
        animator.runtimeAnimatorController = defaultAnimator;
    }
    private static bool TryToEmotion(Expression exp, out Emotion emotion)
    {
        emotion = default;
        Emotion? e = exp switch
        {
            Expression.Happy => Emotion.Happy,
            Expression.Surprise => Emotion.Surprise,
            Expression.EyesClosed => Emotion.EyesClosed,
            _ => null
        };
        if (e != null) emotion = (Emotion)e;
        return e != null;
    }
    public void ShowEmotion(Expression emotion)
    {
        if (emotionApi == null)
        {
            if (TryToEmotion(emotion, out var e))
            {
                ShowVanillaEmotion(e);
            }
            return;
        }
        ClearEmotion();
        emotionApi.ShowEmotion(character, emotion);
    }
    public void ClearEmotion()
    {
        ClearVanillaEmotion();
        emotionApi?.ResetEmotion(character);
    }
    private void ShowVanillaEmotion(Emotion emotion, int priority = 0)
    {
        emotions.Add(EAnimator.ShowEmotion(emotion, priority));
    }
    private void ClearVanillaEmotion()
    {
        foreach (var emotion in emotions) emotion.ReleaseResource();
        emotions.Clear();
    }
    public bool IsLookingAt(CharacterObject character) => IsLookingAt(character.gameObject.transform);
    public bool IsLookingAt(Transform target) => LAnimator.lookAt == target;
    public void LookAt(CharacterObject character) => LookAt(character.gameObject.transform);
    public void LookAt(Transform transform) => LAnimator.lookAt = transform;
    public void DisableLooking() => LAnimator.lookAt = defaultLookAt;
    public bool IsTalking { get => TAnimator.isTalking; }
    public void SetTalking(bool isTalking) => TAnimator.SetTalking(isTalking);
}

internal class CharacterNPCObject : CharacterObject
{
    private readonly NPCIKAnimator ikAnimator;
    protected override ITalkingAnimator TAnimator => ikAnimator;
    protected override IEmotionAnimator EAnimator => ikAnimator;
    protected override IPoseAnimator PAnimator => ikAnimator;
    protected override ICanLook LAnimator => ikAnimator;
    internal CharacterNPCObject(Characters ch, GameObject obj, NPCIKAnimator animator) : base(ch, obj, animator.gameObject)
    {
        ikAnimator = animator;
    }
    public override void Pose(Pose pose)
    {
        if (pose == global::Pose.RaiseArms)
        {
            animator.runtimeAnimatorController = raiseArmsDummyAnimator;
        }
        base.Pose(pose);
    }
    protected override void EndPose(Pose pose)
    {
        ikAnimator.animator.SetBool(pose.ToString(), false);
        animator.runtimeAnimatorController = standingDummyAnimator;
    }
}
internal class CharacterPlayerObject : CharacterObject
{
    private readonly PlayerIKAnimator ikAnimator;
    protected override ITalkingAnimator TAnimator => ikAnimator;
    protected override IEmotionAnimator EAnimator => ikAnimator;
    protected override IPoseAnimator PAnimator => ikAnimator;
    protected override ICanLook LAnimator => ikAnimator;
    private readonly new Animator animator;
    private readonly Animator stretchAnimator;
    public readonly Animator headAnimator;
    private readonly PlayerIKAnimator.HandIK leftHand;
    private readonly PlayerIKAnimator.HandIK rightHand;
    private readonly PlayerIKAnimator.LimbIK leftFoot;
    private readonly PlayerIKAnimator.LimbIK rightFoot;
    private readonly Traverse isPosing;
    private readonly Traverse headIKEnabled;

    internal bool IsPosing
    {
        get => isPosing.GetValue<bool>();
        set => isPosing.SetValue(value);
    }
    internal bool HeadIKEnabled
    {
        get => headIKEnabled.GetValue<bool>();
        set => headIKEnabled.SetValue(value);
    }
    internal CharacterPlayerObject(Characters ch, GameObject obj, PlayerIKAnimator animator) : base(ch, obj, animator.gameObject)
    {
        ikAnimator = animator;
        stretchAnimator = animator.stretchAnimator;
        headAnimator = animator.headAnimator;
        var tr = Traverse.Create(animator);
        this.animator = tr.Field("animator").GetValue<Animator>();
        leftHand = tr.Field("leftHand").GetValue<PlayerIKAnimator.HandIK>();
        rightHand = tr.Field("rightHand").GetValue<PlayerIKAnimator.HandIK>();
        leftFoot = tr.Field("leftFoot").GetValue<PlayerIKAnimator.LimbIK>();
        rightFoot = tr.Field("rightFoot").GetValue<PlayerIKAnimator.LimbIK>();
        isPosing = tr.Field("isPosing");
        headIKEnabled = tr.Field("headIKEnabled");
    }
    protected override void EndPose(Pose pose)
    {
        stretchAnimator.SetBool(pose.ToString(), value: false);
        animator.SetBool(pose.ToString(), value: false);
        leftHand.SetEnabled(enabled: true);
        rightHand.SetEnabled(enabled: true);
        leftFoot.SetEnabled(enabled: true);
        rightFoot.SetEnabled(enabled: true);
        HeadIKEnabled = true;
        IsPosing = false;
    }
}

