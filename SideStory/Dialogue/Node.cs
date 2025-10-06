
using SideStory.Dialogue.Actions;

namespace SideStory.Dialogue;

internal class Node
{
    protected static int LastSelected { get => DialogueController.instance.LastSelected; }
    private readonly List<BaseAction> actions;
    private int index = -1;
    internal readonly int priority;
    internal readonly Action? onConversationFinish;
    internal readonly Func<bool> condition;
    private readonly Dictionary<string, int> anchors = [];
    public Node(List<BaseAction> actions, Func<bool>? condition = null, int priority = 0, Action? onConversationFinish = null)
    {
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
                if (!anchors.TryGetValue(anchor, out var nextIdx)) return new NodeCompleteAction();
                index = nextIdx - 1;
                continue;
            }
            else return action;
        }
    }
}

