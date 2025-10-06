
using ModdingAPI;

namespace SideStory;

internal abstract class StartNodeEntry : NodeEntry
{
    protected abstract string StartNode { get; }
    sealed protected override Characters? Character => null;
    private bool setupDone = false;
    internal override void Setup()
    {
        if (setupDone) return;
        setupDone = true;
        var startNode = StartNode;
        var nodes = Nodes;
        foreach (var node in nodes)
        {
            Dialogue.NodeSelector.RegisterNodeFromStartNode(startNode, node);
        }
    }
}

