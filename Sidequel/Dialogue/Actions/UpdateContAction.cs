
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class UpdateContAction : BaseAction, IInvokableInAction
{
    private readonly int value;
    public UpdateContAction(int value, string? anchor = null) : base(ActionType.Cont, anchor)
    {
        this.value = value;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        Cont.Add(value);
        yield break;
    }
}

