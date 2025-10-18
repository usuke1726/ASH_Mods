
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class LineAction : BaseAction, IInvokableInAction
{
    internal readonly string line;
    internal readonly string speaker;
    private readonly Func<bool>? condition;
    public LineAction(string line, string speaker, Func<bool>? condition = null, string? anchor = null) : base(ActionType.Line, anchor)
    {
        this.line = line;
        this.speaker = speaker;
        this.condition = condition;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (condition != null && !condition()) yield break;
        var text = TextReplacer.ReplaceVariables(I18n(line));
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

