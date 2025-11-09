
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class IfAction : FlowBase
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
    internal override string? GetAnchor() => condition() ? trueAnchor : falseAnchor;
}
internal class IfSingleAction : BaseAction, IInvokableInAction
{
    private readonly Func<bool> condition;
    private readonly IInvokableInAction? trueAction;
    private readonly IInvokableInAction? falseAction;
    public IfSingleAction(Func<bool> condition, IInvokableInAction? trueAction, IInvokableInAction? falseAction, string? anchor = null) : base(ActionType.If, anchor)
    {
        this.condition = condition;
        this.trueAction = trueAction;
        this.falseAction = falseAction;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        var self = this;
        while (true)
        {
            var action = self.condition() ? self.trueAction : self.falseAction;
            if (action is IfSingleAction nextIf)
            {
                self = nextIf;
                continue;
            }
            yield return action?.Invoke(conversation);
            break;
        }
    }
}

