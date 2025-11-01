
namespace Sidequel.Dialogue.Actions;

internal class SwitchAction : FlowBase
{
    private readonly Func<int> getIndex;
    private readonly List<string?> anchors;
    private readonly Func<string>? getAnchor = null;
    private readonly int count;
    public SwitchAction(Func<int> getIndex, IEnumerable<string?> anchors, string? anchor = null) : base(ActionType.Switch, anchor)
    {
        this.getIndex = getIndex;
        this.anchors = [.. anchors];
        count = this.anchors.Count;
    }
    public SwitchAction(Func<string> getAnchor, string? anchor = null) : base(ActionType.Switch, anchor)
    {
        this.getAnchor = getAnchor;
        getIndex = null!;
        anchors = null!;
    }
    internal override string? GetAnchor()
    {
        if (getAnchor != null) return getAnchor();
        return count > 0 ? anchors[Math.Clamp(getIndex(), 0, count - 1)] : null;
    }
}

