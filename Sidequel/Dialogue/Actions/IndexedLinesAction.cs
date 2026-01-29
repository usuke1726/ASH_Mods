
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class IndexedLinesAction : RangedLinesAction
{
    public IndexedLinesAction(Func<int, string> getI18nKey, string speaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(1, 1000, getI18nKey, speaker, actions, replacer, useId, anchor) { }
    public IndexedLinesAction(Func<int, string> getI18nKey, Func<int, string> getSpeaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(1, 1000, getI18nKey, getSpeaker, actions, replacer, useId, anchor) { }
    public IndexedLinesAction(Func<int, string> getI18nKey, HashSet<int> playerIndexes, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(1, 1000, getI18nKey, playerIndexes, actions, replacer, useId, anchor) { }
}

internal class RangedLinesAction : BaseAction, IInvokableInAction
{
    private readonly int minInclusive;
    private readonly int maxInclusive;
    private readonly Func<int, string> getI18nKey;
    private readonly Func<int, string>? getSpeaker = null;
    private readonly string? speaker = null;
    private readonly HashSet<int>? playerIndexes = null;
    private readonly Func<string, string> replacer;
    private readonly Dictionary<int, List<IInvokableInAction>> actionsMap = [];
    private readonly bool useId;
    public RangedLinesAction(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, string speaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(ActionType.IndexesLines, anchor)
    {
        this.minInclusive = minInclusive;
        this.maxInclusive = Math.Max(minInclusive, maxInclusive);
        this.getI18nKey = getI18nKey;
        this.speaker = speaker;
        this.replacer = replacer ?? (s => s);
        this.useId = useId;
        SetupActions(actions);
    }
    public RangedLinesAction(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, Func<int, string> getSpeaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(ActionType.IndexesLines, anchor)
    {
        this.minInclusive = minInclusive;
        this.maxInclusive = Math.Max(minInclusive, maxInclusive);
        this.getI18nKey = getI18nKey;
        this.getSpeaker = getSpeaker;
        this.replacer = replacer ?? (s => s);
        this.useId = useId;
        SetupActions(actions);
    }
    public RangedLinesAction(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, HashSet<int> playerIndexes, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(ActionType.IndexesLines, anchor)
    {
        this.minInclusive = minInclusive;
        this.maxInclusive = Math.Max(minInclusive, maxInclusive);
        this.getI18nKey = getI18nKey;
        this.playerIndexes = playerIndexes;
        this.replacer = replacer ?? (s => s);
        this.useId = useId;
        SetupActions(actions);
    }
    private void SetupActions(List<Tuple<int, IInvokableInAction>>? actions)
    {
        if (actions == null) return;
        foreach (var tuple in actions)
        {
            actionsMap.TryAdd(tuple.Item1, []);
            actionsMap[tuple.Item1].Add(tuple.Item2);
        }
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (speaker != null && Character.TryGetCharacter(conversation, speaker, out var character))
        {
            conversation.currentSpeaker = character.gameObject.transform;
        }
        for (int index = minInclusive; index <= maxInclusive; index++)
        {
            if (actionsMap.TryGetValue(index, out var actions))
            {
                foreach (var action in actions) yield return action.Invoke(conversation);
            }
            if (getSpeaker != null)
            {
                var sp = getSpeaker(index);
                if (sp != null && Character.TryGetCharacter(conversation, sp, out var ch))
                {
                    conversation.currentSpeaker = ch.gameObject.transform;
                }
            }
            else if (playerIndexes != null)
            {
                var sp = playerIndexes.Contains(index) ? Character.Player : Character.Original;
                if (Character.TryGetCharacter(conversation, sp, out var ch))
                {
                    conversation.currentSpeaker = ch.gameObject.transform;
                }
            }
            var s = replacer(I18n(getI18nKey(index), useId));
            if (string.IsNullOrEmpty(s)) break;
            var text = TextReplacer.ReplaceVariables(s);
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return conversation.ShowLine(text);
            }
        }
        if (actionsMap.TryGetValue(maxInclusive + 1, out var actionsAfterLines))
        {
            foreach (var action in actionsAfterLines) yield return action.Invoke(conversation);
        }
    }
}

