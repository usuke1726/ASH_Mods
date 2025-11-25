
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class LookAtAction : BaseAction, IInvokableInAction
{
    private readonly string character;
    private readonly string? target;
    public LookAtAction(string character, string? target, string? anchor = null) : base(ActionType.LookAt, anchor)
    {
        this.character = character;
        this.target = target;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        LookAt(conversation, character, target);
        yield break;
    }
    internal static void LookAt(IConversation conversation, string character, string? target)
    {
        if (!Character.TryGetCharacter(conversation, character, out var ch)) return;
        ch.lookAt = (target != null && Character.TryGetCharacter(conversation, target, out var t)) ? t.transform : null;
        lookedCharacters.Add(ch);
    }
    internal static void LookAt(string character, string target) => LookAt(null!, character, target);

    private static readonly HashSet<ModdingAPI.Character> lookedCharacters = [];
    internal static void CleanUp()
    {
        foreach (var ch in lookedCharacters) ch.lookAt = null;
        lookedCharacters.Clear();
    }
}

