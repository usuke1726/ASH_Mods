
namespace SideStory.Dialogue.Actions;

internal class SwitchAction : FlowBase
{
    private readonly Func<int> getIndex;
    private readonly List<string?> anchors;
    private readonly int count;
    public SwitchAction(Func<int> getIndex, IEnumerable<string?> anchors, string? anchor = null) : base(ActionType.Switch, anchor)
    {
        this.getIndex = getIndex;
        this.anchors = [.. anchors];
        count = this.anchors.Count;
    }
    internal override string? GetAnchor() => count > 0 ? anchors[Math.Clamp(getIndex(), 0, count - 1)] : null;
}

