
using System.Collections;
using Sidequel.Dialogue.Actions;

namespace Sidequel.Dialogue;

internal abstract class NodeEntryBase
{
    protected abstract Node[] Nodes { get; }
    internal abstract void Setup();

    protected static string Player => Character.Player;
    protected static string Original => Character.Original;
    protected static int LastSelected => DialogueController.instance.LastSelected;

#pragma warning disable IDE1006
    protected static IEnumerable<int> range(int minInclusive, int maxInclusive) => Enumerable.Range(minInclusive, Math.Max(maxInclusive + 1 - minInclusive, 1));
    protected static CommandAction command(Action action, bool hideBox = true, string? anchor = null) => new(action, hideBox, anchor);
    protected static CommandAction command(Func<IEnumerator> coroutine, bool hideBox = true, string? anchor = null) => new(coroutine, hideBox, anchor);
    protected static GetItemAction item(Func<string> getItemId, string? anchor = null) => new(getItemId, anchor);
    protected static AddItemAction item(Func<string> getItemId, Func<int> getAmount, string? anchor = null) => new(getItemId, getAmount, anchor);
    protected static AddMultipleItemsAction item(Func<string[]> getItemIds, Func<int[]> getAmounts, string? anchor = null) => new(getItemIds, getAmounts, anchor);
    protected static EmoteAction emote(Emotes emotion, string speaker, string? anchor = null) => new(emotion, speaker, anchor);
    protected static GotoAction @goto(string target, string? anchor = null) => new(target, anchor);
    protected static IfAction @if(Func<bool> condition, string? trueAnchor, string? falseAnchor, string? anchor = null) => new(condition, trueAnchor, falseAnchor, anchor);
    protected static IfSingleAction @if(Func<bool> condition, IInvokableInAction trueAction, IInvokableInAction falseAction, string? anchor = null) => new(condition, trueAction, falseAction, anchor);
    protected static SwitchAction @switch(Func<int> getIndex, IEnumerable<string?> anchors, string? anchor = null) => new(getIndex, anchors, anchor);
    protected static LineAction line(string line, string speaker, Func<bool>? condition = null, Func<string, string>? replacer = null, string? anchor = null) => new(line, speaker, condition, replacer, anchor);
    protected static LineIfAction lineif(Func<bool> condition, string trueLine, string falseLine, string speaker, Func<string, string>? replacer = null, string? anchor = null) => new(condition, trueLine, falseLine, speaker, replacer, anchor);
    protected static RangedLinesAction lines(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, string speaker, Func<string, string>? replacer = null, string? anchor = null) => new(minInclusive, maxInclusive, getI18nKey, speaker, replacer, anchor);
    protected static RangedLinesAction lines(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, Func<int, string> getSpeaker, Func<string, string>? replacer = null, string? anchor = null) => new(minInclusive, maxInclusive, getI18nKey, getSpeaker, replacer, anchor);
    protected static IndexedLinesAction lines(Func<int, string> getI18nKey, string speaker, Func<string, string>? replacer = null, string? anchor = null) => new(getI18nKey, speaker, replacer, anchor);
    protected static IndexedLinesAction lines(Func<int, string> getI18nKey, Func<int, string> getSpeaker, Func<string, string>? replacer = null, string? anchor = null) => new(getI18nKey, getSpeaker, replacer, anchor);
    protected static OptionAction option(string[] options, Func<string, string>? replacer = null, string? anchor = null) => new(options, replacer, anchor);
    protected static Anchor anchor(string anchor) => new(anchor);
    protected static NodeCompleteAction end(Func<bool>? condition = null, string? anchor = null) => new(condition, anchor);
    protected static TagAction tag(string id, object value, string? anchor = null) => new(id, value, anchor);
    protected static TransitionAction transition(Action action, string? anchor = null) => new(action, anchor);
    protected static WaitAction wait(float time, bool hideBox = true, string? anchor = null) => new(time, hideBox, anchor);
    protected static UpdateContAction cont(int value, string? anchor = null) => new(value, anchor);
    protected static UpdateNodeStateAction state(NodeStates state, string? anchor = null) => new(state, anchor);
    protected static UpdateNodeStateAction state(string nodeId, NodeStates state, string? anchor = null) => new(nodeId, state, anchor);
#pragma warning restore IDE1006

    sealed public override string ToString() => base.ToString();
    sealed public override bool Equals(object obj) => base.Equals(obj);
    sealed public override int GetHashCode() => base.GetHashCode();

    internal virtual void OnGameStarted() { }

    internal enum NodeStates
    {
        NotYet = 0,
        InProgress = 1,
        Refused = 2,
        Done = 3,
    }
}

