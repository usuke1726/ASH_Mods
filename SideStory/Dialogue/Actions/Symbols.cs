
namespace SideStory.Dialogue.Actions;

internal class Anchor(string anchor) : BaseAction(ActionType.Anchor, anchor) { }
internal class NodeCompleteAction(string? anchor = null) : BaseAction(ActionType.NodeComplete, anchor) { }

