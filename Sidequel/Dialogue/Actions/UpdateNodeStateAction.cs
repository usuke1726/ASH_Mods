
using System.Collections;
using NodeStates = Sidequel.Dialogue.NodeEntryBase.NodeStates;

namespace Sidequel.Dialogue.Actions;

internal class UpdateNodeStateAction : BaseAction, IInvokableInAction
{
    private readonly string? nodeId;
    private readonly NodeStates state;
    public UpdateNodeStateAction(NodeStates state, string? anchor = null) : this(null!, state, anchor) { }
    public UpdateNodeStateAction(string nodeId, NodeStates state, string? anchor = null) : base(ActionType.NodeState, anchor)
    {
        this.nodeId = nodeId;
        this.state = state;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        var id = nodeId ?? CurrentNodeId;
        if (id != null)
        {
            Flags.SetNodeState(id, state);
        }
        yield break;
    }
}

