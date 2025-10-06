
using ModdingAPI;

namespace SideStory;

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
}

