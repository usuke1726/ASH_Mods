
using ModdingAPI;
using SideStory.Dialogue;
using SideStory.System;

namespace SideStory.NodeData;

internal class NewGame : NodeEntry
{
    protected override Characters? Character => null;
    private static readonly string newGameNode = "NewGame.StartedFirstNode";
    protected override Node[] Nodes => [
        new([
            tag(newGameNode, true),
            wait(2.0f),
            ..range(1, 4).Select(i => line($"node.newgame{i:00}", Player)),
            wait(1.5f),
            ..range(5, 8).Select(i => line($"node.newgame{i:00}", Player)),
            wait(1.5f),
            ..range(9, 13).Select(i => line($"node.newgame{i:00}", Player)),
            wait(1.5f),
            ..range(14, 15).Select(i => line($"node.newgame{i:00}", Player)),
            end(),
        ],
            condition: () => State.IsNewGame && !STags.GetBool(newGameNode),
            priority: int.MaxValue),
    ];
}

