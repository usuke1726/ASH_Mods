
using System.Collections;

namespace SideStory.Dialogue.Actions;

internal class LineAction : BaseAction
{
    internal readonly string line;
    internal readonly string speaker;
    public LineAction(string line, string speaker, string? anchor = null) : base(ActionType.Line, anchor)
    {
        this.line = line;
        this.speaker = speaker;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
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

