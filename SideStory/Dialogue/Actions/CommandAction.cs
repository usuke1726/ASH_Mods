
using System.Collections;
using ModdingAPI;
using SideStory.Item;

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
    internal override IEnumerator Invoke(IConversation conversation)
    {
        if (hideBox) conversation.Hide();
        if (coroutine != null) yield return coroutine();
        else action?.Invoke();
    }
}

internal class GetItemAction : BaseAction
{
    internal readonly string itemId;
    public GetItemAction(string itemId, string? anchor = null) : base(ActionType.Command, anchor)
    {
        this.itemId = itemId;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
        if (!Context.TryToGetPlayer(out var player)) yield break;
        if (DataHandler.Find(itemId, out var item))
        {
            conversation.Hide();
            var c = player.StartCoroutine(item.PickUpRoutine());
            Context.levelUI.statusBar.ShowCollection(item).HideAndKill(1f);
            yield return c;
        }
    }
}
internal class AddItemAction : BaseAction
{
    internal readonly string itemId;
    internal readonly int amount;
    public AddItemAction(string itemId, int amount, string? anchor = null) : base(ActionType.Command, anchor)
    {
        this.itemId = itemId;
        this.amount = amount;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
        if (DataHandler.Find(itemId, out var item))
        {
            conversation.Hide();
            DataHandler.AddCollected(item, amount);
            Context.levelUI.statusBar.ShowCollection(item).HideAndKill(1f);
        }
        yield break;
    }
}

