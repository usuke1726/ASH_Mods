
using System.Collections;
using ModdingAPI;
using UnityEngine;

namespace SideStory.Dialogue.Actions;

internal class TransitionAction : BaseAction
{
    private readonly Action action;
    public TransitionAction(Action action, string? anchor = null) : base(ActionType.Transition, anchor)
    {
        this.action = action;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
        var transitionDone = false;
        Context.gameServiceLocator.transitionAnimation.Begin(action, () => transitionDone = true);
        yield return new WaitUntil(() => transitionDone);
    }
}

