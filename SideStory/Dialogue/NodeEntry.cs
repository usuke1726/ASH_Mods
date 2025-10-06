
using System.Collections;
using ModdingAPI;
using SideStory.Dialogue.Actions;

namespace SideStory;

internal abstract class NodeEntry
{
    protected abstract Dialogue.Node[] Nodes { get; }
    protected virtual Characters? Character => null;
    protected virtual bool IsGlobal => false;

    protected static string Player => Dialogue.Character.Player;
    protected static string Original => Dialogue.Character.Original;
    protected static int LastSelected => Dialogue.DialogueController.instance.LastSelected;

#pragma warning disable IDE1006
    protected static CommandAction command(Action action, bool hideBox = true, string? anchor = null) => new(action, hideBox, anchor);
    protected static CommandAction command(Func<IEnumerator> coroutine, bool hideBox = true, string? anchor = null) => new(coroutine, hideBox, anchor);
    protected static GetItemAction item(string itemId, string? anchor = null) => new(itemId, anchor);
    protected static AddItemAction item(string itemId, int amount, string? anchor = null) => new(itemId, amount, anchor);
    protected static EmoteAction emote(Emotes emotion, string speaker, string? anchor = null) => new(emotion, speaker, anchor);
    protected static IfAction @if(Func<bool> condition, string? trueAnchor, string? falseAnchor, string? anchor = null) => new(condition, trueAnchor, falseAnchor, anchor);
    protected static LineAction line(string line, string speaker, string? anchor = null) => new(line, speaker, anchor);
    protected static OptionAction option(string[] options, string? anchor = null) => new(options, anchor);
    protected static Anchor anchor(string anchor) => new(anchor);
    protected static NodeCompleteAction end(string? anchor = null) => new(anchor);
    protected static NodeCompleteAction complete(string? anchor = null) => new(anchor);
    protected static TagAction tag(string id, object value, string? anchor = null) => new(id, value, anchor);
    protected static WaitAction wait(float time, bool hideBox = true, string? anchor = null) => new(time, hideBox, anchor);
#pragma warning restore IDE1006

    private bool setupDone = false;
    internal void Setup()
    {
        if (setupDone) return;
        setupDone = true;
        var nodes = Nodes;
        if (IsGlobal)
        {
            foreach (var node in nodes)
            {
                Dialogue.NodeSelector.RegisterNode(node);
            }
        }
        else
        {
            var character = Character;
            foreach (var node in nodes)
            {
                Dialogue.NodeSelector.RegisterNode(character, node);
            }
        }
    }
}

