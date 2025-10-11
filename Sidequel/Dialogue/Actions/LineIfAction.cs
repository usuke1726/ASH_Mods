
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class LineIfAction : BaseAction, IInvokableInAction
{
    private readonly Func<bool> condition;
    private readonly string trueLine;
    private readonly string falseLine;
    private readonly string speaker;
    public LineIfAction(Func<bool> condition, string trueLine, string falseLine, string speaker, string? anchor = null) : base(ActionType.Line, anchor)
    {
        this.condition = condition;
        this.trueLine = trueLine;
        this.falseLine = falseLine;
        this.speaker = speaker;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        var line = condition() ? trueLine : falseLine;
        var text = TextReplacer.ReplaceVariables(I18n_.Localize(line));
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

