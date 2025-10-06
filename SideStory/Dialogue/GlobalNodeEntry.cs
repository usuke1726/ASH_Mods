
namespace SideStory;

internal abstract class GlobalNodeEntry : Dialogue.NodeEntryBase
{
    private bool setupDone = false;
    sealed internal override void Setup()
    {
        if (setupDone) return;
        setupDone = true;
        var nodes = Nodes;
        foreach (var node in nodes)
        {
            Dialogue.NodeSelector.RegisterNode(node);
        }
    }
}

