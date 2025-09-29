
using IMoreExpressions;
using ModdingAPI;
using UnityEngine;

namespace CustomConversation.Actions;

public abstract class ActionCore(Characters character, float time) : IComparable<ActionCore>
{
    public float time = time;
    public Characters character = character;
    public virtual void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters) { }
    public int CompareTo(ActionCore other)
    {
        return (this is EndAction, other is EndAction) switch
        {
            (true, false) => 1,
            (false, true) => -1,
            _ => time.CompareTo(other.time)
        };
    }
}

public class SpeakAction : ActionCore
{
    private const float DefaultShowingTime = 1.0f;
    private const float DefaultXOffset = 0.35f;
    public readonly string text;
    public readonly float showingTime;
    public readonly float xOffset;
    public SpeakAction(Characters character, float time, string text, float? showingTime = null, float? xOffset = null) : base(character, time)
    {
        this.text = text;
        this.showingTime = showingTime != null ? (float)showingTime : DefaultShowingTime;
        this.xOffset = xOffset != null ? (float)xOffset : DefaultXOffset;
    }
    public override void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters)
    {
        TemporalBox.Add(obj.parent.transform, text, showingTime, xOffset);
    }
}

public class DrawAttentionAction(Characters target, float time) : ActionCore(target, time)
{
    public override void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters)
    {
        foreach (var ch in characters)
        {
            if (ch.character != character) ch.LookAt(obj);
        }
    }
}
public class AvoidAttentionAction(Characters target, float time) : ActionCore(target, time)
{
    public override void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters)
    {
        foreach (var ch in characters)
        {
            if (ch.IsLookingAt(obj)) ch.DisableLooking();
        }
    }
}
public class LookAtAction : ActionCore
{
    public readonly Characters? target = null;
    public readonly Transform? lookAt = null;
    public LookAtAction(Characters character, float time, Characters target) : base(character, time)
    {
        this.target = target;
    }
    public LookAtAction(Characters character, float time, Transform? lookAt) : base(character, time)
    {
        this.lookAt = lookAt;
    }
    public override void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters)
    {
        if (lookAt != null)
        {
            obj.LookAt(lookAt);
            return;
        }
        if (target == null)
        {
            obj.DisableLooking();
            return;
        }
        foreach (var ch in characters)
        {
            if (ch.character == target) obj.LookAt(ch);
        }
    }
}

public class EmoteAction(Characters character, float time, Expression? emotion) : ActionCore(character, time)
{
    public readonly Expression? emotion = emotion;
    public override void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters)
    {
        obj.ClearEmotion();
        if (emotion != null) obj.ShowEmotion((Expression)emotion);
    }
}
public class PoseAction(Characters character, float time, Pose? pose) : ActionCore(character, time)
{
    public readonly Pose? pose = pose;
    public override void Invoke(CharacterObject obj, IEnumerable<CharacterObject> characters)
    {
        obj.ClearPose();
        if (pose != null) obj.Pose((Pose)pose);
    }
}
public sealed class EndAction(Characters character, float time) : ActionCore(character, time) { }

