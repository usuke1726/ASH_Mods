
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class LineAction : BaseAction, IInvokableInAction
{
    internal readonly string line;
    internal readonly string speaker;
    private readonly Func<bool>? condition;
    private readonly Func<string, string> replacer;
    private readonly bool useId;
    public LineAction(string line, string speaker, Func<bool>? condition = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(ActionType.Line, anchor)
    {
        this.line = line;
        this.speaker = speaker;
        this.condition = condition;
        this.replacer = replacer ?? (s => s);
        this.useId = useId;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (condition != null && !condition()) yield break;
        var text = TextReplacer.ReplaceVariables(replacer(I18n(line, useId)));
        if (Character.TryGetCharacter(conversation, speaker, out var character))
        {
            conversation.currentSpeaker = character.gameObject.transform;
        }
        if (!string.IsNullOrWhiteSpace(text))
        {
            yield return conversation.ShowLine(text);
        }
    }
}

