
using System.Collections;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Dialogue.Actions;

internal class TransitionAction : BaseAction, IInvokableInAction
{
    private readonly Action action;
    private readonly bool hideBox;
    public TransitionAction(Action action, bool hideBox = true, string? anchor = null) : base(ActionType.Transition, anchor)
    {
        this.action = action;
        this.hideBox = hideBox;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (hideBox) conversation.Hide();
        var transitionDone = false;
        Context.gameServiceLocator.transitionAnimation.Begin(action, () => transitionDone = true);
        yield return new WaitUntil(() => transitionDone);
    }
}

