
using ModdingAPI;
using Sidequel.Dialogue.Actions;

namespace Sidequel;

internal abstract class NodeEntry : Dialogue.NodeEntryBase
{
    protected abstract Characters? Character { get; }
    private bool setupDone = false;
    sealed internal override void Setup()
    {
        if (setupDone) return;
        setupDone = true;
        var nodes = Nodes;
        var character = Character;
        foreach (var node in nodes)
        {
            Dialogue.NodeSelector.RegisterNode(character, node);
        }
    }
    protected void SetNext(string nodeId) => SetNext(nodeId, Character);
#pragma warning disable IDE1006
    protected CommandAction next(Func<string> getNodeId, string? anchor = null) => command(() => SetNext(getNodeId()), anchor: anchor);
#pragma warning restore IDE1006
}

