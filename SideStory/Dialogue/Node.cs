
using SideStory.Dialogue.Actions;

namespace SideStory.Dialogue;

internal class Node
{
    protected static int LastSelected { get => DialogueController.instance.LastSelected; }
    private readonly List<BaseAction> actions;
    private int index = -1;
    internal readonly int priority;
    internal readonly Func<bool> condition;
    private readonly Dictionary<string, int> anchors = [];
    public Node(List<BaseAction> actions, Func<bool>? condition = null, int priority = 0)
    {
        this.actions = actions;
        this.condition = condition ?? (() => true);
        this.priority = priority;
        for (int i = 0; i < actions.Count; i++)
        {
            var anchor = actions[i].anchor;
            if (anchor != null) anchors[anchor] = i;
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
            if (action is IfAction ifAction)
            {
                var anchor = ifAction.GetAnchor();
                if (anchor == null) continue;
                if (!anchors.TryGetValue(anchor, out var nextIdx)) return new NodeCompleteAction();
                index = nextIdx - 1;
                continue;
            }
            else return action;
        }
    }
}

