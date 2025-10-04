
namespace SideStory.Dialogue.Actions;

internal class IfAction : BaseAction
{
    internal readonly Func<bool> condition;
    internal readonly string? trueAnchor;
    internal readonly string? falseAnchor;
    public IfAction(Func<bool> condition, string? trueAnchor, string? falseAnchor, string? anchor = null) : base(ActionType.If, anchor)
    {
        this.condition = condition;
        this.trueAnchor = trueAnchor;
        this.falseAnchor = falseAnchor;
    }
    internal string? GetAnchor() => condition() ? trueAnchor : falseAnchor;
}

