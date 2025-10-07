
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
            lines(1, 4, i => $"node.newgame{i:00}", Player),
            wait(1.5f),
            lines(5, 8, i => $"node.newgame{i:00}", Player),
            wait(1.5f),
            lines(9, 13, i => $"node.newgame{i:00}", Player),
            wait(1.5f),
            lines(14, 15, i => $"node.newgame{i:00}", Player),
            end(),
        ],
            condition: () => State.IsNewGame && !STags.GetBool(newGameNode),
            priority: int.MaxValue),
    ];
}

