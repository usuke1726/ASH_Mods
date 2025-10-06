
namespace SideStory;

internal abstract class StartNodeEntry : Dialogue.NodeEntryBase
{
    protected abstract string StartNode { get; }
    private bool setupDone = false;
    sealed internal override void Setup()
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

