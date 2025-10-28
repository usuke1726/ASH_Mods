
using Sidequel.Dialogue.Actions;

namespace Sidequel.Dialogue;

internal class Node
{
    protected static int LastSelected { get => DialogueController.instance.LastSelected; }
    private readonly List<BaseAction> actions;
    private int index = -1;
    internal readonly string? id;
    internal readonly int priority;
    internal readonly Action? onConversationFinish;
    internal readonly Func<bool> condition;
    private readonly Dictionary<string, int> anchors = [];
#if DEBUG
    private static readonly HashSet<string> ids = [];
#endif
    public Node(string? id, List<BaseAction> actions, Func<bool>? condition = null, int priority = 0, Action? onConversationFinish = null)
    {
        this.id = id;
        this.actions = actions;
        this.condition = condition ?? (() => true);
        this.priority = priority;
        this.onConversationFinish = onConversationFinish;
        for (int i = 0; i < actions.Count; i++)
        {
            var anchor = actions[i].anchor;
            if (anchor != null)
            {
                if (anchors.ContainsKey(anchor)) Monitor.Log($"anchor \"{anchor}\" already exists", LL.Error);
                anchors[anchor] = i;
            }
        }
#if DEBUG
        if (id != null)
        {
            if (ids.Contains(id))
            {
                Monitor.Log($"node id \"{id}\" has been already registered!", LL.Warning);
            }
            ids.Add(id);
        }
#endif
    }
    internal void Reset()
    {
        index = -1;
    }
    internal BaseAction NextAction()
    {
        while (true)
        {
            index++;
            if (index >= actions.Count) return new NodeCompleteAction();
            var action = actions[index];
            if (action is FlowBase flow)
            {
                var anchor = flow.GetAnchor();
                if (anchor == null) continue;
                if (!anchors.TryGetValue(anchor, out var nextIdx))
                {
                    Monitor.Log($"anchor \"{anchor}\" not found!!", LL.Warning);
                    return new NodeCompleteAction();
                }
                index = nextIdx - 1;
                continue;
            }
            else return action;
        }
    }
}

