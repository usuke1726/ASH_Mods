
using System.Collections;
using UnityEngine;

namespace Sidequel.Dialogue.Actions;

internal class WaitAction : BaseAction, IInvokableInAction
{
    internal readonly float time;
    internal readonly bool hideBox;
    public WaitAction(float time, bool hideBox = true, string? anchor = null) : base(ActionType.Wait, anchor)
    {
        this.time = time;
        this.hideBox = hideBox;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (hideBox) conversation.Hide();
        yield return new WaitForSeconds(time);
    }
}

