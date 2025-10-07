
namespace SideStory.Dialogue.Actions;

internal class Anchor(string anchor) : BaseAction(ActionType.Anchor, anchor) { }
internal class NodeCompleteAction(Func<bool>? condition = null, string? anchor = null) : BaseAction(ActionType.NodeComplete, anchor)
{
    private readonly Func<bool>? condition = condition;
    public bool End() => condition?.Invoke() ?? true;
}

