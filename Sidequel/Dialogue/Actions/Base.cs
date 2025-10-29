
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal abstract class BaseAction(ActionType type, string? anchor = null)
{
    internal readonly string? anchor = anchor;
    internal readonly ActionType type = type;
    protected static string? CurrentNodeId { get; private set; } = "";
    private static string i18nKeyPrefix = "";
    internal static void OnNodeStarted(Node node)
    {
        var id = CurrentNodeId = node.id;
        i18nKeyPrefix = id == null ? Const.I18n.NodePrefix : $"{Const.I18n.NodePrefix}.{id}";
    }
    protected static string I18n(string key) => I18n_.Localize($"{i18nKeyPrefix}.{key}");
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

