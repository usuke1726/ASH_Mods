
using System.Collections;
using UnityEngine;

namespace Sidequel.Dialogue.Actions;

internal class LookAtAction : BaseAction, IInvokableInAction
{
    private readonly string character;
    private readonly bool isStringTarget;
    private readonly string? target;
    private readonly Func<Transform?> getTarget = null!;
    public LookAtAction(string character, string? target, string? anchor = null) : base(ActionType.LookAt, anchor)
    {
        this.character = character;
        this.target = target;
        isStringTarget = true;
    }
    public LookAtAction(string character, Func<Transform?> getTarget, string? anchor = null) : base(ActionType.LookAt, anchor)
    {
        this.character = character;
        this.getTarget = getTarget;
        isStringTarget = false;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (isStringTarget) LookAt(conversation, character, target);
        else LookAtTr(conversation, character, getTarget);
        yield break;
    }
    internal static void LookAt(IConversation conversation, string character, string? target)
    {
        if (!Character.TryGetCharacter(conversation, character, out var ch)) return;
        ch.lookAt = (target != null && Character.TryGetCharacter(conversation, target, out var t)) ? t.transform : null;
        lookedCharacters.Add(ch);
    }
    internal static void LookAtTr(IConversation conversation, string character, Func<Transform?> getTarget)
    {
        if (!Character.TryGetCharacter(conversation, character, out var ch)) return;
        ch.lookAt = getTarget();
        lookedCharacters.Add(ch);
    }
    internal static void LookAt(string character, string target) => LookAt(null!, character, target);
    internal static void LookAtTr(string character, Func<Transform?> getTarget) => LookAtTr(null!, character, getTarget);

    private static readonly HashSet<ModdingAPI.Character> lookedCharacters = [];
    internal static void CleanUp()
    {
        foreach (var ch in lookedCharacters) ch.lookAt = null;
        lookedCharacters.Clear();
    }
}

