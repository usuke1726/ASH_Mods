
namespace Sidequel.Dialogue.Actions;
internal class GotoAction(string target, string? anchor = null) : FlowBase(ActionType.Goto, anchor)
{
    private readonly string target = target;
    internal override string? GetAnchor() => target;
}

