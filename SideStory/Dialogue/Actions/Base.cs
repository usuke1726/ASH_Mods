
using System.Collections;

namespace SideStory.Dialogue.Actions;

internal abstract class BaseAction(ActionType type, string? anchor = null)
{
    internal readonly string? anchor = anchor;
    internal readonly ActionType type = type;
    internal virtual IEnumerator Invoke(IConversation conversation) { yield break; }
}

