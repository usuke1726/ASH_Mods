
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal abstract class BaseAction(ActionType type, string? anchor = null)
{
    internal readonly string? anchor = anchor;
    internal readonly ActionType type = type;
    public virtual IEnumerator Invoke(IConversation conversation) { yield break; }
}
internal abstract class FlowBase(ActionType type, string? anchor = null) : BaseAction(type, anchor)
{
    internal abstract string? GetAnchor();
    sealed public override IEnumerator Invoke(IConversation conversation) => base.Invoke(conversation);
}
internal interface IInvokableInAction
{
    IEnumerator Invoke(IConversation conversation);
}

