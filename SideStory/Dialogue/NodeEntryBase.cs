
using System.Collections;
using SideStory.Dialogue.Actions;

namespace SideStory.Dialogue;

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
    protected static GetItemAction item(string itemId, string? anchor = null) => new(itemId, anchor);
    protected static AddItemAction item(string itemId, int amount, string? anchor = null) => new(itemId, amount, anchor);
    protected static EmoteAction emote(Emotes emotion, string speaker, string? anchor = null) => new(emotion, speaker, anchor);
    protected static IfAction @if(Func<bool> condition, string? trueAnchor, string? falseAnchor, string? anchor = null) => new(condition, trueAnchor, falseAnchor, anchor);
    protected static SwitchAction @switch(Func<int> getIndex, IEnumerable<string?> anchors, string? anchor = null) => new(getIndex, anchors, anchor);
    protected static LineAction line(string line, string speaker, string? anchor = null) => new(line, speaker, anchor);
    protected static OptionAction option(string[] options, string? anchor = null) => new(options, anchor);
    protected static Anchor anchor(string anchor) => new(anchor);
    protected static NodeCompleteAction end(string? anchor = null) => new(anchor);
    protected static NodeCompleteAction complete(string? anchor = null) => new(anchor);
    protected static TagAction tag(string id, object value, string? anchor = null) => new(id, value, anchor);
    protected static TransitionAction transition(Action action, string? anchor = null) => new(action, anchor);
    protected static WaitAction wait(float time, bool hideBox = true, string? anchor = null) => new(time, hideBox, anchor);
#pragma warning restore IDE1006

    sealed public override string ToString() => base.ToString();
    sealed public override bool Equals(object obj) => base.Equals(obj);
    sealed public override int GetHashCode() => base.GetHashCode();
}

