
using ModdingAPI;

namespace SideStory;

internal abstract class GlobalNodeEntry : NodeEntry
{
    sealed protected override Characters? Character => null;
    private bool setupDone = false;
    internal override void Setup()
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

