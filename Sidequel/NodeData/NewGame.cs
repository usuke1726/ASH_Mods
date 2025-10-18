
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;

namespace Sidequel.NodeData;

internal class NewGame : NodeEntry
{
    protected override Characters? Character => null;
    private static readonly string newGameNode = "NewGame.StartedFirstNode";
    protected override Node[] Nodes => [
        new("newgame", [
            tag(newGameNode, true),
            wait(2.0f),
            lines(1, 4, i => $"{i:00}", Player),
            wait(1.5f),
            lines(5, 8, i => $"{i:00}", Player),
            wait(1.5f),
            lines(9, 13, i => $"{i:00}", Player),
            wait(1.5f),
            lines(14, 15, i => $"{i:00}", Player),
            end(),
        ],
            condition: ShouldNewGameNodeStart,
            priority: int.MaxValue),
    ];
    internal static bool ShouldNewGameNodeStart()
    {
        return State.IsNewGame && !STags.GetBool(newGameNode);
    }
}

