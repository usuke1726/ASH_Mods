
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class IndexedLinesAction : RangedLinesAction
{
    public IndexedLinesAction(Func<int, string> getI18nKey, string speaker, string? anchor = null) : base(1, 1000, getI18nKey, speaker, anchor) { }
    public IndexedLinesAction(Func<int, string> getI18nKey, Func<int, string> getSpeaker, string? anchor = null) : base(1, 1000, getI18nKey, getSpeaker, anchor) { }
}

internal class RangedLinesAction : BaseAction, IInvokableInAction
{
    private readonly int minInclusive;
    private readonly int maxInclusive;
    private readonly Func<int, string> getI18nKey;
    private readonly Func<int, string>? getSpeaker = null;
    private readonly string? speaker = null;
    public RangedLinesAction(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, string speaker, string? anchor = null) : base(ActionType.IndexesLines, anchor)
    {
        this.minInclusive = minInclusive;
        this.maxInclusive = Math.Max(minInclusive, maxInclusive);
        this.getI18nKey = getI18nKey;
        this.speaker = speaker;
    }
    public RangedLinesAction(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, Func<int, string> getSpeaker, string? anchor = null) : base(ActionType.IndexesLines, anchor)
    {
        this.minInclusive = minInclusive;
        this.maxInclusive = Math.Max(minInclusive, maxInclusive);
        this.getI18nKey = getI18nKey;
        this.getSpeaker = getSpeaker;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (speaker != null && Character.TryGetCharacter(conversation, speaker, out var character))
        {
            conversation.currentSpeaker = character.gameObject.transform;
        }
        for (int index = minInclusive; index <= maxInclusive; index++)
        {
            if (getSpeaker != null)
            {
                var sp = getSpeaker(index);
                if (sp != null && Character.TryGetCharacter(conversation, sp, out var ch))
                {
                    conversation.currentSpeaker = ch.gameObject.transform;
                }
            }
            var s = I18n(getI18nKey(index));
            if (string.IsNullOrEmpty(s)) break;
            var text = TextReplacer.ReplaceVariables(s);
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return conversation.ShowLine(text);
            }
        }
    }
}

