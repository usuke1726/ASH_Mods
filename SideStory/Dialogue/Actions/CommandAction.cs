
using System.Collections;
using ModdingAPI;
using SideStory.Item;
using UnityEngine;

namespace SideStory.Dialogue.Actions;

internal class CommandAction : BaseAction
{
    internal readonly Action? action = null;
    internal readonly Func<IEnumerator>? coroutine = null;
    internal readonly bool hideBox;
    public CommandAction(Action action, bool hideBox = true, string? anchor = null) : base(ActionType.Command, anchor)
    {
        this.action = action;
        this.hideBox = hideBox;
    }
    public CommandAction(Func<IEnumerator> coroutine, bool hideBox = true, string? anchor = null) : base(ActionType.Command, anchor)
    {
        this.coroutine = coroutine;
        this.hideBox = hideBox;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (hideBox) conversation.Hide();
        if (coroutine != null) yield return coroutine();
        else action?.Invoke();
    }
}

internal class GetItemAction : BaseAction, IInvokableInAction
{
    internal readonly Func<string> getItemId;
    public GetItemAction(Func<string> getItemId, string? anchor = null) : base(ActionType.Command, anchor)
    {
        this.getItemId = getItemId;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (!Context.TryToGetPlayer(out var player)) yield break;
        if (DataHandler.Find(getItemId(), out var item))
        {
            conversation.Hide();
            var showsPrompt = item.item.showPrompt == CollectableItem.PickUpPrompt.Always || (item.item.showPrompt == CollectableItem.PickUpPrompt.OnlyOnce && !item.item.hasShownPrompt);
            var c = player.StartCoroutine(item.PickUpRoutine());
            Context.levelUI.statusBar.ShowCollection(item.item).HideAndKill(1f);
            yield return c;
            if (!showsPrompt)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
internal class AddItemAction : BaseAction, IInvokableInAction
{
    internal readonly Func<string> getItemId;
    internal readonly Func<int> getAmount;
    public AddItemAction(Func<string> getItemId, Func<int> getAmount, string? anchor = null) : base(ActionType.Command, anchor)
    {
        this.getItemId = getItemId;
        this.getAmount = getAmount;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (DataHandler.Find(getItemId(), out var item))
        {
            conversation.Hide();
            DataHandler.AddCollected(item, getAmount());
            Context.levelUI.statusBar.ShowCollection(item.item).HideAndKill(1f);
            yield return new WaitForSeconds(1f);
        }
    }
}

